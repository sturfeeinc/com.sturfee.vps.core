using System;
using System.IO;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public static class Paths
    {
        internal static readonly string TileCacheDir = Path.Combine(Application.persistentDataPath, "TileCache");
        public static readonly string SturfeeResourcesRelative = Path.Combine("Sturfee", "SturfeeConfiguration");
        public static readonly string SturfeeResourcesAbsolute = Path.Combine(Path.Combine(Application.dataPath, "Resources"), "Sturfee");

        public static readonly string ConfigFile = "SturfeeConfiguration.txt";

    }
}