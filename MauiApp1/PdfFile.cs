public class PdfFile
{
    public int id { get; set; }
    public Folder folder { get; set; }
    public string fileName { get; set; }
    public string fileUrl { get; set; }
    public string webUrl { get; set; }
    public string webViewUrl { get; set; }
    public string? dataUrl { get; set; }
    public string? mimeType { get; set; }
    public string? formatInfo { get; set; }
    public string? fileSize { get; set; }

}
