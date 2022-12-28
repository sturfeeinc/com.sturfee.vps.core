using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{
    public class SturfeeEventManager
    {
        public static event SturfeeEvents.ProviderRegisterDelegate OnProviderRegister;
        public static event SturfeeEvents.ProviderUnregisterDelegate OnProviderUnregister;

        public static event SturfeeEvents.SessionReadyAction OnSessionReady;
        public static event SturfeeEvents.SessionDestroyAction OnSessionDestroy;

        public static event SturfeeEvents.TilesLoadedAction OnTilesLoaded;
        public static event SturfeeEvents.TilesLoadingFailAction OnTileLoadingFail;


        public static event SturfeeEvents.LocalizationRequestedAction OnLocalizationRequested;
        public static event SturfeeEvents.LocalizationStartAction OnLocalizationStart;
        public static event SturfeeEvents.LocalizationLoadingAction OnLocalizationLoading;
        public static event SturfeeEvents.LocalizationFailAction OnLocalizationFail;
        public static event SturfeeEvents.LocalizationSuccessfulAction OnLocalizationSuccessful;
        public static event SturfeeEvents.LocalizationDisabledAction OnLocalizationDisabled;

        public static bool AvatarOn = false;

        // FOR DEBUG
        public static event SturfeeEvents.DebugButtonPressedAction OnDebugButtonPressed;
        public static void TriggerSturfeeDebugs()
        {
            SturfeeEventManager.OnDebugButtonPressed?.Invoke();
        }

        internal static void SessionReady()
        {
            SturfeeDebug.Log($" [Event] :: OnSessionReady");
            OnSessionReady?.Invoke();
        }

        internal static void SessionDestroy()
        {
            SturfeeDebug.Log($" [Event] :: OnSessionDestroy");
            OnSessionDestroy?.Invoke();
        }

        internal static void RegisterProvider<T>(T provider) where T : IProvider
        {
            // Tiles
            if (typeof(T) == typeof(ITilesProvider))
            {
                var tilesProvier = (ITilesProvider)provider;
                tilesProvier.OnTileLoaded += TileProvider_OnTileLoaded;
                tilesProvier.OnTileLoadingFail += TilesProvier_OnTileLoadingFail;
            }

            // Localization
            if (typeof(T) == typeof(ILocalizationProvider))
            {
                var localizationProvider = (ILocalizationProvider)provider;
                localizationProvider.OnLocalizationRequested += LocalizationProvider_OnLocalizationRequested;
                localizationProvider.OnLocalizationStart += LocalizationProvider_OnLocalizationStart;
                localizationProvider.OnLocalizationLoading += LocalizationProvider_OnLocalizationLoading;
                localizationProvider.OnLocalizationFail += LocalizationProvider_OnLocalizationFail;
                localizationProvider.OnLocalizationSuccessful += LocalizationProvider_OnLocalizationSuccessful;
                localizationProvider.OnLocalizationDisabled += LocalizationProvider_OnLocalizationDisabled;
            }

            // TODO: Handle event subscription if any other providers have events

            SturfeeDebug.Log($" [Event] :: OnProviderRegister ({provider.GetType().Name})");
            OnProviderRegister?.Invoke(provider);
        }

        

        public static void UnregisterProvider<T>(T provider) where T : IProvider
        {
            if (provider != null)
            {
                // Tiles
                if (typeof(T) == typeof(ITilesProvider))
                {
                    var tilesProvier = (ITilesProvider)provider;
                    tilesProvier.OnTileLoaded -= TileProvider_OnTileLoaded;
                    tilesProvier.OnTileLoadingFail -= TilesProvier_OnTileLoadingFail;
                }

                // Localization
                if (typeof(T) == typeof(ILocalizationProvider))
                {
                    var localizationProvider = (ILocalizationProvider)provider;
                    localizationProvider.OnLocalizationRequested -= LocalizationProvider_OnLocalizationRequested;
                    localizationProvider.OnLocalizationStart -= LocalizationProvider_OnLocalizationStart;
                    localizationProvider.OnLocalizationLoading -= LocalizationProvider_OnLocalizationLoading;
                    localizationProvider.OnLocalizationFail -= LocalizationProvider_OnLocalizationFail;
                    localizationProvider.OnLocalizationSuccessful -= LocalizationProvider_OnLocalizationSuccessful;
                    localizationProvider.OnLocalizationDisabled -= LocalizationProvider_OnLocalizationDisabled;
                }

                // TODO: Handle event subscription if any other providers have events
            }
            SturfeeDebug.Log($" [Event] :: OnProviderUnregister ({provider?.GetType().Name})");
            OnProviderUnregister?.Invoke(provider);
        }

        private static void TileProvider_OnTileLoaded()
        {
            SturfeeDebug.Log($" [Event] :: OnTilesLoaded");
            OnTilesLoaded?.Invoke();
        }

        private static void TilesProvier_OnTileLoadingFail(string error)
        {
            SturfeeDebug.Log($" [Event] :: OnTileLoadingFail");
            OnTileLoadingFail?.Invoke(error);
        }

        private static void LocalizationProvider_OnLocalizationRequested()
        {
            SturfeeDebug.Log($" [Event] :: OnLocalizationRequested");
            OnLocalizationRequested?.Invoke();
        }

        private static void LocalizationProvider_OnLocalizationStart()
        {
            SturfeeDebug.Log($" [Event] :: OnLocalizationStart");
            OnLocalizationStart?.Invoke();
        }

        private static void LocalizationProvider_OnLocalizationDisabled()
        {
            SturfeeDebug.Log($" [Event] :: OnLocalizationDisabled");
            OnLocalizationDisabled?.Invoke();
        }

        private static void LocalizationProvider_OnLocalizationSuccessful()
        {
            SturfeeDebug.Log($" [Event] :: OnLocalizationSuccessful");
            OnLocalizationSuccessful?.Invoke();
        }

        private static void LocalizationProvider_OnLocalizationFail(string error)
        {
            SturfeeDebug.Log($" [Event] :: OnLocalizationFail");
            OnLocalizationFail?.Invoke(error);
        }

        private static void LocalizationProvider_OnLocalizationLoading()
        {
            SturfeeDebug.Log($" [Event] :: OnLocalizationLoading");
            OnLocalizationLoading?.Invoke();
        }

    }
}
