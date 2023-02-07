using Akka.Configuration;

namespace AkkaTransfer
{
    public static class HoconLoader
    {
        public static Config FromFile(string path)
        {
            var hoconContent = System.IO.File.ReadAllText(path);
            return ConfigurationFactory.ParseString(hoconContent);
        }

        public static string ReadSendIpAndPort(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}
