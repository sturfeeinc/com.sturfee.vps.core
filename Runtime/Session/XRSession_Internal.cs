using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace SturfeeVPS.Core
{
    internal class XRSession_Internal
    {
        public readonly int SESSION_TIMEOUT = 10;

        public XRSessionStatus Status { internal set; get; }

        internal LocalizationManager LocalizationManager;
        internal XRSessionConfig Config;
        internal GeoLocation CenterRef;
        internal string SessionFail;

        private string _token;
        private GameObject _sessionGO;
        
        private bool _tokenValidated;
        private bool _coverageChecked;
        private bool _tilesLoaded;

        private CancellationTokenSource _preScanCTS;

        private IGpsProvider _gpsProvider;
        private IPoseProvider _poseProvider;
        private IVideoProvider _videProvider;

        public IGpsProvider GpsProvider
        {
            get
            {
                if(_gpsProvider == null)
                {
                    _gpsProvider = Config.GpsProvider;
                    _gpsProvider.Initialize();
                }
                return _gpsProvider; 
            }
        }

        public IPoseProvider PoseProvider
        {
            get
            {
                if (_poseProvider == null)
                {
                    _poseProvider = Config.PoseProvider;
                    _poseProvider.Initialize();
                }
                return _poseProvider;
            }
        }

        public IVideoProvider VideoProvider
        {
            get
            {
                if (_videProvider == null)
                {
                    _videProvider = Config.VideoProvider;
                    _videProvider.Initialize();
                }
                return _videProvider;
            }
        }

        public XRSession_Internal(XRSessionConfig config)
        {
            LoadSessionConfig(config);
            Config = config;
            _token = config.AccessToken;
            LocalizationManager = new LocalizationManager();
        }

        public async Task CreateSession(CancellationToken ct)
        {
            await Task.Yield();
            SturfeeEventManager.Instance.SessionInitializing(); 
            try
            {
                Uri uri = new Uri(ServerInfo.SturfeeAPI);
                SturfeeDebug.Log("Pointing to endpoint : " + uri.Host);

                await WaitForGps(ct);
                await CheckConnection(); 

                var location = GpsProvider.GetCurrentLocation();
                SturfeeDebug.Log("Start Location : " + location.ToFormattedString());

                await ValidateToken(location);
                await CheckCoverage(location);
                await LoadTiles(location, ct);

                //await LocalizationManager.OpenWebsocketConnection(location);

                await WaitForVideo(ct);
                await WaitForPose(ct);

                SturfeeDebug.Log("Session ready");
                SturfeeEventManager.Instance.SessionIsReady();

            }
            catch(SessionException e)
            {
                SturfeeDebug.LogError(e.Message);
                Status = XRSessionStatus.NotCreated;
                SessionFail = e.Message;
                throw e;
            }
            catch (Exception e)
            {
                SturfeeDebug.LogError(e.Message);
                Status = XRSessionStatus.NotCreated;
                SessionFail = e.Message;
                throw e;
            }
        }

        public void DestroySession()
        {
            _preScanCTS?.Cancel();
        }

        public GeoLocation GetXRCameraLocation()
        {
            return LocalizationManager.GetXRCameraLocation();
        }

        public Quaternion GetXRCameraOrientation()
        {
            return LocalizationManager.GetXRCameraOrientation();
        }

        public Vector3 GetLocationOffset()
        {
            return LocalizationManager.GetLocationOffset();
        }

        public Quaternion GetOrientationOffset()
        {
            return LocalizationManager.GetOrientationOffset();
        }        

        public async void EnableVPS()
        {
            await PrepareForScan();
        }

        public void PerformLocalization(ScanType scanType, LocalizationMode localizationMode = LocalizationMode.WebServer)
        {
            LocalizationManager.PerformLocalization(scanType, localizationMode);
        }

        public void CancelVPS()
        {
            if (Status <= XRSessionStatus.Ready)
            {
                _preScanCTS?.Cancel();
                LocalizationManager.CancelScan();
            }
            else if (Status == XRSessionStatus.Scanning || Status == XRSessionStatus.Loading)
            {
                Status = XRSessionStatus.Ready;
                LocalizationManager.StopScan();
            }
        }

        public void DisableVPS()
        {
            LocalizationManager.DisableVPS();
            Status = XRSessionStatus.Ready;
        }

        private async Task WaitForGps(CancellationToken ct = default)
        {
            float start = Time.time;
            var gpsProvider = GpsProvider;

            if(gpsProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                return;
            }

            gpsProvider.Initialize();

            while (gpsProvider.GetProviderStatus() != ProviderStatus.Ready)
            {
                ct.ThrowIfCancellationRequested();

                if (gpsProvider.GetProviderStatus() == ProviderStatus.NotSupported)
                {
                    throw new SessionException(ErrorMessages.GpsProviderNotSupported);
                }

                if (gpsProvider.GetProviderStatus() == ProviderStatus.Stopped)
                {
                    SturfeeDebug.LogError("GpsProvider stopped !");
                    throw new SessionException(ErrorMessages.GpsProviderNotSupported);
                }

                if(Time.time - start > SESSION_TIMEOUT)
                {
                    throw new SessionException(ErrorMessages.GpsProviderTimeout);
                }

                await Task.Yield();
            }
        }

        private async Task WaitForPose(CancellationToken ct = default)
        {
            float start = Time.time;

            if (PoseProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                return;
            }

            while (PoseProvider.GetProviderStatus() != ProviderStatus.Ready)
            {
                ct.ThrowIfCancellationRequested();

                if (PoseProvider.GetProviderStatus() == ProviderStatus.NotSupported)
                {
                    throw new SessionException(ErrorMessages.PoseProviderNotSupported);
                }

                if (PoseProvider.GetProviderStatus() == ProviderStatus.Stopped)
                {
                    SturfeeDebug.LogError("PoseProvider stopped !");
                    throw new SessionException(ErrorMessages.PoseProviderNotSupported);
                }

                if (Time.time - start > SESSION_TIMEOUT)
                {
                    throw new SessionException(ErrorMessages.PoseProviderTimeout);
                }

                await Task.Yield();
            }
        }

        private async Task WaitForVideo(CancellationToken ct = default)
        {
            float start = Time.time;

            if (VideoProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                return;
            }

            while (VideoProvider.GetProviderStatus() != ProviderStatus.Ready)
            {
                ct.ThrowIfCancellationRequested();

                if (VideoProvider.GetProviderStatus() == ProviderStatus.NotSupported)
                {
                    throw new SessionException(ErrorMessages.VideoProviderNotSupported);
                }

                if (VideoProvider.GetProviderStatus() == ProviderStatus.Stopped)
                {
                    SturfeeDebug.LogError("VideoProvider stopped !");
                    throw new SessionException(ErrorMessages.VideoProviderNotSupported);
                }

                if (Time.time - start > SESSION_TIMEOUT)
                {
                    throw new SessionException(ErrorMessages.VideoProviderTimeout);
                }

                await Task.Yield();
            }
        }

        private async Task CheckConnection()
        {
            try
            {
                await WebServices.CheckConnection(() =>
                {
                    SturfeeDebug.LogWarning(ErrorMessages.SlowInternet.Item2);
                });                
            }
            catch(HttpException)
            {
                throw new SessionException(ErrorMessages.NoConnectivity);
            }            
        }

        public async Task ValidateToken(GeoLocation location = null)
        {
            if (string.IsNullOrEmpty(_token))
            {
                throw new SessionException(ErrorMessages.TokenNotAvailable);
            }

            if (!_tokenValidated)
            {
                if (location == null)
                {
                    location = GpsProvider.GetCurrentLocation();
                }
                try
                {
                    await WebServices.ValidateToken(location, _token);
                    _tokenValidated = true;
                }
                catch(HttpException e)
                {
                    switch (e.ErrorCode)
                    {                        
                        case 501:
                            throw new SessionException(ErrorMessages.NoCoverageArea);
                        case 400:
                            throw new SessionException(ErrorMessages.Error400);
                        case 403:
                            throw new SessionException(ErrorMessages.NotAuthorizedToken);
                        case 500:
                            throw new SessionException(ErrorMessages.Error500);
                        default:
                            throw new SessionException(ErrorMessages.HttpErrorGeneric);
                    }
                }
            }
        }

        public async Task CheckCoverage(GeoLocation location = null)
        {
            if (!_coverageChecked)
            {
                if (location == null)
                {
                    location = GpsProvider.GetCurrentLocation();
                }

                try
                {
                    await WebServices.CheckCoverage(location, _token);
                    _coverageChecked = true;
                }
                catch(HttpException e)
                {
                    throw e.ErrorCode switch
                    {
                        501 => new SessionException(ErrorMessages.NoCoverageArea),
                        400 => new SessionException(ErrorMessages.Error400),
                        403 => new SessionException(ErrorMessages.Error403),
                        500 => new SessionException(ErrorMessages.Error500),
                        _ => new SessionException(ErrorMessages.HttpErrorGeneric),
                    };
                }
            }            
        }

        public async Task LoadTiles(GeoLocation location = null, CancellationToken ct = default)
        {
            if (!_tilesLoaded)
            {                
                if(location == null)
                {
                    location = GpsProvider.GetCurrentLocation();
                }                
                var tileService = new TileService();
                await tileService.LoadTiles(location, (int)Config.TileSize, _token, ct);
                CenterRef = tileService.CenterReference;
                PositioningUtils.Init(tileService.CenterReference);

                _tilesLoaded = true;
                SturfeeEventManager.Instance.TilesLoaded();
            }
        }

        public float GetTerrainElevation(GeoLocation location = null)
        {
            if (location == null)
            { 
                location = XRSessionManager.GetSession().GpsProvider.GetCurrentLocation();
            }
            RaycastHit hit;
            
            Vector3 worldPos = PositioningUtils.GeoToWorldPosition(location);
            Vector3 unityPos = new Vector3(worldPos.x, 15000, worldPos.y);

            Ray ray = new Ray(unityPos, Vector3.down);
            //Debug.DrawRay(ray.origin, ray.direction * 10000, Color.green, 2000);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(SturfeeLayers.Terrain)))
            {
                float elevation = hit.point.y;
                SturfeeDebug.Log("Elevation : " + elevation);
                return elevation;
            }

            return (float)location.Altitude;
        }        

        internal Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return GetSessionGO().GetComponent<MonoBehaviour>().StartCoroutine(enumerator);
        }

        internal void StopCoroutine(Coroutine coroutine)
        {
            GetSessionGO().GetComponent<MonoBehaviour>().StopCoroutine(coroutine);
        }

        internal GameObject GetSessionGO()
        {
            if (_sessionGO == null)
            {
                _sessionGO = new GameObject(SturfeeObjects.XRSessionObject);
                _sessionGO.AddComponent<XRSessionBehaviour>();
            }
            return _sessionGO;
        }

        private async Task PrepareForScan()
        {
            _preScanCTS = new CancellationTokenSource();
            var cancellationToken = _preScanCTS.Token;

            // Providers
            var gpsPrescan = GpsProvider.PrepareForScan(cancellationToken);
            var posePrescan= PoseProvider.PrepareForScan(cancellationToken);
            var videoPrescan = VideoProvider.PrepareForScan(cancellationToken);

            // Session
            var sessionVerification = VerifySession(cancellationToken).CancelOnFaulted(_preScanCTS);

            // Socket
            WebSocketService wss = new WebSocketService(ServerInfo.WebSocketServiceUrl, _token, Config.Locale);
            var socketConnection = LocalizationManager.PrepareForScan(wss, GpsProvider.GetCurrentLocation());

            try
            {
                await sessionVerification;
                SturfeeDebug.Log("Session verification complete");
                await gpsPrescan;
                SturfeeDebug.Log("GPS pre-scan complete");
                await posePrescan;
                SturfeeDebug.Log("Pose pre-scan complete");
                await videoPrescan;
                SturfeeDebug.Log("Video pre-scan complete");
                await socketConnection;
                SturfeeDebug.Log("Socket connection verified");

                SturfeeEventManager.Instance.ReadyForScan();           
            }
            catch (Exception e)
            {
                if (e is SessionException se)
                {
                    SturfeeEventManager.Instance.LocalizationFail(se.IdError);
                }
                else if (e is IdException ie)
                {
                    SturfeeEventManager.Instance.LocalizationFail(ie.IdError);                    
                }
                else if (e is OperationCanceledException)
                {
                    SturfeeDebug.Log("OperationCanceledException : CancelVPS was called");
                }
                else
                {
                    SturfeeDebug.LogError($"{e.Message}" + "\n" + e.StackTrace);
                    SturfeeEventManager.Instance.LocalizationFail(ErrorMessages.SessionNotReady);
                }

                Status = XRSessionStatus.NotCreated;
            }
            finally
            {
                _preScanCTS = null;
            }
        }

        private async Task VerifySession(CancellationToken ct)
        {
            if (Status < XRSessionStatus.Ready)
            {
                while (Status == XRSessionStatus.Initializing)
                {
                    SturfeeDebug.Log(" Waiting for session to be ready...");
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (Status == XRSessionStatus.NotCreated)
                {                    
                    SturfeeDebug.Log(" Session is not created or session creation failed. " +
                        "Attempting to create a session again");

                    await CreateSession(ct);
                }
            }
        }

        private void LoadSessionConfig(XRSessionConfig config)
        {
            var editorConfiguration = LoadEditorConfiguration();
            config.AccessToken = editorConfiguration.AccessToken;
            config.TileSize = editorConfiguration.TileSize;
            config.Locale = editorConfiguration.Theme.Locale;

            SturfeeDebug.Log(" Creating Session with " +
                " TileSize : " + config.TileSize.ToString() +
                " Language: " + config.Locale);
        }

        private EditorConfiguration LoadEditorConfiguration()
        {
            TextAsset configTextAsset = Resources.Load<TextAsset>(Paths.SturfeeResourcesRelative);
            if(configTextAsset != null)
            {
                return JsonUtility.FromJson<EditorConfiguration>(configTextAsset.text);
            }
            SturfeeDebug.LogError(" Cannot load editor config");
            return null;
        }
    }
}