using UnityEngine;

namespace SturfeeVPS.Core
{
    internal class SatelliteScanner : PanoramicMultiframeScanner
    {
        public SatelliteScanner(ScanConfig scanConfig = null) : base(scanConfig) 
        {
            Debug.Log(" Creating satellite Scanner");

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
        }
    }
}