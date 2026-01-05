public enum NotificationKey
{
    DeckAdded, DeckUpdated, DeckDeleted,
    NoChanges, DeckMustHaveCountCards,

    ProfileUpdated,

    СlashWithWinnerInBotBattle, ClashWithTieInBotBattle,
    СlashWithWinnerInNetworkBattle, ClashWithTieInNetworkBattle,

    AnonymousSingInSuccess, GoogleSingInSuccess,
    SingOutError, GoogleSingOutFailed,

    GoogleNetworkError, GoogleInternalError, GoogleApiNotConnected,
    GoogleInvalidAccount, GoogleTimeout, GoogleDeveloperError,
    GoogleCanceled, GoogleInterrupted, GoogleError, GoogleUnknown,
    GoogleTokenError, GoogleDataProcessingError,

    FirebaseAuthError, FirebaseUserDataError,
    FirebaseAuthCanceled, FirebaseAuthSuccess,

    AnonymousAuthError, AnonymousUserDataError,
    AnonymousAuthCanceled, AnonymousAuthSuccess
}
