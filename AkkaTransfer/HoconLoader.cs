using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public static class HoconLoader
    {
        public static Config FromFile(string path)
        {
            var hoconContent = System.IO.File.ReadAllText(path);
            return ConfigurationFactory.ParseString(hoconContent);
        }
    }
}
