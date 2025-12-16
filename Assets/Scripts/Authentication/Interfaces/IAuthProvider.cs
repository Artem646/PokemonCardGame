public interface IAuthProvider
{
    void SignIn();
    void SignUp();
    void SignOut();
    bool IsSignedIn();
    string GetUserId();
}