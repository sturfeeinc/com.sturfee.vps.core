using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public class LocalizationService
    {
        public static readonly string STATUS_SUCCESS = "success";
        public static readonly string STATUS_OK = "ok";

        private DateTime _requestTimestamp;
        private string _language = "en-US";

        public LocalizationService() { }

        public LocalizationService(string url)
        {
            _uri = new Uri(url);
        }

        public LocalizationService(string url, string accessToken, string language = "en-US")
        {
            _accessToken = accessToken;
            _uri = new Uri(url);
            _language = language;
        }        

        public void Connect(double latitude, double longitude, Action callback, Action<string> errorCallback)
        {
            Mono.StartCoroutine(ConnectAsync(latitude, longitude, callback, errorCallback));
        }

        public async Task Connect(double latitude, double longitude)
        {
            SturfeeDebug.Log($"Opening socket connection at {Uri} \t Latitude : {latitude} , Longitude : {longitude}");

            _socket = new WebSocket(Uri);
            var authHeaders = new Dictionary<string, string> {
                        {"Authorization", "Bearer " + AccessToken},
                        {"latitude", latitude.ToString()},
                        {"longitude", longitude.ToString()},
                        {"Accept-Language", _language}
                    };

            Mono.StartCoroutine(_socket.Connect(authHeaders));

            while (!_socket.IsConnected)
            {
                if (!string.IsNullOrEmpty(_socket.error))
                {
                    throw new Exception(_socket.error);
                }

                // if socket is closed by server before opening the connection
                if (_socket.IsClosed())
                {
                    throw new Exception("Socket connection closed by server");
                }

                await Task.Yield();
            }

            SturfeeDebug.Log("Socket connection active");

        }

        public void Close()
        {
            SturfeeDebug.Log(" Closing socket connection");
            Socket.Close();
        }

        public void Align(byte[] buffer, Action< byte[]> callback, Action<string> errorCallback)
        {
            Mono.StartCoroutine(AlignAsync(buffer, callback, errorCallback));
        }

        public bool IsConnected
        {
            get
            {
                if(_socket == null)
                {
                    return false;
                }
                return _socket.IsConnected;
            }
        }

        private IEnumerator ConnectAsync(double latitude, double longitude, Action callback, Action<string> errorCallback)
        {
            SturfeeDebug.Log("Opening socket connection at " + Uri.ToString(), false);

            _socket = new WebSocket(Uri);
            var authHeaders = new Dictionary<string, string> {
                        {"Authorization", "Bearer " + AccessToken},
                        {"latitude", latitude.ToString()},
                        {"longitude", longitude.ToString()},
                        {"Accept-Language", _language}
                    };

            Mono.StartCoroutine(_socket.Connect(authHeaders));

            while (!_socket.IsConnected)
            {
                if (!string.IsNullOrEmpty(_socket.error))
                {
                    errorCallback(_socket.error);
                    yield break;
                }

                yield return null;
            }

            callback();
        }

        private IEnumerator AlignAsync(byte[] buffer, Action<byte[]> callback, Action<string> errorCallback)
        {
            // Wait till the socket connection is open
            while (!Socket.IsConnected)
            {
                //SturfeeDebug.Log(" Waiting for socket connection to open...");
                yield return null;
            }
            try
            {
                if(buffer == null) { SturfeeDebug.Log("Send Buffer is NULL"); }
                Socket.Send(buffer, (success) =>
                {
                    if (success)
                    {
                        _requestTimestamp = DateTime.Now;
                    }
                });
            }
            catch (Exception e)
            {
                // Connection was established but some exception closed the socket
                SturfeeDebug.Log("Socket connection closed unexpectedly. Re-opening the connection...");

                _socket = null;     // Next reference to "Socket" will create new connection

                // Establish socket connection again and retry the same call 
                Mono.StartCoroutine(AlignAsync(buffer, callback, errorCallback));

                // No need to further wait for this coroutine
                yield break;
            }

            bool isDone = false;

            while (!isDone)
            {
                byte[] response = Socket.Recv();

                // We got response
                if (response != null)
                {
                    callback(response);
                    SetLatency();
                    isDone = true;
                }

                // Error
                if (!string.IsNullOrEmpty(Socket.error))
                {
                    errorCallback(Socket.error);
                    isDone = true;
                }

                // If reply is null but no error, we wait !
                yield return null;
            }
        }

        private void SetLatency()
        {
            DateTime responseTimestamp = DateTime.Now;
            double latency = (responseTimestamp - _requestTimestamp).TotalSeconds;
            SturfeeDebug.Log(" Latency : " + latency, false);
            //Info info = SessionInfo.Info;
            //info.Latency = latency;
            //SessionInfo.Info = info;
        }

        private WebSocket _socket;
        private WebSocket Socket
        {
            get
            {
                if (_socket == null)
                {
                    _socket = new WebSocket(Uri);
                    var authHeaders = new Dictionary<string, string> {
                        {"Authorization", "Bearer " +  AccessToken}
                    };

                    Mono.StartCoroutine(_socket.Connect(authHeaders));
                }
                return _socket;
            }
        }

        private Uri _uri;
        private Uri Uri
        {
            get
            {
                if (_uri == null)
                {
                    _uri = new Uri(ServerInfo.WebSocketServiceUrl);
                    SturfeeDebug.Log("Connecting to " + _uri, false);
                }

                return _uri;
            }
        }

        private string _accessToken;
        private string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    //_accessToken = XRSessionManager.GetSession().Config.AccessToken;
                    _accessToken = "sturfee_debug";
                }

                return _accessToken;
            }
        }

        private MonoBehaviour _mono;
        private MonoBehaviour Mono
        {
            get
            {
                if (_mono == null)
                {
                    _mono = XRSessionManager.GetSession().GetSessionGO().GetComponent<MonoBehaviour>();

                    // To be used when called without session creation (mostly for testing purposes)
                    if (_mono == null)
                    {
                        _mono = new GameObject("CoroutineHelper").AddComponent<XRSessionBehaviour>().GetComponent<MonoBehaviour>();
                    }
                }
                return _mono;
            }
        }
    }
}
