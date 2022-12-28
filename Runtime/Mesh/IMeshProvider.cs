using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public interface IMeshProvider 
    {
        Task<byte[]> GetMesh();

    }
}
