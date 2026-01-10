using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseSignInController
{
    private readonly FirebaseAuth auth;

    public FirebaseSignInController()
    {
        auth = FirebaseAuthService.Instance.GetAuth();
    }

    public Task<FirebaseUser> SignInWithGoogleAsync(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        return auth.SignInWithCredentialAsync(credential);
    }

    public void SignOut()
    {
        try
        {
            auth.SignOut();
        }
        catch (Exception ex)
        {
            Debug.Log($"[P][AuthWithGoogleProvider.cs] Ошибка при выходе: {ex.Message}");
        }
    }

    public bool IsSignedIn => auth?.CurrentUser != null;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
}
