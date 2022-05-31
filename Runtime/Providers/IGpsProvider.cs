using UnityEngine;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Gps provider interface.
    /// </summary>
    public interface IGpsProvider : IProvider
    {
        /// <summary>
        /// Gets the current Geo-spatial Location.
        /// </summary>
        /// <returns>The Geo-spatial Location.</returns>
        GeoLocation GetCurrentLocation();
    }
}