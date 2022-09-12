using System;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal partial class LocalizationManager
    {
        public static readonly float CAMERA_HEIGHT = 1.5f;

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
                return session.GpsProvider.GetCurrentLocation();
            }

            GameObject parent = new GameObject("parent");
            parent.transform.rotation = session.GetOrientationOffset();

            GameObject child = new GameObject("child");
            child.transform.parent = parent.transform;
            child.transform.localPosition =
                session.PoseProvider.GetPosition() - _trackingOrigin;

            Vector3 vpsWorld = PositioningUtils.GeoToWorldPosition(LocationCorrection);
            Vector3 finalWorldPos = vpsWorld + child.transform.position;

            UnityEngine.Object.Destroy(child);
            UnityEngine.Object.Destroy(parent);

            return PositioningUtils.WorldToGeoLocation(finalWorldPos);
        }

        public Quaternion GetXRCameraOrientation()
        {
            var sensor = XRSessionManager.GetSession().PoseProvider.GetOrientation();

            if (PlayerPrefs.GetInt("SturfeeVPS.Offset.UseEuler", 0) == 0)
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
                location = new GeoLocation
                {
                    Latitude = XRSessionManager.GetSession().GpsProvider.GetCurrentLocation().Latitude,
                    Longitude = XRSessionManager.GetSession().GpsProvider.GetCurrentLocation().Longitude,
                    Altitude = TerrainElevation + CameraHeight
                };

                relative = XRSessionManager.GetSession().PoseProvider.GetPosition();
            }
            else
            {
                location = LocationCorrection;
                relative = Rotate(_trackingOrigin);
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