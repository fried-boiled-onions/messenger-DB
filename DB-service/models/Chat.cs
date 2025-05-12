public class Chat
{
    public int Id {get; set;}
    public string Name {get; set;}
    public bool IsGroup {get;set;}
    public DateTime CreatedAt {get; set;}
}

public class ChatMember
{
    public int Id {get;set;}
    public int ChatId {get;set;}
    public int UserId {get;set;}
}