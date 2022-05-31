using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SturfeeVPS.Core
{
    internal static class WebServices
    {
        public static readonly int NO_CONNECTION_TIMEOUT = 10;
        public static readonly int SLOW_INTERNET_TIMEOUT = 3;
        public static readonly int REQUEST_TIMEOUT = 3;
        public static readonly int REQUEST_TIMEOUT_BIG = 10;

        public static async Task CheckConnection(Action slowCallback)
        {
            SturfeeDebug.Log("Checking for connectivity...");

            UnityWebRequest www = UnityWebRequest.Get("https://google.com");
            www.timeout = NO_CONNECTION_TIMEOUT;

            //Slow connection timeout
            bool showSlowInternetText = false;
            float requestTime = Time.time;

            await www.SendWebRequest();

            while (!www.isDone)
            {
                if ((Time.time - requestTime) > SLOW_INTERNET_TIMEOUT)
                {
                    //Slow Internet detected
                    showSlowInternetText = true;
                }
                await Task.Yield();
            }

            if (string.IsNullOrEmpty(www.error))
            {
                if (showSlowInternetText)
                {
                    slowCallback();
                }

                SturfeeDebug.Log(" Connection established.");
            }
            else
            {
                Debug.LogError(www.error);
                throw new HttpException(www.responseCode, www.error);
            }
        }

        public static async Task ValidateToken(GeoLocation location, string accessToken)
        {
            SturfeeDebug.Log(" Validating token...");

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(ServerInfo.SturfeeAPI + "/status/" + "?accessToken=" + accessToken);
            unityWebRequest.timeout = REQUEST_TIMEOUT;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
            unityWebRequest.SetRequestHeader("latitude", location.Latitude.ToString());
            unityWebRequest.SetRequestHeader("longitude", location.Longitude.ToString());

            await unityWebRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);
            }

            SturfeeDebug.Log("Token check complete. Token is valid !");
        }

        public static async Task ValidateToken(string accessToken)
        {
            SturfeeDebug.Log(" Validating token...");

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(ServerInfo.SturfeeAPI + "/status/" + "?accessToken=" + accessToken);
            unityWebRequest.timeout = REQUEST_TIMEOUT;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);            

            await unityWebRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);
            }

            SturfeeDebug.Log("Token check complete. Token is valid !");
        }

        public static async Task CheckCoverage(GeoLocation location, string accessToken)
        {
            SturfeeDebug.Log(" Checking for localization coverage");

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(ServerInfo.SturfeeAPI + "/alignment_available/?lat=" + location.Latitude + "&lng=" + location.Longitude + "&token=" + accessToken);
            unityWebRequest.timeout = REQUEST_TIMEOUT;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
            unityWebRequest.SetRequestHeader("latitude", location.Latitude.ToString());
            unityWebRequest.SetRequestHeader("longitude", location.Longitude.ToString());

            await unityWebRequest.SendWebRequest();

            if(!string.IsNullOrEmpty(unityWebRequest.error))
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);
            }

            SturfeeDebug.Log("Localization available at this location");
        }

        public static async Task<byte[]> DownloadTiles(GeoLocation location, int radius, string accessToken)
        {
            string uri = ServerInfo.SturfeeAPI + "/get_gltf/?" +
                "lat=" + location.Latitude +
                "&lng=" + location.Longitude +
                "&radius=" + radius +
                "&token=" + accessToken;


            UnityWebRequest unityWebRequest = UnityWebRequest.Get(uri);
            unityWebRequest.timeout = REQUEST_TIMEOUT_BIG;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
            unityWebRequest.SetRequestHeader("latitude", location.Latitude.ToString());
            unityWebRequest.SetRequestHeader("longitude", location.Longitude.ToString());

            await unityWebRequest.SendWebRequest();

            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                var filebytes = unityWebRequest.downloadHandler.data;
                return filebytes;                
            }
            else
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);                
            }
        }

        

       


    }
}
