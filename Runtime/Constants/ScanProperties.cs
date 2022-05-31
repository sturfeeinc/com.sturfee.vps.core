using UnityEngine;
namespace SturfeeVPS.Core
{
    public static class ScanProperties
    {
        public static float YawAngle
        {
            get
            {
                return PlayerPrefs.GetFloat("Sturfee.VPS.Core.YawAngle", Defaults.YawAngle);
            }
        }

        public static int TargetCount
        {
            get
            {
                return PlayerPrefs.GetInt("Sturfee.VPS.Core.TargetCount", Defaults.TargetCount);
            }
        }

        public static int PitchMin
        {
            get
            {
                return PlayerPrefs.GetInt("Sturfee.VPS.Core.PitchMin", Defaults.PitchMin);
            }
        }
        public static int PitchMax
        {
            get
            {
                return PlayerPrefs.GetInt("Sturfee.VPS.Core.PitchMax", Defaults.PitchMax);
            }
        }
        public static int RollhMin
        {
            get
            {
                return PlayerPrefs.GetInt("Sturfee.VPS.Core.RollMin", Defaults.RollMin);
            }
        }
        public static int RollMax
        {
            get
            {
                return PlayerPrefs.GetInt("Sturfee.VPS.Core.RollMax", Defaults.RollMax);
            }
        }

        internal static int InitialiRadius
        {
            get
            {
                return PlayerPrefs.GetInt("Sturfee.VPS.Core.InitialiRadius", Defaults.InitialRadius);
            }
        }

        internal static float ScoreThreshold = 15f;

        //Relocalization
        internal static int RelocRadius = 6;
        internal static int RelocYaw = 10;
        internal static int RelocPitch = 3;

        public static class Defaults
        {
            public static readonly int TargetCount = 8;
            public static readonly float YawAngle = 30;
            public static readonly int PitchMin = -30;
            public static readonly int PitchMax = -5;
            public static readonly int RollMin = -10;
            public static readonly int RollMax = 10;
            public static readonly int InitialRadius = 30;
        }
    }
}
