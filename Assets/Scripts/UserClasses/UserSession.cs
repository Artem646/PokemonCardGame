public class UserSession
{
    private static UserSession _instance;
    public static UserSession Instance => _instance ??= new UserSession();

    public User ActiveUser { get; set; }

    public void ClearActiveUser() => ActiveUser = null;
}