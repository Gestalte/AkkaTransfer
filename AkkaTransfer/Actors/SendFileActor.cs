using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public sealed class SendFileCoordinator : ReceiveActor
    {
        private readonly ISendFileHeaderRepository sendFileHeaderRepository;        

        public SendFileCoordinator(FileBox sendFilebox, ISendFileHeaderRepository sendFileHeaderRepository)
        {
            this.sendFileHeaderRepository = sendFileHeaderRepository;

            var sendProps = Props.Create(() => new SendFileActor(sendFilebox, this.sendFileHeaderRepository))
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 100)));

            var sendFileRouter = Context.ActorOf(sendProps);

            Receive<Manifest>(manifest =>
            {
                manifest.Files
                .Select(s => s.Filename)
                .ToList()
                .ForEach(filename =>
                {
                    sendFileRouter.Tell(filename);
                });
            });

            Receive<List<(string, List<int>)>>(missingParts =>
            {
                missingParts.ForEach(s => sendFileRouter.Tell(s));
            });
        }
    }

    public class SendFileActor : ReceiveActor
    {
        private readonly ISendFileHeaderRepository sendFileHeaderRepository;

        public SendFileActor(FileBox sendFilebox, ISendFileHeaderRepository sendFileHeaderRepository)
        {
            this.sendFileHeaderRepository = sendFileHeaderRepository;

            var props = Props
                .Create<SendPartActor>()
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            var sendRouter = Context.ActorOf(props);

            Receive<string>(filename =>
            {
                var fileHeader = this.sendFileHeaderRepository.GetFileHeaderByFilename(filename);

                List<FilePartMessage> messages;

                if (fileHeader != null)
                {
                    messages = fileHeader.SendFilePieces
                        .Select(s => new FilePartMessage(s.Content, s.Position, fileHeader.PieceCount, filename))
                        .ToList();
                }
                else
                {
                    var pathToSend = FileBox.FindFilePath(filename, sendFilebox);
                    messages = FileHelper.SplitIntoMessages(pathToSend!, filename).ToList();
                }

                foreach (var message in messages)
                {
                    sendRouter.Tell(message);
                }
            });

            Receive<(string, List<int>)>(missingPart =>
            {
                (string filename, List<int> missingPositions) = missingPart;

                var fileHeader = this.sendFileHeaderRepository.GetFileHeaderByFilename(filename);

                List<FilePartMessage> parts = fileHeader.SendFilePieces
                    .Where(s => missingPositions.Contains(s.Position))
                    .Select(s => new FilePartMessage(s.Content, s.Position, missingPositions.Count, filename))
                    .ToList();

                foreach (var part in parts)
                {
                    sendRouter.Tell(part);
                }
            });
        }
    }
}
