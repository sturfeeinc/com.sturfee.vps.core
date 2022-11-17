using SturfeeVPS.Core;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TestTilesProvider : TestProvider, ITilesProvider
{
    public float Elevation;

    public event TilesLoadedAction OnTileLoaded;

    public void Destroy()
    {
        //throw new System.NotImplementedException();
    }

    public float GetElevation(GeoLocation location)
    {
        return Elevation;
    }

    public ProviderStatus GetProviderStatus()
    {
        return ProviderStatus;
    }

    public void Initialize()
    {
        //throw new System.NotImplementedException();
    }

    public Task<GameObject> LoadTiles(GeoLocation location, float radius, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public void TriggerTileLoadEvent()
    {
        OnTileLoaded?.Invoke();
    }
}
