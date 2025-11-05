public class AuthFactory : IAuthFactory
{
    public IAuthProvider CreateAuthProvider(AuthType type)
    {
        switch (type)
        {
            case AuthType.Google: return new GoogleProvider();
            case AuthType.Anonymous: return new AnonymousProvider();
            default: throw new System.NotImplementedException();
        }
    }
}
