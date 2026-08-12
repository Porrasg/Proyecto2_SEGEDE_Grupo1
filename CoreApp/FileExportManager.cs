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
                _ => throw new Exception("El formato de exportación no es válido")
            };

            result.Format = format;
            result.RowCount = request.Rows.Count;
            return result;
        }

        private static void Validate(FileExportRequest request)
        {
            if (request == null) throw new Exception("La solicitud de exportación es obligatoria");
            if (string.IsNullOrWhiteSpace(request.Title)) throw new Exception("El título del archivo es obligatorio");
            if (request.Headers == null || request.Headers.Count == 0) throw new Exception("La exportación debe incluir encabezados");
            if (request.Headers.Count > MaximumColumns) throw new Exception($"La exportación admite un máximo de {MaximumColumns} columnas");
            if (request.Rows == null) throw new Exception("Las filas de la exportación son obligatorias");
            if (request.Rows.Count > MaximumRows) throw new Exception($"La exportación admite un máximo de {MaximumRows} filas");

            foreach (var header in request.Headers)
            {
                if (string.IsNullOrWhiteSpace(header)) throw new Exception("Los encabezados no pueden estar vacíos");
                if (header.Length > MaximumCellLength) throw new Exception("Un encabezado excede el tamaño permitido");
            }

            foreach (var row in request.Rows)
            {
                if (row == null || row.Count != request.Headers.Count)
                    throw new Exception("Todas las filas deben tener la misma cantidad de columnas que los encabezados");
                if (row.Any(cell => (cell?.Length ?? 0) > MaximumCellLength))
                    throw new Exception("Una celda excede el tamaño permitido");
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
            var lines = new List<string>
            {
                request.Title,
                $"Generado: {DateTime.Now:yyyy-MM-dd HH:mm}",
                string.Empty,
                string.Join(" | ", request.Headers),
                new string('-', Math.Min(105, request.Headers.Sum(header => header.Length + 3)))
            };

            foreach (var row in request.Rows)
            {
                lines.AddRange(WrapLine(string.Join(" | ", row.Select(cell => cell ?? string.Empty)), 105));
            }

            const int linesPerPage = 49;
            var pages = lines.Chunk(linesPerPage).ToList();
            if (pages.Count == 0) pages.Add(Array.Empty<string>());

            var objects = new Dictionary<int, byte[]>();
            var pageObjectIds = new List<int>();
            var nextObjectId = 4;

            foreach (var pageLines in pages)
            {
                var pageObjectId = nextObjectId++;
                var contentObjectId = nextObjectId++;
                pageObjectIds.Add(pageObjectId);

                var contentBuilder = new StringBuilder("BT\n/F1 9 Tf\n40 760 Td\n12 TL\n");
                foreach (var line in pageLines)
                {
                    contentBuilder.Append('(').Append(PdfEscape(line)).Append(") Tj\nT*\n");
                }
                contentBuilder.Append("ET\n");
                var contentBytes = Encoding.ASCII.GetBytes(contentBuilder.ToString());
                objects[contentObjectId] = Combine(
                    Encoding.ASCII.GetBytes($"<< /Length {contentBytes.Length} >>\nstream\n"),
                    contentBytes,
                    Encoding.ASCII.GetBytes("endstream"));
                objects[pageObjectId] = Encoding.ASCII.GetBytes($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObjectId} 0 R >>");
            }

            objects[1] = Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>");
            objects[2] = Encoding.ASCII.GetBytes($"<< /Type /Pages /Kids [{string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {pageObjectIds.Count} >>");
            objects[3] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Courier /Encoding /WinAnsiEncoding >>");

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

        private static IEnumerable<string> WrapLine(string value, int width)
        {
            if (string.IsNullOrEmpty(value)) return new[] { string.Empty };
            return Enumerable.Range(0, (value.Length + width - 1) / width)
                .Select(index => value.Substring(index * width, Math.Min(width, value.Length - index * width)));
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
