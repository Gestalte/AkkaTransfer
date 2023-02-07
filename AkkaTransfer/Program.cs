// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using Akka.Util.Internal;
using AkkaTransfer;
using AkkaTransfer.Actors;

FileBox fileSendBox = new("SendBox");
FileBox fileReceiveBox = new("ReceiveBox");

var hocon = HoconLoader.FromFile("akka.net.hocon");
ActorSystem system = ActorSystem.Create("server-system", hocon);

Props sendProps = Props.Create(typeof(SendFileActor), fileSendBox);
Props receiveProps = Props.Create(typeof(ReceiveFileActor), fileReceiveBox);

IActorRef sendFileActor = system.ActorOf(sendProps, "send-file-actor");
IActorRef receiveFileActor = system.ActorOf(receiveProps, "receive-file-actor");



fileSendBox.GetFilesInBox()
    .Select(filePath => Path.GetFileName(filePath))
    .ForEach(fileName =>
    {
        sendFileActor.Tell(new SendFileMessage(fileName));
    });
