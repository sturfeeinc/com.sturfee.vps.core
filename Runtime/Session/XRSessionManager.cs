using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.Core
{
    public static class XrSessionManager
    {
        private static XrSession _session;

        public static XrSession CreateSession(GeoLocation location)
        {
            if(_session != null)
            {
                SturfeeDebug.LogError("Cannot create multiple XrSessions");
                throw new Exception("Cannot create multiple XrSessions");
            }

            _session = new XrSession(location);
            _session.OnSessionReady += OnSessionReady;

            _session.CreateSession();

            return _session;
        }

        public static XrSession GetSession()
        {
            return _session;
        }

        public static void DestroySession()
        {
            if (_session != null)
            {
                _session.OnSessionReady -= OnSessionReady;
                _session = null;

                SturfeeEventManager.SessionDestroy();
            }
        }

        private static void OnSessionReady()
        {
            SturfeeEventManager.SessionReady();
        }

    }
}
