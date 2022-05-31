namespace SturfeeVPS.Core
{
    public static class XRSessionManager
    {
        private static XRSession _session;

        /// <summary>
        /// Creates an XRSession based on providers provided in <see cref="XRSessionConfig"/>
        /// </summary>
        /// <param name="xRSessionConfig"> Configuration for creating this session</param>
        public static void CreateSession(XRSessionConfig xRSessionConfig)
        {
            if (_session != null)
            {
                SturfeeDebug.LogError("An XRSession is already active. " +
                    "Cannot create multiple XRSessions. Please destroy current active XRSession before" +
                    "creating a new session.");
            }
            _session = new XRSession(xRSessionConfig);
        }

        /// <summary>   
        /// Gets Current active XRSession
        /// </summary>
        /// <returns><see cref="XRSession"/></returns>
        public static XRSession GetSession()
        {
            if (_session != null)
            {
                return _session;
            }
            else
            {
                SturfeeDebug.LogError("No active XRSession. Please create one using CreateSession()");
            }

            return null;
        }

        /// <summary>
        /// Destroys Current active XRSession
        /// </summary>
        public static void DestroySession()
        {
            if (_session != null)
            {
                _session.DestroySession();
                _session = null;
            }

            SturfeeEventManager.Destroy();
        }
    }
}
