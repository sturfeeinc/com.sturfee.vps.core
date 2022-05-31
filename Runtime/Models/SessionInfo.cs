using System;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.Analytics
{
    [Serializable]
    public class SessionInfo
    {
        public string Timestamp;
        public string SessionId;
        public string Token;
        public string Locale;
        public string TileSize;
        public string Status;
        public string TrackingId;
        public string SessionFail;
        public string CaptureType;
        public Images Images;
        public CenterRef CenterRef;
        public XRCamera XRCamera;
        public ARCamera ARCamera;
        public Scan[] Scans;
        public DeviceInfo DeviceInfo;
        public VpsCorrections VpsCorrections;
    }

    [Serializable]
    public class XRCamera
    {
        public Vector3 LocalPos;
        public Quaternion LocalRot;
        public Vector3 WorldPos;
        public Quaternion WorldRot;
        public GeoLocation Location;
        public UtmPosition UtmPosition;
        public Matrix4x4 ProjectionMatrix;
    }

    [Serializable]
    public class ARCamera
    {
        public Vector3 LocalPos;
        public Quaternion LocalRot;
        public Vector3 WorldPos;
        public Quaternion WorldRot;
        public Matrix4x4 ProjectionMatrix;
    }

    [Serializable]
    public class CenterRef
    {
        public GeoLocation Location;
        public UtmPosition UtmPosition;
    }

    [Serializable]
    public class Scan
    {
        public int ScanId;
        public ScanType ScanType;
        public LocalizationRequest[] Requests;
        public LocalizationResponse Response;
        public string TrackingId;
        public string LocalizationFail;
    }

    [Serializable]
    public class DeviceInfo
    {
        public string Model;
        public string OS;
    }

    [Serializable]
    public class VpsCorrections
    {
        public Quaternion YawOffset;
        public Quaternion PitchOffset;
        public GeoLocation VPSLocation;
    }

    [Serializable]
    public enum CaptureType
    {
        Request,
        Response,
        Other
    }

    [Serializable]
    public class Images
    {
        public string screenshot;
        public string raw;
    }
}