public class Account
{
    public int? id { get; set; }
    public Guid? uuid { get; set; }
    public string? name { get; set; }
    public string? address { get; set; }
    public string? dataUrl { get; set; }
    public string[]? errors { get; set; }
    public DateTime? createdAt { get; set; }
    public DateTime? updatedAt { get; set; }

}
