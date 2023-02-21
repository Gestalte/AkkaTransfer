using Akka.Actor;
using Akka.Event;
using AkkaTransfer.Data;
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
        private readonly IFileHeaderRepository fileHeaderRepository;

        public FileBox Box { get; }

        public ReceiveFileActor(FileBox box, IFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;

            Box = box;

            Receive<FilePartMessage>(message => Handle(message));
        }

        private void Handle(FilePartMessage message)
        {
            System.Diagnostics.Debug.WriteLine($"Receive part {message.Position} of {message.Count}");

            this.fileHeaderRepository.AddNewPieceUnitOfWork(message);
        }
    }
}
