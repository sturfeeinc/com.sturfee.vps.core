using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace SturfeeVPS.Core
{
    public class XRSession 
    {
        internal string SessionId;

        internal LocalizationManager LocalizationManager => _sessionInternal.LocalizationManager;
        internal XRSessionConfig Config => _sessionInternal.Config;
        internal GeoLocation CenterRef => _sessionInternal.CenterRef;
        internal string SessionFail => _sessionInternal.SessionFail;

        private XRSession_Internal _sessionInternal;
        private CancellationTokenSource _sessionCTS;

        #region Providers

        /// <summary>
        /// GpsProvider used to create this session
        /// </summary>
        public IGpsProvider GpsProvider => _sessionInternal.GpsProvider;

        /// <summary>
        /// PoseProvider used to create this session
        /// </summary>
        public IPoseProvider PoseProvider => _sessionInternal.PoseProvider;

        /// <summary>
        /// VideoProvider used to create this session
        /// </summary>
        public IVideoProvider VideoProvider => _sessionInternal.VideoProvider;

        #endregion

        #region Pose

        /// <summary>
        /// Gets current XR Camera Geo-location in World 
        /// </summary>
        /// <returns> XR Camera's Geo-location </returns>
        public GeoLocation GetXRCameraLocation()
        {
            return _sessionInternal.GetXRCameraLocation();
        }

        /// <summary>
        /// Gets current XR Camera orientation in World
        /// </summary>
        /// <returns>XR Camera's Orientation</returns>
        public Quaternion GetXRCameraOrientation()
        {
            return _sessionInternal.GetXRCameraOrientation();
        }

        /// <summary>
        /// Gets offset position that can be added to local position read from sensor (PoseProvider).
        /// The resulting position is equivalent to <see cref="GetXRCameraLocation()"/>, but converted into local space
        /// </summary>
        /// <returns> Location Offset</returns>        
        public Vector3 GetLocationOffset()
        {
            return _sessionInternal.GetLocationOffset();
        }

        /// <summary>
        /// Gets offset orientation that can be applied to orientation read from sensor (PoseProvider).
        /// The resulting orientation is equivalent to <see cref="GetXRCameraOrientation()"/>
        /// </summary>
        /// <returns> Orientation offset </returns>
        public Quaternion GetOrientationOffset()
        {
            return _sessionInternal.GetOrientationOffset();
        }

        #endregion

        #region Localization

        /// <summary>
        /// Begins the process to perform localization to enable VPS on current session
        /// </summary>
        public void EnableVPS()
        {
            _sessionInternal.EnableVPS();
        }

        /// <summary>
        /// Performs localization by capturing multiple frames
        /// </summary>
        /// <param name="scanType">The type of scan to be performed</param>
        /// <param name="localizationMode"></param>
        public void PerformLocalization(ScanType scanType, LocalizationMode localizationMode = LocalizationMode.WebServer)
        {
            _sessionInternal.PerformLocalization(scanType, localizationMode);
        }

        /// <summary>
        /// Stops current VPS Localization request, if any.
        /// </summary>
        public void CancelVPS()
        {
            _sessionInternal.CancelVPS();
        }

        /// <summary>
        /// Resets any offsets applied using VPS Localization
        /// </summary>
        public void DisableVPS()
        {
            _sessionInternal.DisableVPS();
        }

        #endregion

        public XRSessionStatus Status
        {
            internal set => _sessionInternal.Status = value;
            get => _sessionInternal.Status;
        }

        internal XRSession(XRSessionConfig xRSessionConfig)
        {
            _sessionInternal = new XRSession_Internal(xRSessionConfig);

            StartCoroutine(CreateSession().AsCoroutine());

            SessionId = DateTime.Now.Ticks.ToString();
        }

        internal Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return _sessionInternal.StartCoroutine(enumerator);
        }

        internal void StopCoroutine(Coroutine coroutine)
        {
            _sessionInternal.StopCoroutine(coroutine);
        }

        internal GameObject GetSessionGO()
        {
            return _sessionInternal.GetSessionGO();
        }

        internal async Task CreateSession()
        {
            _sessionCTS = new CancellationTokenSource();
            try
            {
                await _sessionInternal.CreateSession(_sessionCTS.Token);
            }
            catch (Exception ex)
            {
                SturfeeDebug.LogError($" ERROR => CreateSession :: {ex.Message}" );
                throw;
            }
        }

        internal void DestroySession()
        {
            _sessionCTS?.Cancel();
            _sessionInternal?.DestroySession();
        }

        internal async Task ValidateToken()
        {
            await _sessionInternal.ValidateToken();
        }

        internal async Task CheckCoverage()
        {
            await _sessionInternal.CheckCoverage();
        }

        internal async Task LoadTiles()
        {
            await _sessionInternal.LoadTiles();
        }

        internal float GetTerrainElevation()
        {
            return _sessionInternal.GetTerrainElevation();
        }
    }
}

