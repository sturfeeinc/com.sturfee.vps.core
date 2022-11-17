﻿using System;
using UnityEngine;

namespace SturfeeVPS.Core
{
    [Serializable]
    public class GeoLocation : IEquatable<GeoLocation>
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        
        public static float Distance(GeoLocation geoLocation1, GeoLocation geoLocation2, bool useAltitude = false)
        {
            var utm1 = GeoCoordinateConverter.GpsToUtm(geoLocation1);
            var utm2 = GeoCoordinateConverter.GpsToUtm(geoLocation2);

            Vector3 difference = new Vector3
            {
                x = (float)(utm1.X - utm2.X),
                y = (float)(utm1.Y - utm2.Y),
            };

            if (useAltitude)
            {
                difference.z = (float)(utm1.Z - utm2.Z);
            }

            float distance = (float)Math.Sqrt(difference.x * difference.x + difference.y* difference.y + difference.z * difference.z);
            return distance;
        }

        public bool Equals(GeoLocation other)
        {
            double lat = Math.Abs(Latitude - other.Latitude);
            double lon = Math.Abs(Longitude - other.Longitude);
            double altitude = Math.Abs(Altitude - other.Altitude);

            return lat < 0.001f && lon < 0.001f &&  altitude < 0.001f;
        }

        public GeoLocation() { }
        public GeoLocation(LocationInfo locationInfo)
        {
            Latitude = locationInfo.latitude;
            Longitude = locationInfo.longitude;
            Altitude = locationInfo.altitude;            
        }

    }

}
