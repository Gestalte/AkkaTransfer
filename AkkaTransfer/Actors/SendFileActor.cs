using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public class SendFileActor : ReceiveActor
    {
        private readonly ISendFileHeaderRepository sendFileHeaderRepository;
        private readonly IActorRef sendRouter;

        private readonly SendFileHeaderRepositoryFactory sendFileHeaderFactory;

        public SendFileActor(SendFileHeaderRepositoryFactory sendFileHeaderFactory)
        {
            this.sendFileHeaderFactory = sendFileHeaderFactory;
            this.sendFileHeaderRepository = this.sendFileHeaderFactory.Create();

            var props = Props
                .Create<SendPartActor>()
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            this.sendRouter = Context.ActorOf(props);

            Receive<string>(SendFile);
            Receive<MissingFilePart>(SendMissingFilePart);
        }

        public void SendFile(string filename)
        {
            var fileHeader = this.sendFileHeaderRepository.GetFileHeaderByFilename(filename);

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
            var fileHeader = this.sendFileHeaderRepository.GetFileHeaderByFilename(missingPart.Filename);

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
