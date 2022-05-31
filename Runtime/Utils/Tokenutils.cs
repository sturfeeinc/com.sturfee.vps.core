using System;
using System.Text;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public class TokenUtils : MonoBehaviour
    {
        public static string GetAccessToken()
        {
            TextAsset configurationTextAsset = Resources.Load<TextAsset>(Paths.SturfeeResourcesRelative);// _configurationFile);// Paths.SturfeeResourcesAbsolute);

            if(configurationTextAsset != null)
            {
                return JsonUtility.FromJson<EditorConfiguration>(configurationTextAsset.text).AccessToken;
            }

            return null;
        }

        public static TokenRegion GetTokenRegion()
        {
            try
            {
                var tokenInfo =  DecodeToken(GetAccessToken());
                switch (tokenInfo.region)
                {
                    case "Japan": return TokenRegion.Japan;
                    case "Not_Japan": return TokenRegion.NonJapan;
                    
                    default: return TokenRegion.All;
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Token in Sturfee Configure Window is incorrect. Please check the token");
            }

            return TokenRegion.Error;
        }

        private static TokenInfo DecodeToken(string accessToken)
        {
            try
            {
                var jwtParts = accessToken.Split('.');
                string fixedJwtPart = jwtParts[1];

                // handle padding base64 string
                if (jwtParts[1].Length % 4 != 0)
                {
                    var pad = "";
                    for (var i = 0; i < (4 - jwtParts[1].Length % 4); i++)
                    {
                        pad += "=";
                    }
                    fixedJwtPart = jwtParts[1] + pad;
                }

                var decodedBytes = Convert.FromBase64String(fixedJwtPart);
                string decodedString = Encoding.UTF8.GetString(decodedBytes);

                var tokenInfo = JsonUtility.FromJson<TokenInfo>(decodedString);

                return tokenInfo;
            }
            catch (Exception e)
            {
                throw new Exception("INVALID TOKEN");
            }
        }
    }

    public enum TokenRegion
    {
        Japan,
        NonJapan,
        All,
        Error
    }

    [Serializable]
    internal class TokenInfo
    {
        public string region;
        public string jti;
        public string iat;
        public string iss;
        public string version;
    }
}

