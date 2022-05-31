using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;

namespace SturfeeVPS.Core
{
    internal class TileService
    {
        public int CacheRadius = 100;
        public int CacheExpiry = 10;    // number of days
        private GeoLocation _centerReference;

        public GeoLocation CenterReference
        {
            get
            {
                if(_centerReference == null)
                {
                    string json = File.ReadAllText(Path.Combine(Paths.TileCacheDir, "Cache.txt"));
                    _centerReference = JsonUtility.FromJson<TileCacheMetaData>(json).Location;
                }
                return _centerReference;
            }            
        }

        public async Task LoadTiles(GeoLocation location, int radius, string token, CancellationToken cancellationToken = default )
        {
            SturfeeDebug.Log(" Loading tiles...");

            GameObject tilesGO = new GameObject(SturfeeObjects.SturfeeTilesObject);
            tilesGO.transform.rotation = Quaternion.Euler(-90, 180, 0);

            string uri = Path.Combine(Paths.TileCacheDir, "sturfee_tiles.gltf");
            if (InCache(location))
            {
                SturfeeDebug.Log(" Loading from Cache");
                await LoadGltf(tilesGO, uri,cancellationToken);
            }
            else
            {
                SturfeeDebug.Log(" Downloading tiles");
                try
                {
                    byte[] gltf = await WebServices.DownloadTiles(location, radius, token);
                    SaveToCache(gltf, location);
                }
                catch (HttpException e)
                {
                    SturfeeDebug.LogError(e.Message);
                    throw new SessionException(ErrorMessages.TileDownloadingError);
                }

                await LoadGltf(tilesGO, uri, cancellationToken);
            }

            SetupTiles(tilesGO);
            SturfeeDebug.Log("Tiles loaded");
        }

        private async Task LoadGltf(GameObject tilesGO, string uri, CancellationToken cancellationToken = default)
        {
            SturfeeDebug.Log(" Loading Gltf...");

            try
            {
                var importOptions = new ImportOptions
                {
                    AsyncCoroutineHelper = tilesGO.GetComponent<AsyncCoroutineHelper>() ?? tilesGO.AddComponent<AsyncCoroutineHelper>()
                };

                GLTFSceneImporter sceneImporter = null;

                ImporterFactory factory = ScriptableObject.CreateInstance<DefaultImporterFactory>();

                string directoryPath = URIHelper.GetDirectoryName(uri);
                importOptions.DataLoader = new FileLoader(directoryPath);
                sceneImporter = factory.CreateSceneImporter(
                    Path.GetFileName(uri),
                    importOptions
                    );

                sceneImporter.SceneParent = tilesGO.transform;
                sceneImporter.Collider = GLTFSceneImporter.ColliderType.Mesh;
                sceneImporter.MaximumLod = 300;
                sceneImporter.Timeout = 100;
                sceneImporter.IsMultithreaded = false;
                sceneImporter.CustomShaderName = "Sturfee/Transparent";

                await sceneImporter.LoadSceneAsync(0, true, null, cancellationToken);
                await sceneImporter.LoadSceneAsync(1, true, null, cancellationToken);

                if (importOptions.DataLoader != null)
                {
                    sceneImporter?.Dispose();
                    sceneImporter = null;
                    importOptions.DataLoader = null;
                }
            }
            catch
            {
                throw new IdException(ErrorMessages.TileLoadingError);
            }
        }

        private void SetupTiles(GameObject tilesGO)
        {
            Transform building = tilesGO.transform.GetChild(0);
            if (building != null)
            {
                building.gameObject.name = "Building";
                for (int i = 0; i < building.childCount; i++)
                {
                    building.GetChild(i).gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.Building);
                    building.GetChild(i).GetComponent<MeshRenderer>().material =
                        Resources.Load<Material>("Materials/BuildingHide");
                    building.GetChild(i).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            Transform terrain = tilesGO.transform.GetChild(1);
            if (terrain != null)
            {
                terrain.gameObject.name = "Terrain";
                for (int i = 0; i < terrain.childCount; i++)
                {
                    terrain.GetChild(i).gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.Terrain);
                    terrain.GetChild(i).GetComponent<MeshRenderer>().material =
                        Resources.Load<Material>("Materials/BuildingHide");
                    terrain.GetChild(i).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

        }

        private bool InCache(GeoLocation location)
        {
            string tiles = Path.Combine(Paths.TileCacheDir, "sturfee_tiles.gltf");
            string tileCache = Path.Combine(Paths.TileCacheDir, "Cache.txt");

            if (!Directory.Exists(Paths.TileCacheDir))
                return false;

            if (File.Exists(tiles) && File.Exists(tileCache))
            {
                string json = File.ReadAllText(Path.Combine(Paths.TileCacheDir, "Cache.txt"));

                var tileCacheMetaData = JsonUtility.FromJson<TileCacheMetaData>(json);

                var centerReference = tileCacheMetaData.Location;

                if (centerReference == null)
                {
                    throw new IdException(ErrorMessages.TileLoadingErrorFromCache);
                }

                SturfeeDebug.Log("Center Ref (Cache): " + centerReference.ToFormattedString(), false);

                UtmPosition centerUTM = GeoCoordinateConverter.GpsToUtm(centerReference);
                UtmPosition utmPosition = GeoCoordinateConverter.GpsToUtm(location);                

                // if within cache radius
                if (Vector3.Distance(utmPosition.ToUnityVector3(), centerUTM.ToUnityVector3()) > CacheRadius)
                {
                    return false;
                }

                // if within expiry time-limt
                DateTime cached = new DateTime();
                if (DateTime.TryParse(tileCacheMetaData.TimeStamp, out cached))
                {
                    if ((DateTime.Now- cached).TotalDays > CacheExpiry)
                    {
                        return false;
                    }
                }
            }

            return false;

        }

        private static void SaveToCache(byte[] gltf, GeoLocation location)
        {
            SturfeeDebug.Log("Center Ref (Server): " + location.ToFormattedString(), false);

            string dir = Paths.TileCacheDir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string gltfPath = Path.Combine(dir, "sturfee_tiles.gltf");
            if (File.Exists(gltfPath))
            {
                File.Delete(gltfPath);
            }

            File.WriteAllBytes(gltfPath, gltf);
            TileCacheMetaData tileCacheMetaData = new TileCacheMetaData
            {
                Location = location,
                TimeStamp = DateTime.Now.ToString("O")
            };

            File.WriteAllText(Path.Combine(dir, "Cache.txt"), JsonUtility.ToJson(tileCacheMetaData));
        }
    }

    
}