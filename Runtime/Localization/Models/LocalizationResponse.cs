using System;
using UnityEngine;

namespace SturfeeVPS.Core
{
    [Serializable]
    public class LocalizationResponse
    {
        public GeoLocation location;
        public Quaternion yawOrientationCorrection;
        public Quaternion pitchOrientationCorrection;
    }
}
