public interface IAuthFactory
{
    IAuthProvider CreateAuthProvider(AuthType type);
}

public enum AuthType
{
    Google,
    Anonymous,
    Unknown
}