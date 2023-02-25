using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using System.Runtime.InteropServices;

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

                string base64 = Convert.ToBase64String(bytes);

                Console.WriteLine(base64);

                // if base64.Length / 1000 has a rest, add 1 so that an incomplete batch is still created.
                var batchCount = (base64.Length / batchSize) + ((base64.Length % batchSize) > 0 ? 1 : 0);
                var rest = base64.Length % batchSize; // Size of the last batch that doesn't fill the entire batchSize.
                bool hasRest = rest > 0;

                FilePartMessage[] filePartMessages = new FilePartMessage[batchCount];

                for (int i = 0; i < batchCount; i++)
                {
                    var newString = hasRest && i == batchCount - 1
                        ? base64.Substring(i * batchSize, rest)
                        : base64.Substring(i * batchSize, batchSize);

                    filePartMessages[i] = new FilePartMessage(newString, i, batchCount, filename);
                }

                Props props = Props.Create<SendPartActor>().WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));
                var sendRouter = Context.ActorOf(props);

                foreach (var filePartMessage in filePartMessages)
                {
                    sendRouter.Tell(filePartMessage);
                }
            });
        }
    }
}
