using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Video provider interface.
    /// </summary>
    public interface IVideoProvider : IProvider
    {
        /// <summary>
        /// Returns current frame of video as an image
        /// </summary>
        /// <returns></returns>
        Texture2D GetCurrentFrame();

        /// <summary>
        /// Gets height of the video texture
        /// </summary>
        /// <returns>The height.</returns>
        int GetHeight();

        /// <summary>
        /// Gets width of the video texture
        /// </summary>
        /// <returns>The width.</returns>
        int GetWidth();

        /// <summary>
        /// Determines whether the screen orientation is portrait.
        /// </summary>
        /// <returns><c>true</c> if this instance is portrait; otherwise, <c>false</c>.</returns>
        bool IsPortrait();

        /// <summary>
        /// Gets projection matrix of Device Camera
        /// </summary>
        /// <returns>The projection matrix.</returns>
        Matrix4x4 GetProjectionMatrix();

        /// <summary>
        /// Gets Field of View of Device Camera
        /// </summary>
        /// <returns>The fov.</returns>
        float GetFOV();
    }
}
