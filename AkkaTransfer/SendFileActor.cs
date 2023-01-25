using Akka.Actor;
using Akka.Actor.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer
{
    public class SendFileMessage
    {
        public string FileName { get; set; }
    }

    public class FilePayloadMessage
    {
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
    }

    internal class SendFileActor : ReceiveActor
    {
        public FileSendBox Box { get; set; }

        public SendFileActor(FileSendBox box)
        {
            Box = box;

            Receive<SendFileMessage>(message => Handle(message));
        }

        private void Handle(SendFileMessage message)
        {
            var fileName = message.FileName;

            var pathToSend = Box.GetFilesInBox().Where(s => Path.GetFileName(s) == fileName).FirstOrDefault();

            if (pathToSend != null)
            {
                var bytes = File.ReadAllBytes(pathToSend);

                var payload = new FilePayloadMessage
                {
                    FileName = fileName,
                    Bytes = bytes
                };

                var props = Props.Create<ReceiveFileActor>(new FileReceiveBox());

                var receiveActor = Context.ActorOf(props);

                receiveActor.Tell(payload);
            }
        }
    }
}
