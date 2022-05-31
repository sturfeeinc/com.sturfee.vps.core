using System;
using System.IO;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal enum LogType
    {
        Log,
        Error,
        Warning
    }

    public static class SturfeeDebug
    {
        public static readonly string STURFEE_XR = "SturfeeXR";

        private static LogType _logType = LogType.Log;
        private static readonly string _fileName = "sturfee_debug_log.txt";
        private static string _fileLocation;

        internal static string FileLocation
        {
            get
            {
                if (string.IsNullOrEmpty(_fileLocation))
                {
                    _fileLocation = Application.persistentDataPath + "/" + _fileName;
                }
                return _fileLocation;
            }

        }

        public static void Log(string info, bool addToConsole = true)
        {
            File.AppendAllText(FileLocation, DateTime.Now.ToString(@"MM/dd/yyyy hh:mm:ss.f") + ": " + "[" + STURFEE_XR + "] : " + info + Environment.NewLine);

            if (addToConsole)
            {
                AddToConsole("[" + STURFEE_XR + "] : " + info, LogType.Log);
            }
        }

        public static void LogError(string info, bool addToConsole = true)
        {
            File.AppendAllText(FileLocation, DateTime.Now.ToString(@"MM/dd/yyyy hh:mm:ss.f") + ": " + "[" + STURFEE_XR + "] : " + info + Environment.NewLine);

            if (addToConsole)
            {
                AddToConsole("[" + STURFEE_XR + "] : " + info, LogType.Error);
            }
        }

        public static void LogWarning(string info, bool addToConsole = true)
        {
            File.AppendAllText(FileLocation, DateTime.Now.ToString(@"MM/dd/yyyy hh:mm:ss.f") + ": " + "[" + STURFEE_XR + "] : " + info + Environment.NewLine);

            if (addToConsole)
            {
                AddToConsole("[" + STURFEE_XR + "] : " + info, LogType.Warning);
            }
        }

        private static void AddToConsole(string info, LogType logType)
        {
            switch (logType)
            {
                case LogType.Log: Debug.Log(info); return;
                case LogType.Error: Debug.LogError(info); return;
                case LogType.Warning: Debug.LogWarning(info); return;
            }

        }

    }
}
