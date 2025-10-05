using System;

public class ImageFile
{
    public int id { get; set; }
    public Folder folder { get; set; }
    public string fileName { get; set; }
    public string fileUrl { get; set; }
    public string thumbUrl { get; set; }
    public string? dataUrl { get; set; }
    public string? mimeType { get; set; }
    public string? formatInfo { get; set; }
    public string? fileSize { get; set; }
    public int? width { get; set; }
    public int? height { get; set; }
    public string? dimensions { get; set; }
    public float? megapixels { get; set; }

}
