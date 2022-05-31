using UnityEngine;

namespace SturfeeVPS.Core
{
    public static class LocationExtensions
    {
        public static Vector3 ToVector3(this UtmPosition utm)
        {
            return new Vector3((float)utm.X, (float)utm.Y, (float)utm.Z);
        }

        public static Vector3 ToUnityVector3(this UtmPosition utm)
        {
            return new Vector3((float)utm.X, (float)utm.Z, (float)utm.Y);
        }

        public static string ToFormattedString(this UtmPosition utm)
        {
            return "(N:" + utm.Northing
                + ", E:" + utm.Easting
                + ", Height:" + utm.Z
                + ", Zone:" + utm.Zone
                + ", Hemisphere:" + utm.Hemisphere
                + ")";
        }

        public static string ToFormattedString(this GeoLocation gps)
        {
            return "(Latitude:" + gps.Latitude
                + ", Longitude:" + gps.Longitude
                + ", Altitude:" + gps.Altitude
                + ")";
        }
    }
}
