public interface IAuthProvider
{
    void SignIn();
    void SignOut();
    bool IsSignedIn();
    string GetUserId();
}