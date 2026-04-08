namespace SemiconductorControlSystem.Models
{
    public class AppConfig
    {
        public string StationName { get; set; } = "MidControl";

        public string DeviceIp { get; set; } = "127.0.0.1";

        public string DevicePort { get; set; } = "502";

        public string PollingIntervalMs { get; set; } = "1000";

        public bool AutoStart { get; set; }

        public string Theme { get; set; } = "light";
    }
}
