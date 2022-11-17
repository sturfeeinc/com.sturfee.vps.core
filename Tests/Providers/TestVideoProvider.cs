using SturfeeVPS.Core;
using UnityEngine;

public class TestVideoProvider : TestProvider, IVideoProvider
{
    public Texture2D CurrentFrame;
    public Matrix4x4 ProjectionMatrix;
    public int SceneWidth;
    public int SceneHeight;
    public float FOV;
    public bool Portrait;

    public void Destroy()
    {
        //throw new System.NotImplementedException();
    }

    public Texture2D GetCurrentFrame()
    {
        return CurrentFrame;
    }

    public float GetFOV()
    {
        return FOV;
    }

    public int GetHeight()
    {
        return SceneHeight;
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return ProjectionMatrix;
    }

    public ProviderStatus GetProviderStatus()
    {
        return ProviderStatus;
    }

    public int GetWidth()
    {
        return SceneWidth;
    }

    public void Initialize()
    {
        //throw new System.NotImplementedException();
    }

    public bool IsPortrait()
    {
        return Portrait;
    }
}