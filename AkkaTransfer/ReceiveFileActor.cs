using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class ReceiveFileActor : ReceiveActor
    {
        public FileReceiveBox Box { get; }

        public ReceiveFileActor(FileReceiveBox box)
        {
            Box = box;

            Receive<FilePayloadMessage>(message => Handle(message));
        }

        private void Handle(FilePayloadMessage message)
        {
            var name = message.FileName;
            var bytes = message.Bytes;

            var savePath = Box.ReceiveBoxPath;

            File.WriteAllBytes(Path.Combine(savePath, name), bytes);
        }
    }
}
