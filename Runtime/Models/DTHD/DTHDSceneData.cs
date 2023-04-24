using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SturfeeVPS.Core.Models
{
    /// <summary>
    /// Data structure for instances of DTHD asset
    /// </summary>
    [Serializable]
    public class DtHdAssetItem
    {
        public string DtHdAssetId;
        public string DtHdAssetItemId;
        public string Name;
        public DateTime? CreatedDate;
        public DateTime? UpdatedDate;
        public GeoLocation Location;
        public float LocalX;
        public float LocalY;
        public float LocalZ;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;
        public float Scale;
    }

    /// <summary>
    /// Types of DTHD asset
    /// </summary>
    [Serializable]
    public enum AssetType
    {
        Prop,
        ProductGroup,
        EditableSurface
    }

    /// <summary>
    /// Data structure for DTHD assets. These contain meta data about an asset used in a DTHD scene. These are not actual instances of the asset in a DTHD scene. Instances require positional and rotational information, which are served by SturfeeVPS.Core.Models.DtHdAssetItem.
    /// </summary>
    [Serializable]
    public class DtHdAsset
    {
        public string DtHdAssetId;
        public string Name;
        public string Description;
        public List<DtHdAssetItem> Items;
        public string FileUrl;
        public int FileSizeBytes;
        public string Format;
        public DateTime? CreatedDate;
        public DateTime? UpdatedDate;
        public AssetType AssetType;
        public string ExternalRefId;
        public string EditMode;
        public string EditRole;
        public string PhysicsMode;
    }

    /// <summary>
    /// Data structure for DTHD 
    /// </summary>
    [Serializable]
    public class DtHdLayout
    {
        public string DtHdId;
        public string UserId;
        public string Name;
        public GeoLocation Location;
        public double RefX;
        public double RefY;
        public double RefZ;
        public DateTime? CreatedDate;
        public DateTime? UpdatedDate;
        public int FileSizeBytes;
        public float SpawnPositionX;
        public float SpawnPositionY;
        public float SpawnPositionZ;
        public float SpawnHeading;
        public bool IsIndoor;
        public bool IsPublic;
        public List<ScanMesh> ScanMeshes;
        public string EnhancedMesh;
        public List<DtHdAsset> Assets;
        public string DtEnvironmentUrl;
    }

    /// <summary>
    /// Data structure for scan meshes associated with a DTHD scene
    /// </summary>
    [Serializable]
    public class ScanMesh
    {
        public string DtHdScanId;
        public string Status;
        public string SiteName;
        public string Thumbnail;
        public DateTime? CreatedDate;
        public DateTime? UpdatedDate;
        public GeoLocation ScanLocation;
        public double RefX;
        public double RefY;
        public double RefZ;
        public int Floor;
        public string ScanMeshUrl;
        public VpsHdSite VpsHdSite;

        public string MediaStatus;
        public string DtScanStatus;
        public string AnchorStatus;
    }

    // [Serializable]
    // public class VpsHdSite
    // {
    //     [JsonProperty("site_id")]
    //     public string SiteId;
    //     public string Name;
    //     [JsonProperty("dthd_id")]
    //     public string DtHdId;
    //     [JsonProperty("dtscan_id")]
    //     public string DtScanId;

    // }

    /// <summary>
    /// Data structure for a VPS HD site
    /// </summary>
    [Serializable]
    public class VpsHdSite
    {
        public string thumbnailUrl;
        public SiteInfo siteInfo;
        public string anchorMesh;
    }

    /// <summary>
    /// Data structure for VPS HD site meta data
    /// </summary>
    [Serializable]
    public class SiteInfo
    {
        public string site_id;
        public string name;
        public string dthd_id;
        public string dtscan_id;
        public string thumbnail_id;
        public DateTime? createdDate;
        public DateTime? updatedDate;
        public int floor;
        public bool isIndoor;
        public double refX;
        public double refY;
        public double refZ;
        public string source;
        public string platform;
        public string s3_key;
        public double longitude;
        public double latitude;
        public int utm_lon_zone;
        public string utm_lat_zone;
        public float radius;
        public bool active;
        public float terrainAdjustment;
        public float projectionErrorThreshold;
    }

    // data for DtHd Environment.json
    /// <summary>
    /// Container for SturfeeVPS.Core.Models.UnityEnvironment
    /// </summary>
    [Serializable]
    public class DtEnvironment
    {
        public UnityEnvironment Unity;
    }

    /// <summary>
    /// Container for SturfeeVPS.Core.Models.UnityReflectionProbe and SturfeeVPS.Core.Models.UnityLight
    /// </summary>
    [Serializable]
    public class UnityEnvironment
    {
        public UnityReflectionProbe[] ReflectionProbes;
        public UnityLight[] Lights;
    }

    /// <summary>
    /// Types of reflection probe: Baked, Custom and Realtime
    /// </summary>
    public enum ReflectionProbeType
    {
        Baked,
        Custom,
        Realtime
    }

    /// <summary>
    /// Data structure for reflection probe instance
    /// </summary>
    [Serializable]
    public class UnityReflectionProbe
    {
        public string ReflectionProbeId;
        public string DtHdId;
        public string UserId;

        public string Name;
        public int Importance;
        public float Intensity;
        public bool BoxProjection;
        public float BoxSizeX;
        public float BoxSizeY;
        public float BoxSizeZ;
        public ReflectionProbeType Type;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;

        public double Lat;
        public double Lon;
        public double Alt;

        public float LocalX;
        public float LocalY;
        public float LocalZ;
    }

    /// <summary>
    /// Types of light: Point, Spot and Directional
    /// </summary>
    public enum SturfeeLightType
    {
        Point,
        Spot,
        Directional
    }

    /// <summary>
    /// Types of shadow: NoShadows, HardShadows and SoftShadows
    /// </summary>
    public enum ShadowType
    {
        NoShadows,
        HardSadows,
        SoftShadows
    }

    /// <summary>
    /// Types of LightMode: RealTime, Mixed, Baked
    /// </summary>
    public enum LightMode
    {
        RealTime,
        Mixed,
        Baked
    }

    /// <summary>
    /// Data structure for light instance
    /// </summary>
    [Serializable]
    public class UnityLight
    {
        public string LightId;
        public string DtHdId;
        public string UserId;

        public string Name;
        public SturfeeLightType LightType;

        public bool IsMainLight;

        public float Range;
        public float SpotAngle;

        public float ColorR;
        public float ColorG;
        public float ColorB;

        public float Intensity;
        public float ShadowStrength;
        public ShadowType ShadowType;
        public LightMode LightMode;

        public DateTime CreatedDate;
        public DateTime UpdatedDate;

        public double Lat;
        public double Lon;
        public double Alt;

        public float LocalX;
        public float LocalY;
        public float LocalZ;

        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;
    }
}
