using System;
namespace SturfeeVPS.Core
{
    [System.Serializable]
    public class UtmPosition
    {
        public double X; // Easting
        public double Y; // Northing
        public double Z; 
        public int Zone; 
        public string Hemisphere; 

        public double Easting
        {
            get { return X; }
            set { X = value; }
        }
        public double Northing
        {
            get { return Y; }
            set { Y = value; }
        }

        public UtmPosition() { }

        public UtmPosition(UtmPosition utm)
        {
            X = utm.X;
            Y = utm.Y;
            Z = utm.Z;
            Zone = utm.Zone;
            Hemisphere = utm.Hemisphere;
        }
    }
}
