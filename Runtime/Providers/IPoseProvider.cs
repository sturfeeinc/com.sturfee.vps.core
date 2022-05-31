using UnityEngine;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Pose provider interface.
    /// </summary>
    public interface IPoseProvider : IProvider
    {
        /// <summary>
        /// Gets orientation of device in world coordinate system
        /// </summary>
        /// <returns>The orientation.</returns>
        Quaternion GetOrientation();

        /// <summary>
        /// Gets relative position of device in world coordinate system
        /// </summary>
        /// <returns>The position.</returns>
        Vector3 GetPosition();

        /// <summary>
        /// Gets height of device from ground 
        /// </summary>
        /// <returns></returns>
        float GetHeightFromGround();
    }
}