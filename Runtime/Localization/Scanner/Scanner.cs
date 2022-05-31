using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SturfeeVPS.Core.Proto;

namespace SturfeeVPS.Core
{
    internal class Scanner
    {
        public ScanType ScanType;
        public int ScanId;
        public string TrackingId;
        public List<LocalizationRequest> Requests;
        public LocalizationResponse Response;
        public string ScanFail;


        internal WebSocketService webSocketService;        
        
        protected Action<ResponseMessage> onLocalizationResponse;

        internal Scanner(int scanId, WebSocketService webSocketService, Action<ResponseMessage> onLocalizationResponse)
        {
            this.webSocketService = webSocketService;
            this.onLocalizationResponse = onLocalizationResponse;
            ScanId = scanId;
        }

        public async Task OpenWebsocketConnection(GeoLocation location)
        {
            try
            {
                await webSocketService.Connect(location.Latitude, location.Longitude);
            }
            catch (Exception e)
            {
                SturfeeDebug.LogError(e.Message);
                throw new SessionException(ErrorMessages.SocketConnectionFail);
            }
        }

        public async virtual Task StartScan()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Scanning;
        }

        internal virtual void StopScan()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Ready;
        }
    }
}