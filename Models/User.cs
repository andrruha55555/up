namespace ApiUp.Models;

public class User
{
    public int id { get; set; }
    public string login { get; set; }
    public string password_hash { get; set; }
    public string role { get; set; } 
    public string email { get; set; }
    public string last_name { get; set; }
    public string first_name { get; set; }
    public string middle_name { get; set; }
    public string phone { get; set; }
    public string address { get; set; }
}
