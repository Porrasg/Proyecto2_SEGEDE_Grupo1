namespace Entities_DTOs
{
    // Contrato común para exportar tablas desde cualquier módulo del sistema.
    public class FileExportRequest
    {
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Format { get; set; } = "CSV";
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<string?>> Rows { get; set; } = new List<List<string?>>();
    }

    public class FileExportResult
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int RowCount { get; set; }
    }
}
