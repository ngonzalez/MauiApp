public class AudioFile
{
    public int id { get; set; }
    public Folder folder { get; set; }
    public string fileName { get; set; }
    public string fileUrl { get; set; }
    public string aasmState { get; set; }
    public string? playlistUrl { get; set; }
    public string? dataUrl { get; set; }
    public string? mimeType { get; set; }
    public string? formatInfo { get; set; }
    public int? fileSize { get; set; }
    public string? title { get; set; }
    public float? length { get; set; }
    public int? bitrate { get; set; }
    public int? sampleRate { get; set; }
    public int? channels { get; set; }
    public string webUrl { get; set; }
    public string webViewUrl { get; set; }
}
