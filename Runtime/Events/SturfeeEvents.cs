using System;
namespace SturfeeVPS.Core
{
    public class SturfeeEvents
    {
        /// <summary>
        /// Event that is fired when the session is initialized
        /// </summary>
        public delegate void SessionInitializeAction();

        /// <summary>
        /// Event that is fired when tiles are loaded for session
        /// </summary>
        public delegate void TilesLoadedAction();

        /// <summary>
        /// Event that is fired when the session is ready
        /// </summary>
        public delegate void SessionReadyAction();

        /// <summary>
        /// Event that is fired when SDK Session is ready to perform a scan for localization
        /// </summary>
        public delegate void ScanReadyAction();

        /// <summary>
        /// Event that is fired when session creation failed
        /// </summary>
        /// <param name="error"></param>
        /// <param name="errorId"></param>
        public delegate void SessionFailAction(string error, string errorId);

        /// <summary>
        /// Event that is fired when a frame is captured during localization
        /// </summary>
        /// <param name="frameNum"> Current capture number</param>
        /// <param name="localizationRequest"> Captured request details</param>
        public delegate void OnFrameCaptured(int frameNum, LocalizationRequest localizationRequest, byte[] image);

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
        public delegate void LocalizationFailAction(string error, string id);
    }
}
