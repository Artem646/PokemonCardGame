using Fusion;

public static class ConnectionConfig
{
    public static string RoomName { get; set; }
    public static GameMode Mode { get; set; } = GameMode.Shared;
    public static bool IsSpectator { get; set; } = false;
    public static bool IsLateSpectator = false;
}
