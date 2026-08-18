using Entities_DTOs;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace CoreApp
{
    // Genera archivos reales sin depender de una librería de escritorio o de Office.
    public class FileExportManager
    {
        private const int MaximumRows = 10000;
        private const int MaximumColumns = 50;
        private const int MaximumCellLength = 5000;

        public FileExportResult Generate(FileExportRequest request)
        {
            Validate(request);

            var format = NormalizeFormat(request.Format);
            var baseName = SanitizeFileName(request.FileName);
            var result = format switch
            {
                "CSV" => new FileExportResult
                {
                    Content = BuildCsv(request),
                    ContentType = "text/csv; charset=utf-8",
                    FileName = baseName + ".csv"
                },
                "XLSX" => new FileExportResult
                {
                    Content = BuildXlsx(request),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = baseName + ".xlsx"
                },
                "PDF" => new FileExportResult
                {
                    Content = BuildPdf(request),
                    ContentType = "application/pdf",
                    FileName = baseName + ".pdf"
                },
                _ => throw new BusinessException("El formato de exportación no es válido")
            };

            result.Format = format;
            result.RowCount = request.Rows.Count;
            return result;
        }

        private static void Validate(FileExportRequest request)
        {
            if (request == null) throw new BusinessException("La solicitud de exportación es obligatoria");
            if (string.IsNullOrWhiteSpace(request.Title)) throw new BusinessException("El título del archivo es obligatorio");
            if (request.Headers == null || request.Headers.Count == 0) throw new BusinessException("La exportación debe incluir encabezados");
            if (request.Headers.Count > MaximumColumns) throw new BusinessException($"La exportación admite un máximo de {MaximumColumns} columnas");
            if (request.Rows == null) throw new BusinessException("Las filas de la exportación son obligatorias");
            if (request.Rows.Count > MaximumRows) throw new BusinessException($"La exportación admite un máximo de {MaximumRows} filas");

            foreach (var header in request.Headers)
            {
                if (string.IsNullOrWhiteSpace(header)) throw new BusinessException("Los encabezados no pueden estar vacíos");
                if (header.Length > MaximumCellLength) throw new BusinessException("Un encabezado excede el tamaño permitido");
            }

            foreach (var row in request.Rows)
            {
                if (row == null || row.Count != request.Headers.Count)
                    throw new BusinessException("Todas las filas deben tener la misma cantidad de columnas que los encabezados");
                if (row.Any(cell => (cell?.Length ?? 0) > MaximumCellLength))
                    throw new BusinessException("Una celda excede el tamaño permitido");
            }
        }

        private static string NormalizeFormat(string? format)
        {
            var normalized = (format ?? "CSV").Trim().ToUpperInvariant();
            return normalized == "EXCEL" ? "XLSX" : normalized;
        }

        private static string SanitizeFileName(string? requested)
        {
            var value = string.IsNullOrWhiteSpace(requested) ? "exportacion_sgde" : Path.GetFileNameWithoutExtension(requested.Trim());
            var invalid = Path.GetInvalidFileNameChars();
            value = new string(value.Where(character => !invalid.Contains(character)).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(value) ? "exportacion_sgde" : value;
        }

        private static byte[] BuildCsv(FileExportRequest request)
        {
            static string Cell(string? value)
            {
                var text = value ?? string.Empty;
                if (text.Length > 0 && "=+-@".Contains(text[0])) text = "'" + text;
                return $"\"{text.Replace("\"", "\"\"")}\"";
            }

            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", request.Headers.Select(Cell)));
            foreach (var row in request.Rows)
            {
                builder.AppendLine(string.Join(",", row.Select(Cell)));
            }

            var bom = new UTF8Encoding(true).GetPreamble();
            return bom.Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        }

        private static byte[] BuildXlsx(FileExportRequest request)
        {
            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                AddEntry(archive, "[Content_Types].xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                      <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                      <Default Extension="xml" ContentType="application/xml"/>
                      <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                      <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                      <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                    </Types>
                    """);
                AddEntry(archive, "_rels/.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                    </Relationships>
                    """);
                AddEntry(archive, "xl/workbook.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                      <sheets><sheet name="Reporte" sheetId="1" r:id="rId1"/></sheets>
                    </workbook>
                    """);
                AddEntry(archive, "xl/_rels/workbook.xml.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                      <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                    </Relationships>
                    """);
                AddEntry(archive, "xl/styles.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                      <fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font></fonts>
                      <fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF1F4E78"/><bgColor indexed="64"/></patternFill></fill></fills>
                      <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
                      <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                      <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"/></cellXfs>
                    </styleSheet>
                    """);
                AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(request));
            }
            return output.ToArray();
        }

        private static string BuildWorksheetXml(FileExportRequest request)
        {
            var builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            var rows = new List<IReadOnlyList<string?>>() { request.Headers.Cast<string?>().ToList() };
            rows.AddRange(request.Rows);

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var excelRow = rowIndex + 1;
                builder.Append($"<row r=\"{excelRow}\">");
                for (var columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
                {
                    var reference = ColumnName(columnIndex + 1) + excelRow;
                    var style = rowIndex == 0 ? " s=\"1\"" : string.Empty;
                    var value = SecurityElement.Escape(rows[rowIndex][columnIndex] ?? string.Empty) ?? string.Empty;
                    builder.Append($"<c r=\"{reference}\" t=\"inlineStr\"{style}><is><t xml:space=\"preserve\">{value}</t></is></c>");
                }
                builder.Append("</row>");
            }

            builder.Append("</sheetData></worksheet>");
            return builder.ToString();
        }

        private static string ColumnName(int index)
        {
            var name = string.Empty;
            while (index > 0)
            {
                index--;
                name = (char)('A' + index % 26) + name;
                index /= 26;
            }
            return name;
        }

        private static void AddEntry(ZipArchive archive, string path, string content)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write(content.Trim());
        }

        private static byte[] BuildPdf(FileExportRequest request)
        {
            var isLandscape = request.Headers.Count > 6;
            var pageWidth = isLandscape ? 792f : 612f;
            var pageHeight = isLandscape ? 612f : 792f;
            const float margin = 36f;
            var printableWidth = pageWidth - (margin * 2);

            // Cálculo dinámico de ancho de columnas según contenido y encabezados
            var colWidths = CalculateColumnWidths(request.Headers, request.Rows, printableWidth);

            var pageBuilders = new List<StringBuilder>();
            var currentBuilder = new StringBuilder();
            pageBuilders.Add(currentBuilder);

            var y = pageHeight - margin;

            void RenderHeader(StringBuilder sb, bool isFirstPage)
            {
                // Banner superior de marca
                sb.AppendLine("0.122 0.306 0.471 rg"); // Primary Slate Blue (#1F4E78)
                sb.AppendLine($"36 {y - 4} {printableWidth} 4 re f");

                // Título y metadatos
                sb.AppendLine("BT");
                sb.AppendLine("/F2 8 Tf");
                sb.AppendLine("0.392 0.455 0.545 rg"); // Muted Gray (#64748B)
                sb.AppendLine($"36 {y - 16} Td");
                sb.AppendLine("(SGDE | SISTEMA DE GENERACION Y DESPACHO ELECTRICO) Tj");
                sb.AppendLine("ET");

                sb.AppendLine("BT");
                sb.AppendLine("/F2 14 Tf");
                sb.AppendLine("0.122 0.306 0.471 rg");
                sb.AppendLine($"36 {y - 34} Td");
                sb.AppendLine($"({PdfEscape(request.Title)}) Tj");
                sb.AppendLine("ET");

                sb.AppendLine("BT");
                sb.AppendLine("/F1 8.5 Tf");
                sb.AppendLine("0.392 0.455 0.545 rg");
                sb.AppendLine($"36 {y - 48} Td");
                var subText = $"Fecha de emision: {DateTime.Now:yyyy-MM-dd HH:mm}   |   Registros: {request.Rows.Count}";
                sb.AppendLine($"({PdfEscape(subText)}) Tj");
                sb.AppendLine("ET");

                // Línea divisoria
                sb.AppendLine("0.796 0.835 0.882 RG");
                sb.AppendLine("0.5 w");
                sb.AppendLine($"36 {y - 56} m {36 + printableWidth} {y - 56} l S");

                y -= 66;
            }

            void RenderTableHeader(StringBuilder sb)
            {
                const float headerHeight = 22f;
                // Fondo de la cabecera
                sb.AppendLine("0.122 0.306 0.471 rg");
                sb.AppendLine($"36 {y - headerHeight} {printableWidth} {headerHeight} re f");

                // Texto de encabezados (Blanco en negrita)
                sb.AppendLine("BT");
                sb.AppendLine("/F2 9 Tf");
                sb.AppendLine("1 1 1 rg");

                var curX = margin;
                for (var i = 0; i < request.Headers.Count; i++)
                {
                    var text = FitText(request.Headers[i], colWidths[i] - 10, 9f, true);
                    sb.AppendLine($"{curX + 5} {y - 15} Td");
                    sb.AppendLine($"({PdfEscape(text)}) Tj");
                    sb.AppendLine($"{- (curX + 5)} {- (y - 15)} Td");
                    curX += colWidths[i];
                }
                sb.AppendLine("ET");

                y -= headerHeight;
            }

            // Iniciar Primera Página
            RenderHeader(currentBuilder, true);
            RenderTableHeader(currentBuilder);

            const float rowHeight = 20f;
            const float bottomMarginThreshold = 50f;

            for (var rowIndex = 0; rowIndex < request.Rows.Count; rowIndex++)
            {
                // Verificar si cabe la siguiente fila; si no, crear nueva página
                if (y - rowHeight < bottomMarginThreshold)
                {
                    currentBuilder = new StringBuilder();
                    pageBuilders.Add(currentBuilder);
                    y = pageHeight - margin;
                    RenderHeader(currentBuilder, false);
                    RenderTableHeader(currentBuilder);
                }

                var row = request.Rows[rowIndex];

                // Fondo Zebra Striping
                if (rowIndex % 2 == 1)
                {
                    currentBuilder.AppendLine("0.973 0.980 0.988 rg"); // #F8FAFC
                    currentBuilder.AppendLine($"36 {y - rowHeight} {printableWidth} {rowHeight} re f");
                }

                // Borde inferior de fila
                currentBuilder.AppendLine("0.88 0.90 0.92 RG");
                currentBuilder.AppendLine("0.5 w");
                currentBuilder.AppendLine($"36 {y - rowHeight} m {36 + printableWidth} {y - rowHeight} l S");

                // Renderizar Texto de Celdas
                currentBuilder.AppendLine("BT");
                currentBuilder.AppendLine("/F1 8.5 Tf");
                currentBuilder.AppendLine("0.118 0.161 0.231 rg"); // #1E293B

                var cellX = margin;
                for (var colIndex = 0; colIndex < request.Headers.Count; colIndex++)
                {
                    var rawValue = colIndex < row.Count ? (row[colIndex] ?? string.Empty) : string.Empty;
                    var formattedValue = FitText(rawValue, colWidths[colIndex] - 10, 8.5f, false);

                    currentBuilder.AppendLine($"{cellX + 5} {y - 14} Td");
                    currentBuilder.AppendLine($"({PdfEscape(formattedValue)}) Tj");
                    currentBuilder.AppendLine($"{- (cellX + 5)} {- (y - 14)} Td");
                    cellX += colWidths[colIndex];
                }
                currentBuilder.AppendLine("ET");

                y -= rowHeight;
            }

            // Agregar Pie de Página en cada página
            var totalPages = pageBuilders.Count;
            for (var p = 0; p < totalPages; p++)
            {
                var sb = pageBuilders[p];
                sb.AppendLine("0.796 0.835 0.882 RG");
                sb.AppendLine("0.5 w");
                sb.AppendLine($"36 34 m {36 + printableWidth} 34 l S");

                sb.AppendLine("BT");
                sb.AppendLine("/F1 8 Tf");
                sb.AppendLine("0.392 0.455 0.545 rg");
                sb.AppendLine("36 22 Td");
                sb.AppendLine("(Documento generado oficialmente por SGDE - Sistema de Generacion y Despacho Eléctrico) Tj");
                sb.AppendLine("ET");

                sb.AppendLine("BT");
                sb.AppendLine("/F1 8 Tf");
                sb.AppendLine("0.392 0.455 0.545 rg");
                var pageStr = $"Pagina {p + 1} de {totalPages}";
                var pageStrX = 36 + printableWidth - (pageStr.Length * 5);
                sb.AppendLine($"{pageStrX} 22 Td");
                sb.AppendLine($"({PdfEscape(pageStr)}) Tj");
                sb.AppendLine("ET");
            }

            // Construcción del PDF
            var objects = new Dictionary<int, byte[]>();
            var pageObjectIds = new List<int>();
            var nextObjectId = 5; // 1: Catalog, 2: Pages, 3: Font Helvetica, 4: Font Helvetica-Bold

            foreach (var builder in pageBuilders)
            {
                var pageObjectId = nextObjectId++;
                var contentObjectId = nextObjectId++;
                pageObjectIds.Add(pageObjectId);

                var contentBytes = Encoding.ASCII.GetBytes(builder.ToString());
                objects[contentObjectId] = Combine(
                    Encoding.ASCII.GetBytes($"<< /Length {contentBytes.Length} >>\nstream\n"),
                    contentBytes,
                    Encoding.ASCII.GetBytes("endstream"));
                objects[pageObjectId] = Encoding.ASCII.GetBytes($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentObjectId} 0 R >>");
            }

            objects[1] = Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>");
            objects[2] = Encoding.ASCII.GetBytes($"<< /Type /Pages /Kids [{string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {pageObjectIds.Count} >>");
            objects[3] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
            objects[4] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

            using var output = new MemoryStream();
            WriteAscii(output, "%PDF-1.4\n%SGDE\n");
            var offsets = new long[nextObjectId];
            for (var id = 1; id < nextObjectId; id++)
            {
                offsets[id] = output.Position;
                WriteAscii(output, $"{id} 0 obj\n");
                output.Write(objects[id]);
                WriteAscii(output, "\nendobj\n");
            }

            var xrefOffset = output.Position;
            WriteAscii(output, $"xref\n0 {nextObjectId}\n0000000000 65535 f \n");
            for (var id = 1; id < nextObjectId; id++)
            {
                WriteAscii(output, $"{offsets[id]:D10} 00000 n \n");
            }
            WriteAscii(output, $"trailer\n<< /Size {nextObjectId} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
            return output.ToArray();
        }

        private static float[] CalculateColumnWidths(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string?>> rows, float printableWidth)
        {
            var count = headers.Count;
            if (count == 0) return Array.Empty<float>();

            var weights = new float[count];
            for (var i = 0; i < count; i++)
            {
                var headerLen = headers[i].Length;
                var maxContentLen = rows.Select(r => i < r.Count ? (r[i]?.Length ?? 0) : 0).DefaultIfEmpty(0).Max();
                weights[i] = Math.Max(5, Math.Max(headerLen, maxContentLen));
            }

            var totalWeight = weights.Sum();
            if (totalWeight <= 0) totalWeight = 1;

            var widths = new float[count];
            var minWidth = 40f;
            for (var i = 0; i < count; i++)
            {
                widths[i] = Math.Max(minWidth, (weights[i] / totalWeight) * printableWidth);
            }

            var currentSum = widths.Sum();
            if (currentSum > 0 && Math.Abs(currentSum - printableWidth) > 0.01f)
            {
                var factor = printableWidth / currentSum;
                for (var i = 0; i < count; i++) widths[i] *= factor;
            }

            return widths;
        }

        private static string FitText(string value, float maxWidth, float fontSize, bool isBold)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            // Ancho aproximado de carácter en Helvetica (en pt)
            var approxCharWidth = fontSize * (isBold ? 0.55f : 0.48f);
            var maxChars = Math.Max(1, (int)(maxWidth / approxCharWidth));

            if (value.Length <= maxChars) return value;
            if (maxChars <= 3) return value.Substring(0, maxChars);

            return value.Substring(0, maxChars - 3) + "...";
        }

        private static string PdfEscape(string value)
        {
            var builder = new StringBuilder();
            foreach (var character in value.Replace("\r", " ").Replace("\n", " "))
            {
                if (character is '(' or ')' or '\\') builder.Append('\\').Append(character);
                else if (character >= 32 && character <= 126) builder.Append(character);
                else if (character >= 160 && character <= 255) builder.Append('\\').Append(Convert.ToString(character, 8).PadLeft(3, '0'));
                else builder.Append('?');
            }
            return builder.ToString();
        }

        private static byte[] Combine(params byte[][] chunks)
        {
            using var stream = new MemoryStream();
            foreach (var chunk in chunks) stream.Write(chunk);
            return stream.ToArray();
        }

        private static void WriteAscii(Stream stream, string value)
        {
            stream.Write(Encoding.ASCII.GetBytes(value));
        }
    }
}
