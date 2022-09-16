using UnityEngine;
namespace SturfeeVPS.Core
{
    public static class ScanProperties
    {
        public static float YawAngle { internal set; get; }
        public static int TargetCount { internal set; get; }
        public static int PitchMin { internal set; get; }
        public static int PitchMax { internal set; get; }
        public static int RollMin { internal set; get; }
        public static int RollMax { internal set; get; }
        public static int InitialiRadius { internal set; get; }

        internal static float ScoreThreshold = 15f;

        //Relocalization
        internal static int RelocRadius = 6;
        internal static int RelocYaw = 10;
        internal static int RelocPitch = 3;

        public static class Defaults
        {
            public static class Satellite
            {
                public static readonly int TargetCount = 8;
                public static readonly float YawAngle = 30;
                public static readonly int PitchMin = -30;
                public static readonly int PitchMax = -5;
                public static readonly int RollMin = -10;
                public static readonly int RollMax = 10;
                public static readonly int InitialRadius = 30;
            }

            public static class HD
            {
                public static readonly int TargetCount = 8;
                public static readonly float YawAngle = 6;
                public static readonly int PitchMin = -180;
                public static readonly int PitchMax = 180;
                public static readonly int RollMin = -10;
                public static readonly int RollMax = 10;
                public static readonly int InitialRadius = 30;
            }
            
        }
    }
}
