public class UploadFile
{ 
    public required Guid uuid { get; set; }
    public required int sessionId { get; set; }
    public required string itemData { get; set; }
    public required string filePath { get; set; }
    public required string mimeType { get; set; }
    public required DateTime createdAt { get; set; }
    public required DateTime updatedAt { get; set; }
    public required string source { get; set; }

}