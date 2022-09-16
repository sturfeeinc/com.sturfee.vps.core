using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{
    [Serializable]
    public class ScanConfig
    {
        public Satellite Satellite;
        public HD HD;
    }

    [Serializable]
    public class Satellite
    {

    }
    [Serializable]
    public class HD
    {
        public string SiteId;
        public GeoLocation Location;
    }
}
