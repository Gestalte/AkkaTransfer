using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class SendManifest
    {
        public List<string> FileNames { get; set; } = new List<string>();
    }

    public class ReceiveManifest
    {
        public List<string> FileNames { get; set; } = new List<string>();
    }
}
