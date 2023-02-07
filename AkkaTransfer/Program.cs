using Akka.Actor;
using Akka.Util.Internal;
using AkkaTransfer;
using AkkaTransfer.Actors;

var hocon = HoconLoader.FromFile("akka.net.hocon");
ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

FileBox fileSendBox = new("SendBox");
FileBox fileReceiveBox = new("ReceiveBox");

Props sendProps = Props.Create(typeof(SendFileActor), fileSendBox);
Props receiveProps = Props.Create(typeof(ReceiveFileActor), fileReceiveBox);

IActorRef sendFileActor = system.ActorOf(sendProps, "send-file-actor");
IActorRef receiveFileActor = system.ActorOf(receiveProps, "receive-file-actor");




// Send files.
fileSendBox.GetFilesInBox()
    .Select(filePath => Path.GetFileName(filePath))
    .ForEach(fileName =>
    {
        sendFileActor.Tell(fileName);
    });


while (true)
{
    Thread.Sleep(1000);
}