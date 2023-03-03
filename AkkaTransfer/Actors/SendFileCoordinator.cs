using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public sealed class SendFileCoordinator : ReceiveActor
    {
        //private readonly ISendFileHeaderRepository sendFileHeaderRepository;
        private readonly SendFileHeaderRepositoryFactory sendFileHeaderRepositoryFactory;

        //public SendFileCoordinator(ISendFileHeaderRepository sendFileHeaderRepository)
        public SendFileCoordinator(SendFileHeaderRepositoryFactory sendFileHeaderRepositoryFactory)
        {
            //this.sendFileHeaderRepository = sendFileHeaderRepository;

            this.sendFileHeaderRepositoryFactory = sendFileHeaderRepositoryFactory;

            var sendProps = Props.Create(() => new SendFileActor(this.sendFileHeaderRepositoryFactory))
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

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

            Receive<MissingFileParts>(missingParts =>
            {
                missingParts.FileParts.ForEach(s => sendFileRouter.Tell(s));
            });
        }
    }
}
