
using System;

namespace SturfeeVPS.Core
{
    public class GeoCoordinateConverter
    {
        private static double _eccSquared = 0.00669438; // eccentricity (0.081819191 ^ 2) WGS84
        private static double _equatorialRadius = 6378137.0; // WGS84 (note above: varies from 6,356.750 km to 6,378.135 km)
        private static double _scaleFactor = 0.9996; // scale factor, used as k0
        private static double _denominatorOfFlatteningRatio = 298.257223563;

        // Ellipsoid model constants 
        private static double sm_a = 6378137.0;
        private static double sm_b = 6356752.314;
        private static double sm_EccSquared = 6.69437999013e-03;
        private static double UTMScaleFactor = 0.9996;

        public static UtmPosition GpsToUtm(GeoLocation gps)
        {
            UtmPosition utmPosition = new UtmPosition();
            Wgs84ToUtm(gps, ref utmPosition);
            return utmPosition;
        }

        public static GeoLocation UtmToGps(UtmPosition utm)
        {
            GeoLocation gps = new GeoLocation();
            UtmToWgs84(utm, ref gps);
            return gps;
        }

        private static void GpsToUtm(GeoLocation gps, ref UtmPosition utm)
        {
            Wgs84ToUtm(gps, ref utm);
        }

        private static void UtmToGps(UtmPosition utm, ref GeoLocation gps)
        {
            UtmToWgs84(utm, ref gps);
        }

        private static void Wgs84ToUtm(GeoLocation gps, ref UtmPosition utm)
        {
            if(gps == null || utm == null)
            {
                utm = new UtmPosition();
                return;
            }

            //if(logger != null)
            //    logger.Debug("--[+] => LocationProvider: Converting GPS ==> UTM");

            // if deviation from WGS84 is desired, do this (after loading the array, duh):
            //  cEllipsoid[] ellipsoidRg = EllipsoidLoad();
            //  dEquatorialRadius = ellipsoid[index-of-desired-reference-ellipsoid].EquatorialRadius;
            //  eccSquared = ellipsoid[index-of-desired-reference-ellipsoid].eccentricitySquared;

            // convert degrees to radians
            double rLatitude = ConvertToRadians(gps.Latitude);
            // use dLonWork to make sure the longitude is between -180.00 .. 179.9
            double fixedLongitude = (gps.Longitude + 180) - ((int)((gps.Longitude + 180) / 360)) * 360 - 180; // -180.00 .. 179.9;
            // convert degrees to radians
            double dLonRad = ConvertToRadians(fixedLongitude);

            utm.Zone = GetUtmZone(gps.Latitude, fixedLongitude);
            // set the resultant UTM Zone string
            utm.Hemisphere = GetHemisphere(gps.Latitude); //iUTM_Zone_Num.ToString() + GetHemisphere(gps.Latitude);

            // set central meridian
            double centralMeridian = CentralMeridianFromUtmZone(utm.Zone);
            // convert degrees to radians
            double rCentralMeridian = ConvertToRadians(centralMeridian);// * dCvtDeg2Rad;

            double eccPrimeSquared = _eccSquared / (1 - _eccSquared);

            double N = _equatorialRadius / Math.Sqrt(1 - _eccSquared * Math.Sin(rLatitude) * Math.Sin(rLatitude));
            double T = Math.Tan(rLatitude) * Math.Tan(rLatitude);
            double C = eccPrimeSquared * Math.Cos(rLatitude) * Math.Cos(rLatitude);
            double A = Math.Cos(rLatitude) * (dLonRad - rCentralMeridian);
            double M = _equatorialRadius * ((1 - _eccSquared / 4 - 3 * _eccSquared * _eccSquared / 64 - 5 * _eccSquared * _eccSquared * _eccSquared / 256) * rLatitude
                - (3 * _eccSquared / 8 + 3 * _eccSquared * _eccSquared / 32 + 45 * _eccSquared * _eccSquared * _eccSquared / 1024) * Math.Sin(2 * rLatitude)
                + (15 * _eccSquared * _eccSquared / 256 + 45 * _eccSquared * _eccSquared * _eccSquared / 1024) * Math.Sin(4 * rLatitude)
                - (35 * _eccSquared * _eccSquared * _eccSquared / 3072) * Math.Sin(6 * rLatitude));

            utm.Easting = (double)(_scaleFactor * N * (A + (1 - T + C) * A * A * A / 6
                + (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A * A * A * A * A / 120)
                + 500000.0);

            utm.Northing = (double)(_scaleFactor * (M + N * Math.Tan(rLatitude) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24
                + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720)));
            if (gps.Latitude < 0)
                utm.Northing += 10000000.0f; // 10000000 meter offset for southern hemisphere

            //if (logger != null)
            //    logger.Debug("--[+] => LocationProvider: Converting GPS ==> UTM ***DONE***");

            utm.Z = gps.Altitude;

            return;
        }        

