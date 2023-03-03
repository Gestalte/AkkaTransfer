using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public class SendFileActor : ReceiveActor
    {
        private readonly IActorRef sendRouter;

        public SendFileActor()
        {
            var props = Props
                .Create<SendPartActor>()
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            this.sendRouter = Context.ActorOf(props);

            Receive<string>(SendFile);
            Receive<MissingFilePart>(SendMissingFilePart);
        }

        public void SendFile(string filename)
        {
            SendFileHeaderRepository sendFileHeaderRepository = new SendFileHeaderRepository(new DbContextFactory());

            var fileHeader = sendFileHeaderRepository.GetFileHeaderByFilename(filename);

            List<FilePartMessage> messages;

            messages = fileHeader.SendFilePieces
                .Select(s => new FilePartMessage(s.Content, s.Position, fileHeader.PieceCount, filename))
                .ToList();

            foreach (var message in messages)
            {
                sendRouter.Tell(message);
            }
        }

        public void SendMissingFilePart(MissingFilePart missingPart)
        {
            SendFileHeaderRepository sendFileHeaderRepository = new SendFileHeaderRepository(new DbContextFactory());

            var fileHeader = sendFileHeaderRepository.GetFileHeaderByFilename(missingPart.Filename);

            List<FilePartMessage> parts = fileHeader.SendFilePieces
                .Where(s => missingPart.MissingPiecePositions.Contains(s.Position))
                .Select(s => new FilePartMessage(s.Content, s.Position, missingPart.MissingPiecePositions.Count, missingPart.Filename))
                .ToList();

            foreach (var part in parts)
            {
                sendRouter.Tell(part);
            }
        }
    }
}
