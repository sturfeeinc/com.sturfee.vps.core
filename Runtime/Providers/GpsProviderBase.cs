using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Abstract base class for GpsProvider that inherits from Monobehaviour
    /// </summary>
    public class GpsProviderBase : MonoBehaviour,IGpsProvider
    {
        // <summary>
        /// Initialize this Provider. Will be called when session is getting created
        /// </summary>
        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the current GPS Location.
        /// </summary>
        /// <returns>The GPS Location.</returns>
        public virtual GeoLocation GetCurrentLocation()
        {            
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets provider's current status
        /// </summary>
        /// <returns>The provider status.</returns>
        public virtual ProviderStatus GetProviderStatus()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Prepare this provider to be ready for scanning 
        /// </summary>
        public async virtual Task PrepareForScan(CancellationToken token)
        {
            SturfeeDebug.Log($" Preparing GPSProvider for scan");
        }

        public IEnumerator PrepareForScan()
        {
            yield return null;
        }

        /// <summary>
        /// Destroy any Objects/GameObjects that were cretaed using this provider
        /// </summary>
        public virtual void Destroy()
        {
            throw new System.NotImplementedException();
        }        
    }
}