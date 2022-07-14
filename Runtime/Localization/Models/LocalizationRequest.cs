using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace SturfeeVPS.Core
{
    [Serializable]
    public class LocalizationRequest
    {
        public int requestId;
        public Multiframe frame;
        public string siteId;
        public ExternalParameters externalParameters;
        public InternalParameters internalParameters;
    }

    [Serializable]
    public class Multiframe
    {
        public int count;
        public int order;
    }

    [Serializable]
    public class ExternalParameters
    {
        public double latitude;
        public double longitude;
        public double height;
        public Quaternion quaternion;
    }

    [Serializable]
    public class InternalParameters
    {
        public int sceneWidth;
        public int sceneHeight;
        public float fov;
        public int isPortrait;
        public Matrix4x4 projectionMatrix;
    }
}
