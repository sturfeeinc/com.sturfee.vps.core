using UnityEngine;

internal static class ServerInfo
{
    private const string API = "https://api.sturfee.com/api/0.2.0";
    private const string WEBSOCKET = "wss://api.sturfee.com/api/0.2.0/alignment/ws";

    //private const string API = "https://blue-api.sturfee.com/api/0.2.0";
    //private const string WEBSOCKET = "wss://blue-api.sturfee.com/api/0.2.0/alignment/ws";

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
