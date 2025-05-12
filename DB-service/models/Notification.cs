public class Notification
{
    public int Id {get;set;}
    public int UserId {get;set;}
    public string Type {get;set;}
    public string Data {get;set;}
    public DateTime CreatedAt {get;set;}
    public bool IsSeen {get;set;}
}