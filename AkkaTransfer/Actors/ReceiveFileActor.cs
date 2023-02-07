using Akka.Actor;
using AkkaTransfer.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer.Actors
{
    public class ReceiveFileActor : ReceiveActor
    {
        public FileBox Box { get; }

        public ReceiveFileActor(FileBox box)
        {
            Box = box;

            Receive<FilePartMessage>(message => Handle(message));
        }

        private void Handle(FilePartMessage message)
        {
            Console.WriteLine($"Receive part {message.Position} of {message.Count}");

            // TODO: Save FilePartMessage to db
        }
    }
}
