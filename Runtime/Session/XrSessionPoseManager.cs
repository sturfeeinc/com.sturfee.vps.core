using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal class XrSessionPoseManager
    {
        private GeoLocation _startLocation;
        private GameObject _localizedOrigin;
        private Vector3 _shift;

        private GameObject _parent = new GameObject();
        private GameObject _child = new GameObject();

        public XrSessionPoseManager(GeoLocation location)
        {
            _startLocation = location;
            _localizedOrigin = new GameObject("_localizationHelper");

            SturfeeEventManager.OnLocalizationStart += OnLocalizationStart;
            SturfeeEventManager.OnLocalizationDisabled += OnLocalizationDisabled;
        }

        // may or may not need
        private void OnLocalizationDisabled()
        {
            _shift = Vector3.zero;
        }

        // FOR DEBUG
        private bool PrintDebug = false;
        public void SetPrintDebug(bool _value)
        {
            PrintDebug = _value;
        }

        public GeoLocation Location
        {
            get
            {
                // Location from GPS/VPS
                var location = GetLocation(out bool locationIncludesElevation);
                var world = PositioningUtils.GeoToWorldPosition(location);

                // delta from PoseProvider
                var delta = RotateWithOffset(GetDeltaPosition(out bool deltaIncludesElevation));
                
                // altitude
                float altitude = GetAltitude(world, delta, locationIncludesElevation, deltaIncludesElevation);

                // shift delta origin to where localization was started.
                // This is because after localization we need delta from only from localization point and not from center ref
                var shift = RotateWithOffset(Shift);

                // var result = world + delta - shift;
                // FOR DEBUG
                var result = world;
                // Debug.Log($"result: {result}");

                //Debug.Log($"Location : {location.ToFormattedString()}, world : {world}, delta : {delta} , shift : {shift}, altitude : {altitude} ");

                // FOR DEBUG
                if (PrintDebug)
                {
                    SturfeeDebug.Log($"[XrSesssionPoseManager.cs] [DEBUG BUTTON PRESS] world: {world}, result: {result}, shift without rotation offset: {Shift}, shift with rotation offset: {shift}, delta position without rotation offset: {GetDeltaPosition(out _)}, delta position with rotation offset: {delta}");
                }

                return PositioningUtils.WorldToGeoLocation(result);
            }
        }

        private GeoLocation GetLocation(out bool includesElevation)
        {
            includesElevation = false;
            var location = _startLocation;

            // First check if we have VPS location
            var localizationProvider = IOC.Resolve<ILocalizationProvider>();
            var gpsProvider = IOC.Resolve<IGpsProvider>();

            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)        // have localiationProvider it's own status =>  notLocalized, localizing, loading, localized
            {
                location = localizationProvider.GetVpsLocation(out includesElevation);
            }
            // If no VPS, check if we have GPS location
            else if (gpsProvider != null)
            {
                if (gpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    if (gpsProvider.GetApproximateLocation(out includesElevation).Latitude != 0 && gpsProvider.GetApproximateLocation(out includesElevation).Longitude != 0)
                    {
                        location = gpsProvider.GetApproximateLocation(out includesElevation);
                    }
                    else
                    {
                        SturfeeDebug.LogWarning($" Cannot determine GPS location. Make sure GetApproximateLocation is deterministic");
                    }
                }
                else
                {
                    location = gpsProvider.GetFineLocation(out includesElevation);
                }
            }
            else
            {
                location = PositioningUtils.WorldToGeoLocation(Vector3.zero);
            }
            // Debug.Log($"GetLocation: Lat: {location.Latitude}, Lon: {location.Longitude}, Alt: {location.Altitude}");
            return location;
        }

        public Quaternion Orientation
        {
            get
            {
                var sensor = GetDeltaRotation();
                var yawOffset = Quaternion.identity;
                var pitchOffset = Quaternion.identity;

                var localizationProvider = IOC.Resolve<ILocalizationProvider>();
                if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    yawOffset = localizationProvider.YawOffset;
                    pitchOffset = localizationProvider.PitchOffset;

                    if (localizationProvider.OffsetType == OffsetType.Euler)
                    {
                        var eulerOffset = localizationProvider.EulerOffset;
                        var offset = Quaternion.Euler(eulerOffset);
                        return offset * sensor;
                    }
                }
                Debug.Log($"[XrSesssionPoseManager] :: pitch offset: {pitchOffset}");
                return yawOffset * sensor * pitchOffset;
            }
        }

        // FOR DEBUG
        public Quaternion YawOffset
        {
            get
            {
                var sensor = GetDeltaRotation();
                var yawOffset = Quaternion.identity;
                var pitchOffset = Quaternion.identity;

                var localizationProvider = IOC.Resolve<ILocalizationProvider>();
                if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    yawOffset = localizationProvider.YawOffset;
                    pitchOffset = localizationProvider.PitchOffset;

                    if (localizationProvider.OffsetType == OffsetType.Euler)
                    {
                        var eulerOffset = localizationProvider.EulerOffset;
                        var offset = Quaternion.Euler(eulerOffset);
                        return offset * sensor;
                    }
                }

                return yawOffset;
            }
        }
        // FOR DEBUG
        public Quaternion PitchOffset
        {
            get
            {
                var sensor = GetDeltaRotation();
                var yawOffset = Quaternion.identity;
                var pitchOffset = Quaternion.identity;

                var localizationProvider = IOC.Resolve<ILocalizationProvider>();
                if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    yawOffset = localizationProvider.YawOffset;
                    pitchOffset = localizationProvider.PitchOffset;

                    if (localizationProvider.OffsetType == OffsetType.Euler)
                    {
                        var eulerOffset = localizationProvider.EulerOffset;
                        var offset = Quaternion.Euler(eulerOffset);
                        return offset * sensor;
                    }
                }
                
                return pitchOffset;
            }
        }

        public Vector3 PositionOffset
        {
            get
            {
                var localPos = PositioningUtils.GeoToWorldPosition(Location);

                // FOR DEBUG
                if (PrintDebug)
                    SturfeeDebug.Log($"[XrSesssionPoseManager.cs] [DEBUG BUTTON PRESS] localPos: {localPos}");

                var poseProvider = IOC.Resolve<IPoseProvider>();
                if(poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    // FOR DEBUG
                    // return localPos; // Jay's change
                    return localPos - RotateWithOffset(poseProvider.GetPosition(out _));
                }
                return localPos;
            }
        }

        public Quaternion RotationOffset {
            get
            {
                var sensor = Quaternion.identity;
                var poseProvider = IOC.Resolve<IPoseProvider>();
                if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    sensor = poseProvider.GetRotation();
                }

                return Orientation * Quaternion.Inverse(sensor);
            }
        }
        
        // FOR DEBUG
        // Jay's changes
        // public Quaternion RotationOffset {

        //     get
        //     {
        //         var sensor = Quaternion.identity;
        //         var poseProvider = IOC.Resolve<IPoseProvider>();
        //         if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
        //         {
        //             sensor = poseProvider.GetRotation();
        //         }

        //         return YawOffset;
        //     }
        // }

        private float GetAltitude(Vector3 world, Vector3 delta, bool locationIncludesElevation, bool deltaIncludesElevation)
        {
            float altitude;

            // if location from Gps/VPS includes elevation and location from PoseProvider(delta) also includes elevation
            if (locationIncludesElevation && deltaIncludesElevation)
            {
                //altitude = Math.Abs(world.z - delta.z) < 1 ? world.z : delta.z;
                altitude = world.z;
            }            
            // if only location from PoseProvider includes elevation 
            else if (deltaIncludesElevation)
            {
                altitude = delta.z;
            }
            // if only location from Gps/VPS includes elevation 
            else if (locationIncludesElevation)
            {
                altitude = world.z + delta.z;   
            }
            // Neither location nor PoseProvider include elevation. Calculate using Elevation and Camera Height from ground
            else
            {
                altitude = Elevation + HeightFromGround + delta.z;
            }

            return altitude;
        }

        public Vector3 Shift
        {
            get
            {
                var localizationProvider = IOC.Resolve<ILocalizationProvider>();
                if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    Debug.Log($"Shift: {_shift}");
                    return _shift;
                }
                return Vector3.zero;
            }
        }

        private float Elevation
        {
            get
            {
                var tilesProvider = IOC.Resolve<ITilesProvider>();
                if(tilesProvider != null && tilesProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    return tilesProvider.GetElevation(GetLocation(out bool includesElevation));
                }
                return 0;
            }
        }

        private float HeightFromGround
        {
            get
            {
                var poseProvider = IOC.Resolve<IPoseProvider>();
                if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                {                 
                    return poseProvider.GetHeightFromGround();
                }
                return 0;
            }
        }

        private Vector3 GetDeltaPosition(out bool includesElevation)
        {
            includesElevation = false;

            // we don't want delta before localization if we have Gps
            var localizationProvider = IOC.Resolve<ILocalizationProvider>();
            var gpsProvider = IOC.Resolve<IGpsProvider>();
            var poseProvider = IOC.Resolve<IPoseProvider>();

            // prefer gps over delta when session is not localized
            if(gpsProvider != null && gpsProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                // gps ready but localiztion is not
                if (localizationProvider == null || localizationProvider.GetProviderStatus() != ProviderStatus.Ready)
                {
                    return Vector3.zero;
                }
            }


            if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var position = poseProvider.GetPosition(out includesElevation);
                return position;
            }


            return Vector3.zero ;
        }

        private Quaternion GetDeltaRotation()
        {
            var poseProvider = IOC.Resolve<IPoseProvider>();
            if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var rotation = poseProvider.GetRotation();
                return rotation;
            }

            return Quaternion.identity;
        }

        /// <summary>
        /// Gets position in world obtained after applying Localization offset
        /// </summary>
        // private Vector3 RotateWithOffset(Vector3 pos)
        // {
        //     _localizedOrigin.transform.rotation = RotationOffset;
        //     var rotated = _localizedOrigin.transform.InverseTransformPoint(pos);
        //     return rotated;
        // }
        private Vector3 RotateWithOffset(Vector3 point)
        {
            
            _parent.transform.rotation = RotationOffset;

            _child.transform.parent = _parent.transform;
            _child.transform.localPosition = point;

            Vector3 result = _child.transform.position;

            return result;
        }


        private void OnLocalizationStart()
        {
            var poseProvider = IOC.Resolve<IPoseProvider>();
            if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                _shift = poseProvider.GetPosition(out bool includesElevation);
            }

            SturfeeDebug.Log($"[XRSessionPoseManager] :: OnLocalizationStart : DeltaAtLocalizationStart {_shift}");
        }
    }
}