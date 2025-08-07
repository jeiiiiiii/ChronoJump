using System;

[Serializable]
public class UserAccountModel
{
    public string userID { get; set; }
    public string username { get; set; }
    public string passwordHash { get; set; }
    public string role { get; set; }
    public string email { get; set; }
    public string createdAt { get; set; }

    public UserAccountModel() { }

    public UserAccountModel(string userID, string username, string passwordHash, string role, string email, string createdAt)
    {
        this.userID = userID;
        this.username = username;
        this.passwordHash = passwordHash;
        this.role = role;
        this.email = email;
        this.createdAt = createdAt;
    }
}
