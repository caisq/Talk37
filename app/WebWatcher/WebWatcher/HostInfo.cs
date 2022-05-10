//using Microsoft.Toolkit.Uwp.Input.GazeInteraction.Device;
using Tobii.StreamEngine;

namespace WebWatcher
{
    class HostInfo
    {
        // Version string for the host app.
        private static string VERSION_MAJOR = "0";
        private static string VERSION_MINOR = "0";
        private static string VERSION_PATCH = "6";
        private static string VERSION =
            $"{VERSION_MAJOR}.{VERSION_MINOR}.{VERSION_PATCH}";
        public static string GetSerializedHostInfo()
        {
            string tobiiStreamEngineVersion = "";
            if (Interop.tobii_get_api_version(out var version) == tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                tobiiStreamEngineVersion =
                    $"{version.major}.{version.minor}.{version.revision}.{version.build}";
            }
            string hostInfoString =
                $"{{\"hostAppVersion\": \"{VERSION}\", \"engineVersion\": \"{tobiiStreamEngineVersion}\"}}";
            return hostInfoString;
        }
    }
}
