using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{
    public class LocationProvider : IGpsProvider
    {
        public bool Override;

        private IGpsProvider _provider;

        private GeoLocation _location = new GeoLocation();

        public LocationProvider(IGpsProvider gpsProvider)
        {
            _provider = gpsProvider;
        }

        public void Initialize()
        {
            if (Override) return;

            _provider.Initialize();
        }

        public GeoLocation GetCurrentLocation()
        {
            if (Override)   return _location;
            
            return _provider.GetCurrentLocation();
        }

        public ProviderStatus GetProviderStatus()
        {
            if (Override) return ProviderStatus.Ready;

            return _provider.GetProviderStatus();
        }


        public void Destroy()
        {
            if (Override) return;

            _provider.Destroy();
        }

        public void OverrideCurrentLocation(GeoLocation location)
        {
            _location = location;
        }

        public Task PrepareForScan(CancellationToken token)
        {
            return _provider.PrepareForScan(token);
        }
    }
}
