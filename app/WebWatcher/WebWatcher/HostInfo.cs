namespace WebWatcher
{
    class HostInfo
    {

        // Version string for the host app.
        private static string VERSION = "0.0.3";
        public static string GetSerializedHostInfo()
        {
            string hostInfoString = $"{{\"hostAppVersion\": \"{VERSION}\"}}";
            return hostInfoString;
        }
    }
}
