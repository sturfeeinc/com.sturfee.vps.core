using System;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public static class PositioningUtils
    {
        private static UtmPosition _referenceUtm;

        public static UtmPosition GetReferenceUTM
        {
            get
            {
                return _referenceUtm;
            }
        }

        public static void Init(GeoLocation referenceLocation)
        {
            _referenceUtm = GeoCoordinateConverter.GpsToUtm(referenceLocation);
        }

        public static Vector3 GeoToWorldPosition(GeoLocation location)
        {
            if (_referenceUtm == null)
            {
                SturfeeDebug.LogError(" Center reference not set");
                return Vector3.zero;
            }

            UtmPosition utmPosition = GeoCoordinateConverter.GpsToUtm(location);            

            utmPosition.X -= _referenceUtm.X;
            utmPosition.Y -= _referenceUtm.Y;
            utmPosition.Z = location.Altitude;

            return utmPosition.ToVector3();
        }

        public static GeoLocation WorldToGeoLocation(Vector3 worldPos)
        {
            if (_referenceUtm == null)
            {
                SturfeeDebug.LogError(" Center reference not set");
                return null;
            }

            var utmPosition = new UtmPosition
            {
                Hemisphere = _referenceUtm.Hemisphere,
                Zone = _referenceUtm.Zone,
                X = _referenceUtm.X + worldPos.x,
                Y = _referenceUtm.Y + worldPos.y,
                Z = _referenceUtm.Z + worldPos.z
            };

            GeoLocation location = GeoCoordinateConverter.UtmToGps(utmPosition);          
            location.Altitude = worldPos.z;

            return location;
        }
    }
}
