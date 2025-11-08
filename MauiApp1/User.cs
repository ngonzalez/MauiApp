public class User
{
    public int? id { get; set; }
    public Guid? uuid { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? emailAddress { get; set; }
    public bool? deliverNotificationsSignIn { get; set; }
    public bool? deliverNotificationsAccountUpdate { get; set; }
    public string[]? errors { get; set; }
    public DateTime? createdAt { get; set; }
    public DateTime? updatedAt { get; set; }
}