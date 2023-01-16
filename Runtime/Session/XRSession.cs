using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.Core
{
    public delegate void SessionReadyAction();


    [Serializable]
    public class XrSession 
    {
        internal event SessionReadyAction OnSessionReady;

        private GeoLocation _startLocation;
        private XrSesssionPoseManager _sesssionPoseManager;

        internal XrSession(GeoLocation location)
        {            
            _startLocation = location;
            _sesssionPoseManager = new XrSesssionPoseManager(location);
        }

        public GeoLocation Location  => _sesssionPoseManager.Location;            
        public Quaternion Orientation => _sesssionPoseManager.Orientation;
        public Vector3 PositionOffset => _sesssionPoseManager.PositionOffset;
        public Quaternion RotationOffset => _sesssionPoseManager.RotationOffset;
        public Vector3 Shift => _sesssionPoseManager.Shift;
        public Quaternion ShiftRotation => _sesssionPoseManager.ShiftRotation;

        public void SetPrintDebug(bool _value) => _sesssionPoseManager.SetPrintDebug(_value);

        public void RegisterProvider<T>(T provider) where T : IProvider
        {            
            SturfeeDebug.Log($" Registering {provider.GetType().Name} as {typeof(T).Name}");
            IOC.Register<T>(provider);
            provider.OnRegister();

            SturfeeEventManager.RegisterProvider(provider);
        }

        public void UnregisterProvider<T>() where T : IProvider
        {
            var provider = IOC.Resolve<T>();
            if (provider != null)
            {
                SturfeeDebug.Log($" Unregistering {provider.GetType().Name} as {typeof(T).Name}");
                IOC.Unregister<T>(provider);
                provider.OnUnregister();
            }

            SturfeeEventManager.UnregisterProvider(provider);
        }
        
        public T GetProvider<T>() where T: IProvider
        {
            var provider = IOC.Resolve<T>();
            if(provider == null)
            {
                //Debug.Log($"Cannot get {typeof(T).Name}. It is not registered");
            }

            return provider;
        }

        public void CreateSession()
        {
            var location = _startLocation;

            PositioningUtils.Init(location);

            OnSessionReady?.Invoke();
        }
    }
}
