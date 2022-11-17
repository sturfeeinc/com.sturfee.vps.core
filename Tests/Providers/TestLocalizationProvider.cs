using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TestLocalizationProvider : TestProvider, ILocalizationProvider
{
    public OffsetType OffsetType { get; set; }

    public Quaternion YawOffset { get; set; }

    public Quaternion PitchOffset { get; set; }

    public Quaternion RollOffset { get; set; }

    public Vector3 EulerOffset { get; set; }

    public GeoLocation VpsLocation { get; set; }

    public event LocalizationLoadingAction OnLocalizationLoading;
    public event LocalizationSuccessfulAction OnLocalizationSuccessful;
    public event LocalizationFailAction OnLocalizationFail;
    public event LocalizationDisabledAction OnLocalizationDisabled;
    public event LocalizationRequestedAction OnLocalizationRequested;
    public event LocalizationStartAction OnLocalizationStart;

    public void Destroy()
    {
        //throw new NotImplementedException();
    }

    public void DisableLocalization()
    {
        throw new NotImplementedException();
    }

    public void EnableLocalization()
    {
        throw new NotImplementedException();
    }

    public ProviderStatus GetProviderStatus()
    {
        return ProviderStatus;
    }

    public void Initialize()
    {
        //throw new NotImplementedException();
    }

    public void TriggerLocalizationRequestedEvent()
    {
        OnLocalizationRequested?.Invoke();
    }

    public void TriggerLocalizationStartEvent()
    {
        OnLocalizationStart?.Invoke();
    }

    public void TriggerLocalizationLoadingEvent()
    {
        OnLocalizationLoading?.Invoke();
    }

    public void TriggerLocalizationFailEvent(string error)
    {
        OnLocalizationFail?.Invoke(error);
    }

    public void TriggerLocalizationSuccessfulEvent()
    {
        OnLocalizationSuccessful?.Invoke();
    }

    public void TriggerLocalizationDisabledEvent()
    {
        OnLocalizationDisabled?.Invoke();
    }
}

