using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SturfeeVPS.Core;
using UnityEngine;
namespace SturfeeVPS.Core
{
    /// <summary>
    /// Abstract base class for VideoProvider that inherits from Monobehaviour
    /// </summary>
    public class VideoProviderBase : MonoBehaviour,IVideoProvider
    {
        // <summary>
        /// Initialize this Provider. Will be called when session is getting created
        /// </summary>
        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns current frame of video as an image
        /// </summary>
        /// <returns>A Texture2D frame</returns>
        public virtual Texture2D GetCurrentFrame()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets Field of View of Device Camera
        /// </summary>
        /// <returns>The fov.</returns>
        public virtual float GetFOV()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets height of the video texture
        /// </summary>
        /// <returns>The height.</returns>
        public virtual int GetHeight()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets projection matrix of Device Camera
        /// </summary>
        /// <returns>The projection matrix.</returns>
        public virtual Matrix4x4 GetProjectionMatrix()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets width of the video texture
        /// </summary>
        /// <returns>The width.</returns>
        public virtual int GetWidth()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Determines whether the screen orientation is portrait.
        /// </summary>
        /// <returns><c>true</c> if orientation is portrait; otherwise, <c>false</c>.</returns>
        public virtual bool IsPortrait()
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
            SturfeeDebug.Log($" Preparing VideoProvider for scan");
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