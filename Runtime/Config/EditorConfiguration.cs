using System;
namespace SturfeeVPS.Core
{
    [Serializable]
    public class EditorConfiguration
    {
        public string AccessToken;
        public TileSize TileSize = TileSize.Small;
        public Theme Theme;
        public Cache Cache;
        public Scan Scan;        
    }

    [Serializable]
    public class Cache
    {
        public float Distance;
        public int ExpirationTime;        
    }

    [Serializable]
    public class Theme
    {
        public string Path;
        public string Locale;
    }

    [Serializable]
    public class Scan
    {
        public float YawAngle;
        public int TargetCount;
        public int PitchMin;
        public int PitchMax;
        public int RollMin;
        public int RollMax;
        public int InitialRadius;
    }
}
