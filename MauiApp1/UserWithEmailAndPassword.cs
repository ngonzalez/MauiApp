using System;

public class UserWithEmailAndPassword
{
    public int id { get; set; }
    public required Guid uuid { get; set; }
    public string emailAddress { get; set; }
    public string password { get; set; }
    //public [string] errors { get; set; }
}
