using System.IO.Compression;
using System.Text;
using CoreApp;
using Entities_DTOs;
using Xunit;

namespace CoreApp.Tests;

public class FileExportManagerTests
{
    [Fact]
    public void Generate_Csv_ProducesBomAndNeutralizesSpreadsheetFormula()
    {
        var result = new FileExportManager().Generate(Request("CSV", "=2+2"));

        Assert.Equal("text/csv; charset=utf-8", result.ContentType);
        Assert.Equal("reporte_prueba.csv", result.FileName);
        Assert.Equal(Encoding.UTF8.GetPreamble(), result.Content.Take(3));
        Assert.Contains("'=2+2", Encoding.UTF8.GetString(result.Content));
    }

    [Fact]
    public void Generate_Xlsx_ProducesRequiredWorkbookEntries()
    {
        var result = new FileExportManager().Generate(Request("XLSX", "dato"));

        using var archive = new ZipArchive(new MemoryStream(result.Content), ZipArchiveMode.Read);
        Assert.NotNull(archive.GetEntry("xl/workbook.xml"));
        Assert.NotNull(archive.GetEntry("xl/worksheets/sheet1.xml"));
        Assert.Equal("reporte_prueba.xlsx", result.FileName);
    }

    [Fact]
    public void Generate_Pdf_ProducesPdfSignature()
    {
        var result = new FileExportManager().Generate(Request("PDF", "dato"));

        Assert.Equal("%PDF-", Encoding.ASCII.GetString(result.Content, 0, 5));
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("reporte_prueba.pdf", result.FileName);
    }

    private static FileExportRequest Request(string format, string value) => new()
    {
        Title = "Prueba",
        FileName = "reporte_prueba",
        Format = format,
        Headers = ["Columna"],
        Rows = [[value]]
    };
}
