using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class TestProvider
{

    protected ProviderStatus ProviderStatus = ProviderStatus.Ready;
    public void SetProviderStatus(ProviderStatus providerStatus)
    {
        ProviderStatus = providerStatus;
    }
}

