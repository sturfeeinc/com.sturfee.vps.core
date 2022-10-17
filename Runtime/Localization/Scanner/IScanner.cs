using SturfeeVPS.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{

    public delegate void LocalizationResponseDelegate(ResponseMessage responseMessage, OffsetType offsetType);
    public interface IScanner
    {
        ScanType ScanType { get; }
        OffsetType OffsetType { get; }
        bool IsScanning { get; }
        Task Initialize(ScanConfig scanConfig = null, CancellationToken cancellationToken = default);
        Task Connect(string accessToken, string language = "en-US");
        void Disconnect();
        void StartScan(int scanId);
        void StopScan();

        event SturfeeEvents.OnFrameCaptured OnFrameCaptured;
        event SturfeeEvents.LocalizationLoadingAction OnLocalizationLoading;
        event LocalizationResponseDelegate OnLocalizationResponse;
    }

    public class ScannerBase : IScanner
    {
        public ScanType ScanType { get; protected set; }
        public OffsetType OffsetType { get; protected set; }
        public bool IsScanning { get; protected set; }

        protected int scanId;
        protected ScanConfig scanConfig;
        protected LocalizationService localizationService;

        public event LocalizationResponseDelegate OnLocalizationResponse;
        public event SturfeeEvents.OnFrameCaptured OnFrameCaptured;
        public event SturfeeEvents.LocalizationLoadingAction OnLocalizationLoading;

        public async virtual Task Initialize(ScanConfig scanConfig = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async virtual Task Connect(string accessToken, string language = "en-US")
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
        

        public virtual void StartScan(int scanId)
        {
            this.scanId = scanId;
            IsScanning = true;
            XRSessionManager.GetSession().Status = XRSessionStatus.Scanning;

            SturfeeDebug.Log(" Scan started with Id : " + scanId);
        }

        public virtual void StopScan()
        {
            IsScanning = false;
            XRSessionManager.GetSession().Status = XRSessionStatus.Ready;
        }

        public virtual void Disconnect()
        {
            SturfeeDebug.Log($" Disconnecting {ScanType} scanner socket connection...");
            IsScanning = false;
            localizationService?.Close();
        }

        protected virtual void InvokeOnLocalizationResponse(ResponseMessage responseMessage, OffsetType offsetType)
        {
            IsScanning = false;
            OnLocalizationResponse?.Invoke(responseMessage, offsetType);
        }

        
    }
}
