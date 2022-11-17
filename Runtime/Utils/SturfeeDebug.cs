using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var assemblyName = Assembly.GetCallingAssembly().GetName().Name;
            var tag = $"SturfeeXR.{assemblyName.Split('.').Last()}";
            File.AppendAllText(FileLocation, DateTime.Now.ToString(@"MM/dd/yyyy hh:mm:ss.f") + ": " + "[" + tag + "] : " + info + Environment.NewLine);

            if (addToConsole)
            {
                AddToConsole("[" + tag + "] : " + info, LogType.Log);
            }
        }

        public static void LogError(string info, bool addToConsole = true)
        {
            var assemblyName = Assembly.GetCallingAssembly().GetName().Name;
            var tag = $"SturfeeXR.{assemblyName.Split('.').Last()}";
            File.AppendAllText(FileLocation, DateTime.Now.ToString(@"MM/dd/yyyy hh:mm:ss.f") + ": " + "[" + tag + "] : " + info + Environment.NewLine);

            if (addToConsole)
            {
                AddToConsole("[" + tag + "] : " + info, LogType.Error);
            }
        }

        public static void LogWarning(string info, bool addToConsole = true)
        {
            var assemblyName = Assembly.GetCallingAssembly().GetName().Name;
            var tag = $"SturfeeXR.{assemblyName.Split('.').Last()}";
            File.AppendAllText(FileLocation, DateTime.Now.ToString(@"MM/dd/yyyy hh:mm:ss.f") + ": " + "[" + tag + "] : " + info + Environment.NewLine);

            if (addToConsole)
            {
                AddToConsole("[" + tag + "] : " + info, LogType.Warning);
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
