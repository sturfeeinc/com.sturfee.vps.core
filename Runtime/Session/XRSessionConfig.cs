using System;
namespace SturfeeVPS.Core
{
    /// <summary>
    /// Session Configuration needed to create any XRSession
    /// </summary>
    public class XRSessionConfig
    {
        /// <summary>
        /// GpsProvider to use while creating XRSession
        /// </summary>
        public IGpsProvider GpsProvider;
        /// <summary>
        /// PoseProvider to use while creating XRSession
        /// </summary>
        public IPoseProvider PoseProvider;
        /// <summary>
        /// VideoProvider to use while creating XRSession
        /// </summary>
        public IVideoProvider VideoProvider;

        internal string Locale = "en-US";
        internal TileSize TileSize = TileSize.Small;
        internal string AccessToken;
        //internal XRTier XRTier;
        //internal bool LoadTiles = true;
        //internal int TargetCount;
        //internal int YawAngle;
    }
}
