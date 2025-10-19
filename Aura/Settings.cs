namespace Aura
{
    public class Settings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string CustomPrompt { get; set; } = string.Empty;
        public bool ProxyEnabled { get; set; } = false;
        public string ProxyAddress { get; set; } = string.Empty;
    }
}
