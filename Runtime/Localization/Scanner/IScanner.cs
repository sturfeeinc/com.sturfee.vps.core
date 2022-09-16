using SturfeeVPS.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{

    internal delegate void LocalizationResponseDelegate(ResponseMessage responseMessage, OffsetType offsetType);
    internal interface IScanner
    {
        event LocalizationResponseDelegate OnLocalizationResponse;
        Task Connect(GeoLocation location, string accessToken, string language = "en-US");
        void Disconnect();
        void StartScan(int scanId);
        void StopScan();
        ScanType ScanType { get; }
        OffsetType OffsetType { get; }
    }

    internal class ScannerBase : IScanner
    {
        public ScanType ScanType { get; set; }
        public OffsetType OffsetType { get; set; }

        protected ScanConfig scanConfig;
        protected int scanId;
        protected LocalizationService localizationService;

        public event LocalizationResponseDelegate OnLocalizationResponse;

        public ScannerBase(ScanConfig scanConfig = null)
        {
            this.scanConfig = scanConfig;
        }

        public async virtual Task Connect(GeoLocation location, string accessToken, string language = "en-US")
        {
            // Disconnect any previous open connection
            if (localizationService != null)
            {
                if (localizationService.IsConnected)
                {
                    Disconnect();
                }

                localizationService = null;
            }           
        }

        public virtual void Disconnect()
        {
            SturfeeDebug.Log($" Disconnecting {ScanType} scanner socket connection...");
            localizationService?.Close();               
        }

        public virtual void StartScan(int scanId)
        {
            this.scanId = scanId;

            XRSessionManager.GetSession().Status = XRSessionStatus.Scanning;
        }

        public virtual void StopScan()
        {
            XRSessionManager.GetSession().Status = XRSessionStatus.Ready;
        }

        protected virtual void InvokeOnLocalizationResponse(ResponseMessage responseMessage, OffsetType offsetType)
        {
            OnLocalizationResponse?.Invoke(responseMessage, offsetType);
        }
    }
}
