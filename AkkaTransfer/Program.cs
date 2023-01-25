// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using Akka.Util.Internal;
using AkkaTransfer;

ActorSystem system = ActorSystem.Create("file-transfer");

Props props = Props.Create(typeof(SendFileActor), new FileSendBox());

IActorRef sendFileActor = system.ActorOf(props, "send-file-actor");

FileSendBox fileSendBox = new FileSendBox();

fileSendBox.GetFilesInBox()
    .Select(filePath => Path.GetFileName(filePath))
    .ForEach(fileName => 
    {
        sendFileActor.Tell(new SendFileMessage { FileName = fileName });
        Console.WriteLine(fileName + " has been Sent!");
    });

Console.Read();

system.Terminate();