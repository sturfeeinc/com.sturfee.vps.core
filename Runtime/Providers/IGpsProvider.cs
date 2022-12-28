using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Gps provider interface.
    /// </summary>
    public interface IGpsProvider : IProvider
    {
        /// <summary>
        /// Gets precise current Geo-spatial Location.
        /// </summary>
        /// <returns>The Geo-spatial Location.</returns>
        GeoLocation GetFineLocation(out bool includesElevation);

        /// <summary>
        /// Gets approximate current Geo-spatial Location.
        /// </summary>
        /// <returns>The Geo-spatial Location.</returns>        
        GeoLocation GetApproximateLocation(out bool includesElevation);

    }
}
