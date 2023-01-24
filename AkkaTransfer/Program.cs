// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using AkkaTransfer;

ActorSystem system = ActorSystem.Create("file-transfer");

IActorRef listReceivedFilesActor = system.ActorOf<ListReceivedFilesActor>("list-received-files");

var result = await listReceivedFilesActor.Ask(new RequestReceivedFilesMessage()) as ReceiveManifest;

if (result?.FileNames.Count > 0)
{
    result.FileNames.ForEach(f => Console.WriteLine(f));
}
else
{
    Console.WriteLine("No files in ReceiveBox");
}

Console.Read();

system.Terminate();