using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public class SendFileCoordinator : ReceiveActor
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
        private const int batchSize = 125;

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
                    messages = SplitIntoMessages(pathToSend!, filename).ToList();
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

        private static FilePartMessage[] SplitIntoMessages(string pathToSend, string filename)
        {
            var bytes = File.ReadAllBytes(pathToSend);
            var base64 = Convert.ToBase64String(bytes);

            Console.WriteLine(base64);

            // if base64.Length / batchSize has a rest, add 1 so that an
            // incomplete batch is still created.
            var batchCount = (base64.Length / batchSize) + ((base64.Length % batchSize) > 0 ? 1 : 0);
            var rest = base64.Length % batchSize; // Size of the last batch that doesn't fill the entire batchSize.
            var hasRest = rest > 0;

            var filePartMessages = new FilePartMessage[batchCount];

            for (int i = 0; i < batchCount; i++)
            {
                var newString = hasRest && i == batchCount - 1
                    ? base64.Substring(i * batchSize, rest)
                    : base64.Substring(i * batchSize, batchSize);

                filePartMessages[i] = new FilePartMessage(newString, i, batchCount, filename);
            }

            return filePartMessages;
        }
    }
}
