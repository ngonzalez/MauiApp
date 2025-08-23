using System;

public class User
{
    public int id { get; set; }
    public required Guid uuid { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string emailAddress { get; set; }
    public string password { get; set; }
    public string createdAt { get; set; }
    public string updatedAt { get; set; }
    //public [string] errors { get; set; }
}
