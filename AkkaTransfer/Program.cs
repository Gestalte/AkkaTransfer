// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using Akka.Util.Internal;
using AkkaTransfer;

var hocon = HoconLoader.FromFile("akka.net.hocon");

ActorSystem system = ActorSystem.Create("server-system", hocon);

Props sendProps = Props.Create(typeof(SendFileActor), new FileSendBox());
Props receiveProps = Props.Create(typeof(ReceiveFileActor), new FileReceiveBox());

IActorRef sendFileActor = system.ActorOf(sendProps, "send-file-actor");
IActorRef receiveFileActor = system.ActorOf(receiveProps, "receive-file-actor");

FileSendBox fileSendBox = new();

fileSendBox.GetFilesInBox()
    .Select(filePath => Path.GetFileName(filePath))
    .ForEach(fileName =>
    {
        sendFileActor.Tell(new SendFileMessage(fileName));
        Console.WriteLine(fileName + " has been Sent!");
    });

Console.Read();

system.Terminate().Wait();