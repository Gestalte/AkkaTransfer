using Akka.Actor;
using Akka.Actor.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer.Actors
{
    public class SendFileMessage
    {
        public SendFileMessage(string fileName)
        {
            FileName = fileName;
        }
        public string FileName { get; set; }
    }

    public class FilePayloadMessage
    {
        public FilePayloadMessage(string fileName, byte[] bytes)
        {
            FileName = fileName;
            Bytes = bytes;
        }

        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
    }

    internal class SendFileActor : ReceiveActor
    {
        private readonly FileBox sendFileBox;

        public SendFileActor(FileBox sendFilebox)
        {
            this.sendFileBox = sendFilebox;

            Receive<SendFileMessage>(message => Handle(message));
        }

        private void Handle(SendFileMessage message)
        {
            var fileName = message.FileName;

            var pathToSend = this.sendFileBox.GetFilesInBox()
                .Where(s => Path.GetFileName(s) == fileName)
                .FirstOrDefault();

            if (pathToSend != null)
            {
                var bytes = File.ReadAllBytes(pathToSend);

                var receiveActor = Context.ActorSelection("akka.tcp://server-system@10.0.0.106:8081/user/receive-file-actor");

                receiveActor.Tell(new FilePayloadMessage(fileName, bytes));
            }
        }
    }
}
