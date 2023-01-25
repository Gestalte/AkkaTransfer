using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class FileReceiveBox : FileBox
    {
        public override void MakeBox()
        {
            var directory = Directory.GetCurrentDirectory();

            var receiveBoxnfo = Directory.CreateDirectory(Path.Combine(directory, "ReceiveBox"));

            BoxPath = receiveBoxnfo.FullName;
        }
    }
}