        // todo: move this to a utility class
        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        private static double ConvertToDegrees(double radians)
        {
            return radians / (Math.PI / 180);
        }

        public static int GetUtmZone(double latitude, double longitude)
        {
            if ((longitude < -180.0) || (longitude > 180.0))
                return (0); // implies failure: 0 is an invalid utm zone

            int iUTM_Zone_Num = (int)((180.0 + longitude) / 6.0 + 1.0);

            if (latitude >= 56.0 && latitude < 64.0) // sw Norge (ie, zone 32V)
            {
                if (longitude >= 3.0 && longitude < 12.0)
                    iUTM_Zone_Num = 32;
            }
            else if (latitude >= 72.0 && latitude < 84.0) // special zones for Svalbard
            {
                if (longitude >= 0.0 && longitude < 9.0)
                    iUTM_Zone_Num = 31;
                else if (longitude >= 9.0 && longitude < 21.0)
                    iUTM_Zone_Num = 33;
                else if (longitude >= 21.0 && longitude < 33.0)
                    iUTM_Zone_Num = 35;
                else if (longitude >= 33.0 && longitude < 42.0)
                    iUTM_Zone_Num = 37;
            }

            return (iUTM_Zone_Num);
        }

        public static string GetHemisphere(double dLat)
        {
            if (dLat >= 0)
                return (CoordinateSystem.NorthernHemisphere);
            return (CoordinateSystem.SouthernHemisphere);
        }

        private static double CentralMeridianFromUtmZone(int zoneNum)
        {
            double centralMeridian = (183.0 - (6.0 * (double)zoneNum)) * -1.0;
            return centralMeridian;
        }


        private static void UtmToWgs84(UtmPosition utm, ref GeoLocation gps)
        {
            utm.X -= 500000.0;
            utm.X /= UTMScaleFactor;

            // If in southern hemisphere, adjust y accordingly.
            if (utm.Hemisphere == CoordinateSystem.SouthernHemisphere)
            {
                utm.Y -= 10000000.0;
            }

            utm.Y /= UTMScaleFactor;

            //Debug.Log("UtmToWgs84: UtmPosition = " + utm.X + ", " + utm.Y + ", " + utm.Z);

            var cmeridian = UTMCentralMeridian(utm.Zone);
            MapXYToLatLon(utm, cmeridian, ref gps);

            return;
        }

        private static double UTMCentralMeridian(int zone)
        {
            return ConvertToRadians(-183.0 + (zone * 6.0));
        }

        private static void MapXYToLatLon(UtmPosition utm, double lambda0, ref GeoLocation gps)
        {
            double phif, Nf, NfPow, nuf2, ep2, tf, tf2, tf4, cf;
            double x1frac, x2frac, x3frac, x4frac, x5frac, x6frac, x7frac, x8frac;
            double x2poly, x3poly, x4poly, x5poly, x6poly, x7poly, x8poly;

            /* Get the value of phif, the footpoint latitude. */
            phif = FootpointLatitude(utm.Y);

            //Debug.Log("MapXYToLatLon: phif = " + phif);

            /* Precalculate ep2 */
            ep2 = (Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0)) / Math.Pow(sm_b, 2.0);

            /* Precalculate cos (phif) */
            cf = Math.Cos(phif);

            /* Precalculate nuf2 */
            nuf2 = ep2 * Math.Pow(cf, 2.0);

            /* Precalculate Nf and initialize NfPow */
            Nf = Math.Pow(sm_a, 2.0) / (sm_b * Math.Sqrt(1 + nuf2));
            NfPow = Nf;

            /* Precalculate tf */
            tf = Math.Tan(phif);
            tf2 = tf * tf;
            tf4 = tf2 * tf2;

            /* Precalculate fractional coefficients for x**n in the equations
               below to simplify the expressions for latitude and longitude. */
            x1frac = 1.0 / (NfPow * cf);

            NfPow *= Nf;   /* now equals Nf**2) */
            x2frac = tf / (2.0 * NfPow);

            NfPow *= Nf;   /* now equals Nf**3) */
            x3frac = 1.0 / (6.0 * NfPow * cf);

            NfPow *= Nf;   /* now equals Nf**4) */
            x4frac = tf / (24.0 * NfPow);

            NfPow *= Nf;   /* now equals Nf**5) */
            x5frac = 1.0 / (120.0 * NfPow * cf);

