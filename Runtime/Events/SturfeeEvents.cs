using System;
namespace SturfeeVPS.Core
{
    public class SturfeeEvents
    {
        /// <summary>
        /// Event fired when a provider is registered to session
        /// </summary>
        /// <param name="provider">provider that is registered</param>
        public delegate void ProviderRegisterDelegate(IProvider provider);

        /// <summary>
        /// Event fired when a provider is unregistered from session
        /// </summary>
        /// <param name="provider">provider that was unregistered</param>
        public delegate void ProviderUnregisterDelegate(IProvider provider);

        /// <summary>
        /// Event that is fired when tiles are loaded for session
        /// </summary>
        public delegate void TilesLoadedAction();

        /// <summary>
        /// Event that is fired when tile loading failed
        /// </summary>
        public delegate void TilesLoadingFailAction(string error);

        /// <summary>
        /// Event that is fired when the session is ready
        /// </summary>
        public delegate void SessionReadyAction();

        /// <summary>
        /// Event that is fired when the session is destroyed
        /// </summary>
        public delegate void SessionDestroyAction();

        /// <summary>
        /// Event fired when localization is requested
        /// </summary>
        public delegate void LocalizationRequestedAction();

        /// <summary>
        /// Fire when localization process starts
        /// </summary>
        public delegate void LocalizationStartAction();

        /// <summary>
        /// Event fired after successful localization
        /// </summary>
        public delegate void LocalizationSuccessfulAction();

        /// <summary>
        /// Event that is fired when alignment is waiting response from VPS service.
        /// </summary>
        public delegate void LocalizationLoadingAction();

        /// <summary>
        /// Event that is fired when alignment fails.
        /// </summary>
        public delegate void LocalizationFailAction(string error);

        /// <summary>
        /// Event that is fired when localization is disabled for the session
        /// </summary>
        public delegate void LocalizationDisabledAction();

        /// <summary>
        /// Event that is fired when Debug Button is pressed
        /// </summary>
        public delegate void DebugButtonPressedAction();

        
    }
}
