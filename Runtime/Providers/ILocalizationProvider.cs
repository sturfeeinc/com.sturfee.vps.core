using UnityEngine;

namespace SturfeeVPS.Core
{
    public delegate void LocalizationRequestedAction();
    public delegate void LocalizationStartAction();
    public delegate void LocalizationStopAction();
    public delegate void LocalizationLoadingAction();
    public delegate void LocalizationSuccessfulAction();
    public delegate void LocalizationFailAction(string error);
    public delegate void LocalizationDisabledAction();


    public interface ILocalizationProvider : IProvider
    {
        OffsetType OffsetType {  get; }
        Quaternion YawOffset {  get; }
        Quaternion PitchOffset { get; }
        Quaternion RollOffset { get; }

        Vector3 EulerOffset { get; }
        Quaternion RotationOffset { get; }
        int FrameNumber { get; }
        GeoLocation Location { get; }

        GeoLocation GetVpsLocation(out bool includesElevation);

        void EnableLocalization();
        void StopLocalization();
        void DisableLocalization();

        event LocalizationRequestedAction OnLocalizationRequested;
        event LocalizationStartAction OnLocalizationStart;
        event LocalizationLoadingAction OnLocalizationLoading;
        event LocalizationSuccessfulAction OnLocalizationSuccessful;
        event LocalizationFailAction OnLocalizationFail;
        event LocalizationDisabledAction OnLocalizationDisabled;
    }
}
