using System;
using System.Collections.Generic;

public static class ErrorMessages
{
    #region Session
    public static (string, string) NoConnectivity = ("NoConnectivity", "No connectivity. Check Internet !");
    public static (string, string) SlowInternet = ("SlowInternet", "\tSlow internet connection detected...\n " +
                                " Application may not work properly.");

    public static (string, string) TokenNotAvailable = ("TokenNotAvailable", "Missing Token");
    public static (string, string) NotAuthorizedToken = ("NotAuthorizedToken", "Unauthorized Token");

    public static (string, string) NoCoverageArea = ("NoCoverageArea", "VPS not available at this location");

    public static (string, string) TileLoadingError = ("TileLoadingError", "Tile loading error");
    public static (string, string) TileLoadingErrorFromCache = ("TileLoadingErrorFromCache", "Tile loading error. Center Reference NUll in cache");
    public static (string, string) TileDownloadingError = ("TileDownloadingError", "Tile downloading error");
    public static (string, string) SocketConnectionFail = ("SocketConnectionFail", "Socket connection failed");
    public static (string, string) SessionNotReady = ("SessionNotReady", "Session is not ready or session creation failed");

    #endregion

    #region Localization
    public static (string, string) PitchError = ("PitchError", "Please look forward while scanning");
    public static (string, string) RollError = ("RollError", "Please do not tilt your phone while scannning");
    public static (string, string) YawError = ("YawError", "IMU Error");
    public static (string, string) ServerError = ("ServerError", "Internal Server Error");
    public static (string, string) ScanError = ("ScanError", " Scan failed. Please try again !");
    #endregion

    #region Http
    public static (string, string) Error400 = ("Error400", "Invalid URL request parameters");
    public static (string, string) Error403 = ("Error403", "Invalid Token !");
    public static (string, string) Error500 = ("Error500", "Server Connection Failed. Please try again");

    public static (string, string) HttpErrorGeneric = ("HttpErrorGeneric", "HTTP Connection Error. Please try again !");
    #endregion


}


