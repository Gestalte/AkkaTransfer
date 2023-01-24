using Akka.Actor;

namespace AkkaTransfer
{
    public class RequestReceivedFilesMessage { }

    public class ListReceivedFilesActor : ReceiveActor
    {
        public ListReceivedFilesActor()
        {
            var box = new FileReceiveBox();

            var result = new ReceiveManifest
            {
                FileNames = box.GetFilesReceived()
                .Select(s => Path.GetFileName(s))
                .ToList()
            };

            Receive<RequestReceivedFilesMessage>(message =>
            {
                Sender.Tell(result);
            });
        }
    }
}
