using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;

namespace AkkaTransfer.Actors
{
    public class SendFileActor : ReceiveActor
    {
        public SendFileActor(FileBox sendFilebox)
        {
            Receive<string>(filename =>
            {
                const int batchSize = 1000;

                var pathToSend = sendFilebox.GetFilesInBox()
                    .Where(s => Path.GetFileName(s) == filename)
                    .FirstOrDefault()
                    ?? throw new ArgumentException("File not found in SendBox", nameof(filename));

                byte[] bytes = File.ReadAllBytes(pathToSend);

                // if bytes.Length / 1000 has a rest, add 1 so that a batch is still created.
                var batchCount = (bytes.Length / batchSize) + ((bytes.Length % batchSize) > 0 ? 1 : 0);
                var rest = bytes.Length % batchSize; // Size of the last batch that doesn't fill the entire batchSize.
                bool hasRest = rest > 0;

                FilePartMessage[] filePartMessages = new FilePartMessage[batchCount];

                for (int i = 0; i < batchCount; i++)
                {
                    var newArray = (hasRest && i == batchCount - 1)
                        ? makeBatchArray(bytes, rest, i)
                        : makeBatchArray(bytes, batchSize, i);

                    filePartMessages[i] = new FilePartMessage(newArray, i, batchCount, filename);
                }

                Props props = Props.Create<SendPartActor>().WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));
                var sendRouter = Context.ActorOf(props);

                foreach (var filePartMessage in filePartMessages)
                {
                    sendRouter.Tell(filePartMessage);
                }
            });
        }

        private byte[] makeBatchArray(byte[] bytes, int size, int iter)
        {
            Byte[] newArray = new Byte[size];

            Array.Copy(bytes, iter * 1000, newArray, 0, size);

            return newArray;
        }
    }
}
