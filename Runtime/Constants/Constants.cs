namespace SturfeeVPS.Core.Constants
{
    /// <summary>
    /// Digital Twin and Digital Twin HD API end-points.
    /// </summary>
    public static class DtConstants
    {
        public static readonly string LOCAL_SPACES_PATH = "DigitalTwin/Spaces";
        public static readonly string LOCAL_ASSETS_PATH = "DigitalTwin/Assets";

        public static readonly string SPACES_API = "https://sharedspaces-api.sturfee.com/api/v2.0";

        public static readonly string DTE_OUTDOOR_TILES_API = "https://digitaltwin.sturfee.com/street/tiles/zip";
        public static readonly string DTE_OUTDOOR_COVERAGE_API = "https://digitaltwin.sturfee.com/street/tiles/zip/coverage";

        public static readonly string SturfeeXrSessionVR = "SturfeeXrSession-VR";
        public static readonly string SturfeeXrSessionARVR = "SturfeeXrSession-AR+VR";

        public static readonly string DTHD_LAYOUT = "https://digitaltwin.sturfee.com/hd/layout";
        private static readonly string _TestID = "3745b04f-7465-4533-b84f-406690685845";

    }

    /// <summary>
    /// Shop App and ShopKeeper App end-points. 
    /// </summary>
    public static class ShopConstants
    {
        public static readonly string SHOP_APP_API_URL = "https://shopapp.sturfee.com"; // "http://shopapp.devsturfee.com";

        public static readonly string LOCAL_THUMBNAILS_PATH = "LOCAL_CACHE/THUMBNAILS";
    }
}
