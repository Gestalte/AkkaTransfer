using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class FileSendBox : FileBox
    {
        public override void MakeBox()
        {
            var directory = Directory.GetCurrentDirectory();

            var sendBoxInfo = Directory.CreateDirectory(Path.Combine(directory, "SendBox"));

            BoxPath = sendBoxInfo.FullName;
        }
    }
}
