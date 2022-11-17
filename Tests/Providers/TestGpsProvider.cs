using SturfeeVPS.Core;

public class TestGpsProvider : TestProvider, IGpsProvider
{
    public GeoLocation ApproximateLocation;
    public GeoLocation FineLocation;

    public void Destroy()
    {
        //throw new System.NotImplementedException();
    }

    public GeoLocation GetApproximateLocation()
    {
        return ApproximateLocation;
    }

    public GeoLocation GetFineLocation()
    {
        if(ProviderStatus == ProviderStatus.Ready)
        {
            return FineLocation;
        }

        return null;
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
