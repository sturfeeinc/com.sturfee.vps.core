using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal class SatelliteScanner : PanoramicMultiframeScanner
    {
        public async override Task Initialize(ScanConfig scanConfig = null, CancellationToken cancellationToken = default)
        {
            SturfeeDebug.Log(" Initializing Satellite Scanner");

            // set scan properties
            ScanProperties.YawAngle = ScanProperties.Defaults.Satellite.YawAngle;
            ScanProperties.TargetCount = ScanProperties.Defaults.Satellite.TargetCount;
            ScanProperties.PitchMin = ScanProperties.Defaults.Satellite.PitchMin;
            ScanProperties.PitchMax = ScanProperties.Defaults.Satellite.PitchMax;
            ScanProperties.RollMin = ScanProperties.Defaults.Satellite.RollMin;
            ScanProperties.RollMax = ScanProperties.Defaults.Satellite.RollMax;

            serviceUrl = ServerInfo.WebSocketServiceUrl;

            ScanType = ScanType.Satellite;
            OffsetType = OffsetType.Quaternion;

            await base.Initialize(scanConfig, cancellationToken);
        }

        public override async Task Connect(string accessToken, string language = "en-US")
        {
            await base.Connect(accessToken, language);

            try
            {
                var location = XRSessionManager.GetSession().GpsProvider.GetCurrentLocation();
                await localizationService.Connect(location.Latitude, location.Longitude);
            }
            catch (Exception e)
            {
                SturfeeDebug.LogError(e.Message);
                throw new SessionException(ErrorMessages.SocketConnectionFail);
            }
        }
    }
}