using UnityEngine;

internal static class ServerInfo
{
    public const string API = "https://api.sturfee.com/api/0.2.0";
    public const string WEBSOCKET = "wss://api.sturfee.com/api/0.2.0/alignment/ws";

    public const string VPSHD_API = "https://fullground.devsturfee.com/api/0.2.0";
    public const string VPSHD_WEBSOCKET = "wss://roll.devsturfee.com/api/0.2.0/alignment/ws";

    public static string SturfeeAPI
    {
        get
        {
            return PlayerPrefs.GetString("SturfeeVPS.Core.CustomApi.Api", API);
        }
    }

    public static string WebSocketServiceUrl
    {
        get
        {
            return PlayerPrefs.GetString("SturfeeVPS.Core.CustomApi.Websocket", WEBSOCKET);
        }
    }

}
