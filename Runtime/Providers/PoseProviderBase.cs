using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Abstract base class for PoseProvider that inherits from Monobehaviour
    /// </summary>
    public abstract class PoseProviderBase : MonoBehaviour,IPoseProvider
    {
        // <summary>
        /// Initialize this Provider. Will be called when session is getting created
        /// </summary>
        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets orientation of device in world coordinate system   
        /// </summary>
        /// <returns></returns>
        public virtual Quaternion GetOrientation()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets relative position of device in world coordinate system
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 GetPosition()
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
        /// (Optional) Prepare this provider to be ready for scanning 
        /// </summary>
        public async virtual Task PrepareForScan(CancellationToken token)
        {

        }

        /// <summary>
        /// (Optional) Gets height of device from ground 
        /// </summary>
        /// <returns></returns>
        public virtual float GetHeightFromGround()
        {
            return 1.5f;
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