            NfPow *= Nf;   /* now equals Nf**6) */
            x6frac = tf / (720.0 * NfPow);

            NfPow *= Nf;   /* now equals Nf**7) */
            x7frac = 1.0 / (5040.0 * NfPow * cf);

            NfPow *= Nf;   /* now equals Nf**8) */
            x8frac = tf / (40320.0 * NfPow);

            /* Precalculate polynomial coefficients for x**n.
               -- x**1 does not have a polynomial coefficient. */
            x2poly = -1.0 - nuf2;

            x3poly = -1.0 - 2 * tf2 - nuf2;

            x4poly = 5.0 + 3.0 * tf2 + 6.0 * nuf2 - 6.0 * tf2 * nuf2
                - 3.0 * (nuf2 * nuf2) - 9.0 * tf2 * (nuf2 * nuf2);

            x5poly = 5.0 + 28.0 * tf2 + 24.0 * tf4 + 6.0 * nuf2 + 8.0 * tf2 * nuf2;

            x6poly = -61.0 - 90.0 * tf2 - 45.0 * tf4 - 107.0 * nuf2
                + 162.0 * tf2 * nuf2;

            x7poly = -61.0 - 662.0 * tf2 - 1320.0 * tf4 - 720.0 * (tf4 * tf2);

            x8poly = 1385.0 + 3633.0 * tf2 + 4095.0 * tf4 + 1575 * (tf4 * tf2);

            // Calculate latitude
            var latitude = phif + x2frac * x2poly * (utm.X * utm.X)
                + x4frac * x4poly * Math.Pow(utm.X, 4.0)
                + x6frac * x6poly * Math.Pow(utm.X, 6.0)
                + x8frac * x8poly * Math.Pow(utm.X, 8.0);

            // Calculate longitude
            var longitude = lambda0 + x1frac * utm.X
                + x3frac * x3poly * Math.Pow(utm.X, 3.0)
                + x5frac * x5poly * Math.Pow(utm.X, 5.0)
                + x7frac * x7poly * Math.Pow(utm.X, 7.0);

            gps.Latitude = ConvertToDegrees(latitude);
            gps.Longitude = ConvertToDegrees(longitude);
        }

        private static double FootpointLatitude(double utmY)
        {
            double y_, alpha_, beta_, gamma_, delta_, epsilon_, n;

            /* Precalculate n (Eq. 10.18) */
            n = (sm_a - sm_b) / (sm_a + sm_b);

            /* Precalculate alpha_ (Eq. 10.22) */
            /* (Same as alpha in Eq. 10.17) */
            alpha_ = ((sm_a + sm_b) / 2.0)
                * (1 + (Math.Pow(n, 2.0) / 4) + (Math.Pow(n, 4.0) / 64));

            /* Precalculate y_ (Eq. 10.23) */
            y_ = utmY / alpha_;

            /* Precalculate beta_ (Eq. 10.22) */
            beta_ = (3.0 * n / 2.0) + (-27.0 * Math.Pow(n, 3.0) / 32.0)
                + (269.0 * Math.Pow(n, 5.0) / 512.0);

            /* Precalculate gamma_ (Eq. 10.22) */
            gamma_ = (21.0 * Math.Pow(n, 2.0) / 16.0)
                + (-55.0 * Math.Pow(n, 4.0) / 32.0);

            /* Precalculate delta_ (Eq. 10.22) */
            delta_ = (151.0 * Math.Pow(n, 3.0) / 96.0)
                + (-417.0 * Math.Pow(n, 5.0) / 128.0);

            /* Precalculate epsilon_ (Eq. 10.22) */
            epsilon_ = (1097.0 * Math.Pow(n, 4.0) / 512.0);

            /* Now calculate the sum of the series (Eq. 10.21) */
            var result = y_ + (beta_ * Math.Sin(2.0 * y_))
                + (gamma_ * Math.Sin(4.0 * y_))
                + (delta_ * Math.Sin(6.0 * y_))
                + (epsilon_ * Math.Sin(8.0 * y_));

            return result;
        }

        //function LatLonToUTMXY(lat, lon, zone, xy)
        //{
        //    MapLatLonToXY(lat, lon, UTMCentralMeridian(zone), xy);

        //    /* Adjust easting and northing for UTM system. */
        //    xy[0] = xy[0] * UTMScaleFactor + 500000.0;
        //    xy[1] = xy[1] * UTMScaleFactor;
        //    if (xy[1] < 0.0)
        //        xy[1] = xy[1] + 10000000.0;

        //    return zone;
        //}
    }
}
