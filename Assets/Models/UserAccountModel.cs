using System;

[Serializable]
public class UserAccountModel
{
    public string userID { get; set; }
    public DateTime createdAt { get; set; }
    public string displayName { get; set; }
    public string email { get; set; }
    public string role { get; set; }


    public UserAccountModel() { }

    public UserAccountModel(string userID, DateTime createdAt, string role, string displayName, string email)
    {
        this.userID = userID;
        this.createdAt = createdAt;
        this.displayName = displayName;
        this.email = email;
        this.role = role;
    }
}
