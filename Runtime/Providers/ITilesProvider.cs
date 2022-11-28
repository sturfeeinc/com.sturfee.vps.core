using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public delegate void TilesLoadedAction();
    public delegate void TilesLoadingFailAction(string error);
    public interface ITilesProvider : IProvider
    {
        Task<GameObject> GetTiles(GeoLocation location, float radius = 0, CancellationToken cancellationToken = default);
        Task<GameObject> GetTiles(CancellationToken cancellationToken = default);
        float GetElevation(GeoLocation location);
        event TilesLoadedAction OnTileLoaded;
        event TilesLoadingFailAction OnTileLoadingFail;
    }
}
