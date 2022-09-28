using System;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal partial class LocalizationManager
    {
        public static readonly float CAMERA_HEIGHT = 1.5f;
        public Quaternion YawOrientationCorrection { get; private set; }
        public Quaternion PitchOrientationCorrection { get; private set; }
        public Vector3 EulerOrientationCorrection { get; private set; }
        public GeoLocation LocationCorrection { get; private set; }

        private float _terrainElevation;
        private float TerrainElevation
        {
            get
            {
                if(_terrainElevation == 0)
                {
                    _terrainElevation = XRSessionManager.GetSession().GetTerrainElevation();
                }
                return _terrainElevation;
            }
        }

        public GeoLocation GetXRCameraLocation()
        {
            XRSession session = XRSessionManager.GetSession();

            if (session.Status != XRSessionStatus.Localized)
            {
                if (session.GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                    return XRSessionManager.GetSession().GetFallbackLocation();

                return session.GpsProvider.GetCurrentLocation();
            }

            GameObject parent = new GameObject("parent");
            parent.transform.rotation = session.GetOrientationOffset();

            GameObject child = new GameObject("child");
            child.transform.parent = parent.transform;
            child.transform.localPosition =
                session.PoseProvider.GetPosition() - _positionOnScanStart;

            Vector3 vpsWorld = PositioningUtils.GeoToWorldPosition(LocationCorrection);
            Vector3 finalWorldPos = vpsWorld + child.transform.position;

            UnityEngine.Object.Destroy(child);
            UnityEngine.Object.Destroy(parent);

            return PositioningUtils.WorldToGeoLocation(finalWorldPos);
        }

        public Quaternion GetXRCameraOrientation()
        {
            var sensor = XRSessionManager.GetSession().PoseProvider.GetOrientation();

            if (_offsetType == OffsetType.Quaternion)
            {
                var yaw = YawOrientationCorrection;
                var pitch = PitchOrientationCorrection;

                return yaw * sensor * pitch;
            }

            Quaternion offset = Quaternion.Euler(EulerOrientationCorrection);
            return offset * sensor ;            
        }

        public Vector3 GetLocationOffset()
        {
            GeoLocation location;
            Vector3 relative;
            if (XRSessionManager.GetSession().Status != XRSessionStatus.Localized)
            {
                if (XRSessionManager.GetSession().GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    location = XRSessionManager.GetSession().GetFallbackLocation();
                }
                else
                {
                    location = XRSessionManager.GetSession().GpsProvider.GetCurrentLocation();
                }
                location.Altitude = TerrainElevation + CameraHeight;                

                relative = XRSessionManager.GetSession().PoseProvider.GetPosition();
            }
            else
            {
                location = LocationCorrection;
                relative = Rotate(_positionOnScanStart);
            }

            var absolute = PositioningUtils.GeoToWorldPosition(location);
            Vector3 offset = absolute - relative;

            return offset;
        }

        public Quaternion GetOrientationOffset()
        {
            var corrected = GetXRCameraOrientation();
            var sensorInverse = Quaternion.Inverse(XRSessionManager.GetSession().PoseProvider.GetOrientation());

            // => remove sensor orientatino from corrected to get only the offset applied
            return corrected * sensorInverse;
        }

        private Vector3 Rotate(Vector3 point)
        {
            GameObject parent = new GameObject();
            parent.transform.rotation = XRSessionManager.GetSession().GetOrientationOffset();

            GameObject child = new GameObject();
            child.transform.parent = parent.transform;
            child.transform.localPosition = point;

            Vector3 result = child.transform.position;
            
            UnityEngine.Object.Destroy(child);
            UnityEngine.Object.Destroy(parent);

            return result;
        }

        private float CameraHeight
        {
            get
            {
                float height = XRSessionManager.GetSession().PoseProvider.GetHeightFromGround();
                if(height >= 1.2f && height <= 1.7f)
                {
                    return height;
                }

                return CAMERA_HEIGHT;
            }
        }
    }
}