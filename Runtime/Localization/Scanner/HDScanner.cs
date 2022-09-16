using SturfeeVPS.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal class HDScanner : PanoramicMultiframeScanner
    {
        private string _siteId;
        public HDScanner(ScanConfig scanConfig = null) : base(scanConfig) 
        {
            Debug.Log(" Creating HD Scanner");

            // set scan properties
            ScanProperties.YawAngle = ScanProperties.Defaults.HD.YawAngle;
            ScanProperties.TargetCount = ScanProperties.Defaults.HD.TargetCount;
            ScanProperties.PitchMin = ScanProperties.Defaults.HD.PitchMin;
            ScanProperties.PitchMax = ScanProperties.Defaults.HD.PitchMax;
            ScanProperties.RollMin = ScanProperties.Defaults.HD.RollMin;
            ScanProperties.RollMax = ScanProperties.Defaults.HD.RollMax;

            serviceUrl = ServerInfo.VPSHD_WEBSOCKET;

            ScanType = ScanType.HD;
            OffsetType = OffsetType.Euler;

            if (scanConfig == null)
            {
                Debug.LogError("[HDScanner] :: Scanconfig is NULL. Cannot read siteId");
                return;
            }

            _siteId = scanConfig.HD.SiteId;

            // override current gps location to use site's location
            var locationProvider = ((LocationProvider)XRSessionManager.GetSession().GpsProvider);
            locationProvider.Override = true;
            locationProvider.OverrideCurrentLocation(scanConfig.HD.Location) ;

            SturfeeDebug.Log($" Setting Session's location to site's location {scanConfig.HD.Location.ToFormattedString()}");
        }


        public override void Disconnect()
        {
            base.Disconnect();

            var locationProvider = ((LocationProvider)XRSessionManager.GetSession().GpsProvider);
            if (locationProvider.Override)
            {
                SturfeeDebug.Log($" Resetting session's gps location to gps provider location");
                locationProvider.Override = false;
            }
        }

        protected override void InvokeOnLocalizationResponse(ResponseMessage responseMessage, OffsetType offsetType)
        {
            base.InvokeOnLocalizationResponse(responseMessage, offsetType);

            var locationProvider = ((LocationProvider)XRSessionManager.GetSession().GpsProvider);
            if (locationProvider.Override)
            {
                SturfeeDebug.Log($" Resetting session's gps location to gps provider location");
                locationProvider.Override = false;
            }
        }

        protected override Request CaptureRequest(uint requestId, OperationMessages operationMessage, uint numOfFrames, uint frameOrder, string trackingId)
        {
            var request = base.CaptureRequest(requestId, operationMessage, numOfFrames, frameOrder, trackingId);

            request.SiteId = _siteId;
            SturfeeDebug.Log($" Adding SiteId {_siteId} to request");

            return request;
        }
    }
}
