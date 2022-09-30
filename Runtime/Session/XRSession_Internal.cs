﻿using System;
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
                    _gpsProvider = new LocationProvider(Config.GpsProvider);
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

                await CheckConnection();

                var location = await GetLocation(ct);
                CenterRef = location;
                PositioningUtils.Init(location);

                SturfeeDebug.Log("Start Location : " + location.ToFormattedString());

                await ValidateToken(location);
                await CheckCoverage(location);
                await LoadTiles(location, ct);
                await WaitForVideo(ct);
                await WaitForPose(ct);

                SturfeeDebug.Log("Session ready");
                SturfeeEventManager.Instance.SessionIsReady();                
            }
            catch (Exception e)
            {
                SturfeeDebug.LogError(e.Message);
                Status = XRSessionStatus.NotCreated;
                SessionFail = e.Message;
                if (e is SessionException)
                {
                    throw;
                }

                throw new SessionException(ErrorMessages.SessionNotReady);
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

        public Vector3 GetEulerOffset => LocalizationManager.EulerOrientationCorrection;

        public async void EnableVPS(ScanType scanType, ScanConfig scanConfig = null)
        {
            _preScanCTS = new CancellationTokenSource();
            var cancellationToken = _preScanCTS.Token;

            try
            {
                await LocalizationManager.InitializeScan(scanType, scanConfig, _token, Config.Locale, cancellationToken);
                await VerifySession(cancellationToken);

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

            //await PrepareForScan(scanType, scanConfig);
        }

        public void PerformLocalization(LocalizationMode localizationMode = LocalizationMode.WebServer)
        {
            LocalizationManager.StartScan(localizationMode);
        }

        public void CancelVPS()
        {
            if (Status <= XRSessionStatus.Ready)
            {
                _preScanCTS?.Cancel();
            }
            else if (Status == XRSessionStatus.Scanning || Status == XRSessionStatus.Loading)
            {
                Status = XRSessionStatus.Ready;
            }

            LocalizationManager.StopScan();
        }

        public void DisableVPS()
        {
            LocalizationManager.DisableVPS();
            Status = XRSessionStatus.Ready;
        }

        internal async Task WaitForGps(CancellationToken ct = default)
        {
            SturfeeDebug.Log(" Waiting for GPS");

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
                
                SturfeeDebug.Log(" Waiting 1 second for GPS...");

                await Task.Delay(1000);
            }

            SturfeeDebug.Log($" GPS ready : {gpsProvider.GetCurrentLocation().ToFormattedString()}");
        }

        internal async Task WaitForPose(CancellationToken ct = default)
        {
            SturfeeDebug.Log(" Waiting for IMU/Pose");

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

            SturfeeDebug.Log($" Pose/IMU ready");
        }

        internal async Task WaitForVideo(CancellationToken ct = default)
        {
            SturfeeDebug.Log(" Waiting for Video");

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

            SturfeeDebug.Log($" Video ready");
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

            if (location == null)
            {
                if (GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    location = GetFallbackLocation();
                }
                else
                {
                    location = GpsProvider.GetCurrentLocation();
                }
            }

            try
            {
                await WebServices.ValidateToken(location, _token);
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

        public async Task CheckCoverage(GeoLocation location = null)
        {
            if (location == null)
            {
                if (GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    location = GetFallbackLocation();
                }
                else
                {
                    location = GpsProvider.GetCurrentLocation();
                }
            }

            try
            {
                await WebServices.CheckCoverage(location, _token);
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

        public async Task LoadTiles(GeoLocation location = null, CancellationToken ct = default)
        {
            if (location == null)
            {
                if (GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    location = GetFallbackLocation();
                }
                else
                {
                    location = GpsProvider.GetCurrentLocation();
                }
            }

            var tileService = new TileService();
            await tileService.LoadTiles(location, (int)Config.TileSize, _token, ct);            

            SturfeeEventManager.Instance.TilesLoaded();
        }

        public float GetTerrainElevation(GeoLocation location = null)
        {
            if (location == null)
            {
                if (GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    location = GetFallbackLocation();
                }
                else
                {
                    location = GpsProvider.GetCurrentLocation();
                }
            }
            RaycastHit hit;
            
            Vector3 worldPos = PositioningUtils.GeoToWorldPosition(location);
            Vector3 unityPos = new Vector3(worldPos.x, worldPos.z + 1000, worldPos.y);

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

        internal GeoLocation GetFallbackLocation()
        {
            GeoLocation location = new GeoLocation();
#if UNITY_EDITOR            
            location = EditorUtils.EditorFallbackLocation;
#else
            location = new GeoLocation(Input.location.lastData);
#endif
            return location;
        }

        private async Task<GeoLocation> GetLocation(CancellationToken ct)
        {
                           
            GeoLocation location = new GeoLocation();
            try
            {
                await WaitForGps(ct);
                location = GpsProvider.GetCurrentLocation();
            }
            catch (IdException e)
            {
                // IF GPSProvider timed out
                if (e.Id == ErrorMessages.GpsProviderTimeout.Item1)
                {
                    location = GetFallbackLocation();
                    SturfeeDebug.Log($"GPS Provider timed out. Using GPS {location.ToFormattedString()} from Unity's location API");
                }
            }

            return location;
        }

        private async Task VerifySession(CancellationToken ct)
        {
            if (Status < XRSessionStatus.Ready)
            {
                while (Status == XRSessionStatus.Initializing)
                {
                    SturfeeDebug.Log(" Waiting 1 second for session to be ready...");
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(1000);
                }

                if (Status == XRSessionStatus.NotCreated)
                {
                    SturfeeDebug.Log(" Session is not created or session creation failed. " +
                        "Attempting to create session again");

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