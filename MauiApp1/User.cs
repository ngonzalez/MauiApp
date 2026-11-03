public class User
{
    public int? id { get; set; }
    public Guid? accountUuid { get; set; }
    public string accountName { get; set; }
    public string accountAddress { get; set; }
    public string? accountSubdomain { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? emailAddress { get; set; }
    public string? emailAddressValidatedAt { get; set; }
    public bool deliverNotificationsSignIn { get; set; }
    public bool deliverNotificationsAccountUpdate { get; set; }
    // The picture's public address (it redirects to the image), null without one
    public string? avatarUrl { get; set; }
    public string[]? errors { get; set; }
    public DateTime? createdAt { get; set; }
    public DateTime? updatedAt { get; set; }

}
