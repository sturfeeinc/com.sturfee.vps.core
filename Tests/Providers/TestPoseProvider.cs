using SturfeeVPS.Core;
using UnityEngine;

public class TestPoseProvider : TestProvider, IPoseProvider
{
    public float HeightFromGround = 1.5f;
    public Vector3 Position;
    public Quaternion Rotation;

    public void Destroy()
    {
        //throw new System.NotImplementedException();
    }

    public float GetHeightFromGround()
    {
        return HeightFromGround;
    }

    public Quaternion GetRotation()
    {
        return Rotation;
    }

    public Vector3 GetPosition()
    {
        return Position;
    }

    public ProviderStatus GetProviderStatus()
    {
        return ProviderStatus;
    }

    public void Initialize()
    {
        //throw new System.NotImplementedException();
    }
}
