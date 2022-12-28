using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public class WorldMeshProvider : IMeshProvider
    {
        public async Task<byte[]> GetMesh()
        {
            await Task.Yield();
            return null;
        }

        private async Task<byte[]> DownloadMesh()
        {
            await Task.Yield();
            return null;
        }
    }
}
