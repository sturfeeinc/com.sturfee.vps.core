using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SturfeeVPS.Core
{

    public class SturfeeSubscriptionInfo
    {
        public int Tier;
    }

    internal class SturfeeTokenInfo
    {
        public string jti;
        public long iat;
        public string iss;
        public string accountId;
        public string scope;
    }

    /// <summary>
    /// Provides some methods to check the Sturfee subscription details
    /// </summary>
    public static class SturfeeSubscriptionManager
    {

        /// <summary>
        /// Returns the subscription details for the provided token.
        /// </summary>
        /// <param name="accessToken">Access Token to check</param>
        /// <returns>SturfeeSubscriptionInfo object</returns>
        public static SturfeeSubscriptionInfo GetSubscriptionInfo(string accessToken)
        {
            return SubscriptionHelper.GetSubscriptionInfo(accessToken);
        }

        /// <summary>
        /// Validates the subscription details with the server
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="callback"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        public static UnityWebRequest GetSubscription(string accessToken, Action<SturfeeSubscriptionInfo> callback, Action<string> errorCallback = null)
        {
            return SubscriptionHelper.Request(accessToken, callback, errorCallback);
        }

        public static void OnResponse()
        {
            SubscriptionHelper.OnResponse();
        }
    }

    internal class SubscriptionHelper
    {
        public static SturfeeSubscriptionInfo Result;

        private static string _url = ServerInfo.SturfeeAPI + "/status/";
        private static UnityWebRequest _www;
        private static Action<SturfeeSubscriptionInfo> _callback;
        private static Action<string> _errorCallback;
        private static string _accessToken = string.Empty;
        private static bool _inErrorState = false;
        private static bool _isFinished = false;

        public static UnityWebRequest Request(string accessToken, Action<SturfeeSubscriptionInfo> callback, Action<string> errorCallback = null)
        {
            _callback = callback;
            _errorCallback = errorCallback;
            _accessToken = accessToken;

            _url = ServerInfo.SturfeeAPI + "/status/";
            _www = UnityWebRequest.Get(_url + "?token=" + accessToken);// WWW.EscapeURL(accessToken));//);
            _www.timeout = 3;
            _www.SetRequestHeader("Authorization", "Bearer " + _accessToken);
            _www.SendWebRequest();

            _inErrorState = false;
            _isFinished = false;

            return _www;
        }

        public static SturfeeSubscriptionInfo GetSubscriptionInfo(string accessToken)
        {
            try
            {
                //var claims = tokenInfo.scope.Split(',');
                //var tier = int.Parse(tokenInfo.scope.Split(':')[1]);
                var tier = 3;

                return new SturfeeSubscriptionInfo
                {
                    Tier = tier
                };

            }
            catch (Exception e)
            {
                throw new Exception("INVALID TOKEN");
            }
        }

        public static void OnError()
        {

        }

        public static void OnResponse()
        {
            if (_inErrorState || _isFinished)
            {
                return;
            }

            EditorUpdate();
        }

        static void EditorUpdate()
        {
            if (_www.isDone)
            {
                //string reply = _www.downloadHandler.text;

                if (_www.responseCode == 200)
                {
                    _isFinished = true;
                    Result = GetSubscriptionInfo(_accessToken);

                    if (_callback != null)
                    {
                        _callback(Result);
                    }
                }
                else if (_www.responseCode == 403)
                {
                    SturfeeDebug.LogError("Invalid Token");
                    _inErrorState = true;

                    _errorCallback?.Invoke(_www.error);
                    _callback?.Invoke(new SturfeeSubscriptionInfo { Tier = 0 });

                }
                else// if (!string.IsNullOrEmpty(_www.error))
                {
                    SturfeeDebug.LogError("Server Error: Sturfee Subscription (" + _www.error + ")");
                    _inErrorState = true;
                    if (_errorCallback != null)
                    {
                        _errorCallback(_www.error);
                    }
                }

                return;
            }
        }


    }


}
