using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{
    /// <summary>
    /// Determines if the provider supports the device it is running on.
    /// </summary>
    /// <returns><c>true</c> if the device is supported; otherwise, <c>false</c>.</returns>
    public interface IProvider
    {
        /// <summary>
        /// Initialize this Provider.
        /// </summary>
        void OnRegister();

        /// <summary>
        /// Gets provider's current status
        /// </summary>
        /// <returns>The provider status.</returns>
        ProviderStatus GetProviderStatus();

        /// <summary>
        /// Destroy any Objects/GameObjects that were created using this provider
        /// </summary>
        void OnUnregister();

    }
}
