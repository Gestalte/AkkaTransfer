using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class FileSendBox
    {
        public FileSendBox()
        {
            MakeSendBox();
        }

        public string SendBoxPath { get; set; } 

        public void MakeSendBox()
        {
            var directory = Directory.GetCurrentDirectory();

            var sendBoxInfo=Directory.CreateDirectory(Path.Combine(directory, "SendBox"));

            SendBoxPath = sendBoxInfo.FullName;
        }

        public List<string> GetFilesToSend() 
            => Directory.GetFiles(SendBoxPath).ToList();
    }
}
