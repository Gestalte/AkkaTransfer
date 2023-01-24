using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class FileReceiveBox
    {
        public FileReceiveBox()
        {
            MakeReceiveBox();
        }

        public string ReceiveBoxPath { get; set; }

        public void MakeReceiveBox()
        {
            var directory = Directory.GetCurrentDirectory();

            var receiveBoxnfo = Directory.CreateDirectory(Path.Combine(directory, "ReceiveBox"));

            ReceiveBoxPath = receiveBoxnfo.FullName;
        }

        public List<string> GetFilesReceived()
            => Directory.GetFiles(ReceiveBoxPath).ToList();
    }
}
