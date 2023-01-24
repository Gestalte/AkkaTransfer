using Akka.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class FileDto
    {
        public FileDto(ByteString bytes)
        {
            FileInBytes = bytes;
        }

        ByteString FileInBytes { get; }
    }
}
