﻿//using Microsoft.Toolkit.Uwp.Input.GazeInteraction.Device;

namespace WebWatcher
{
    class HostInfo
    {
        // Version string for the host app.
        private static string VERSION_MAJOR = "0";
        private static string VERSION_MINOR = "0";
        private static string VERSION_PATCH = "4";
        private static string VERSION =
            $"{VERSION_MAJOR}.{VERSION_MINOR}.{VERSION_PATCH}";
        public static string GetSerializedHostInfo()
        {
            string tobiiStreamEngineVersion = "";
            // TODO(cais): Add reference. Prevent leak and failure to quit.
            //if (GazeDevice.Instance != null)
            //{
                //tobiiStreamEngineVersion = GazeDevice.Instance.GetVersion();
            //}
            string hostInfoString =
                $"{{\"hostAppVersion\": \"{VERSION}\", \"engineVersion\": \"{tobiiStreamEngineVersion}\"}}";
            return hostInfoString;
        }
    }
}
