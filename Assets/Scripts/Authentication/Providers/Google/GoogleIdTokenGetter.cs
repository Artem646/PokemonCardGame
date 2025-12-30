using System.Threading.Tasks;
using UnityEngine;
using Google;

public static class GoogleIdTokenGetter
{
    private static GoogleSignInConfiguration configuration = new()
    {
        WebClientId = ConfigLoader.GetWebClientId(),
        RequestIdToken = true,
        RequestEmail = true
    };

    public static async Task<string> GetIdTokenAsync()
    {
        GoogleSignIn.Configuration = configuration;

        GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
        if (googleUser == null) return null;

        Debug.Log($"[GoogleSignInHelper] Успешный вход: {googleUser.Email}, idToken получен.");
        return googleUser.IdToken;
    }
}
