using System;
using System.Collections;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public class SturfeeEventManager
    {
        private static SturfeeEventManager _instance;

        public static SturfeeEventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SturfeeEventManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Fires when XR Session is initializing
        /// </summary>
        public event SturfeeEvents.SessionInitializeAction OnSessionInitializing;

        /// <summary>
        /// Fires when tiles are loaded in session
        /// </summary>
        public event SturfeeEvents.TilesLoadedAction OnTilesLoaded;

        /// <summary>
        /// Fires when XR Session is initialized and ready.
        /// </summary>
        public event SturfeeEvents.SessionReadyAction OnSessionReady;

        /// <summary>
        /// Fires when XR Session creation failed
        /// </summary>
        public event SturfeeEvents.SessionFailAction OnSessionFail;

        /// <summary>
        /// Fires after localization request is successful
        /// </summary>
        public event SturfeeEvents.LocalizationSuccessfulAction OnLocalizationSuccessful;

        /// <summary>
        /// Fires after SDK is ready to scan for localization
        /// </summary>
        public event SturfeeEvents.ScanReadyAction OnReadyForScan;

        /// <summary>
        /// Fires after every frame capture during localization
        /// </summary>
        public event SturfeeEvents.OnFrameCaptured OnFrameCaptured;

        /// <summary>
        /// Fires when alignment request is made.
        /// </summary>
        public event SturfeeEvents.LocalizationLoadingAction OnLocalizationLoading;

        /// <summary>
        /// Fires when localization request fails.
        /// </summary>
        public event SturfeeEvents.LocalizationFailAction OnLocalizationFail;

        public static void Destroy()
        {
            if (_instance != null)
            {
                _instance = null;
            }
        }

        internal void SessionInitializing()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Initializing;
            OnSessionInitializing?.Invoke();
        }

        internal void TilesLoaded()
        {
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnTilesLoaded?.Invoke();
            }));
        }

        internal void SessionIsReady()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Ready;
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnSessionReady?.Invoke();
            }));
        }

        internal void SessionFail((string, string) error)
        {
            SturfeeDebug.LogError("Session Fail : " + error.Item2);
            XRSessionManager.GetSession().Status = XRSessionStatus.NotCreated;
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnSessionFail?.Invoke(error.Item2, error.Item1);
            }));
        }

        internal void ReadyForScan()
        {
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnReadyForScan?.Invoke();
            }));
        }

        internal void LocalizationLoading()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Loading;
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnLocalizationLoading?.Invoke();
            }));
        }

        internal void LocalizationFail((string, string) error )
        {
            SturfeeDebug.LogError("Localization Fail : " + error.Item2);
            XRSessionManager.GetSession().Status = XRSessionStatus.Ready;
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnLocalizationFail?.Invoke(error.Item2, error.Item1);
            }));
        }

        internal void FrameCaptured(int frameNum, LocalizationRequest localizationRequest, byte[] image)
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Scanning;
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnFrameCaptured?.Invoke(frameNum, localizationRequest, image);
            }));
        }

        internal void LocalizationSuccessful()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Localized;
            XRSessionManager.GetSession().StartCoroutine(InvokeEventDelayed(() =>
            {
                OnLocalizationSuccessful?.Invoke();
            }));
        }

        private IEnumerator InvokeEventDelayed(Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback();
        }
    }
}
