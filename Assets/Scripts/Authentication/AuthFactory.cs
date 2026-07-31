public class AuthFactory : IAuthFactory
{
    public IAuthProvider CreateAuthProvider(AuthType type)
    {
        return type switch
        {
            AuthType.Google => new GoogleProvider(),
            AuthType.Email => new EmailProvider(),
            AuthType.Anonymous => new AnonymousProvider(),
            _ => throw new System.NotImplementedException(),
        };
    }
}
