﻿using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;

namespace AkkaTransfer.Actors
{
    public class SendFileCoordinator : ReceiveActor
    {
        public SendFileCoordinator(FileBox sendFilebox)
        {
            var props = Props.Create(() => new SendFileActor(sendFilebox))
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 100)));

            var sendFileRouter = Context.ActorOf(props);

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
        }
    }

    public class SendFileActor : ReceiveActor
    {
        private const int batchSize = 125;

        public SendFileActor(FileBox sendFilebox)
        {
            var props = Props
                .Create<SendPartActor>()
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            var sendRouter = Context.ActorOf(props);

            Receive<string>(filename =>
            {
                var pathToSend = FileBox.FindFilePath(filename, sendFilebox);

                var messages = SplitIntoMessages(pathToSend, filename);

                foreach (var message in messages)
                {
                    sendRouter.Tell(message);
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
