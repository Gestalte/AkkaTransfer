using Akka.Actor;
using Akka.Util.Internal;
using AkkaTransfer;
using AkkaTransfer.Actors;
using AkkaTransfer.Data;
using Microsoft.EntityFrameworkCore;

// Setup ReceiveDb.

var dbContext = new ReceiveDbContext();

if (dbContext.Database.GetPendingMigrations().ToList().Count != 0)
{
    dbContext.Database.Migrate();
}

IFileHeaderRepository repo = new  FileHeaderRepository(dbContext);

// Setup Actors.

var hocon = HoconLoader.FromFile("akka.net.hocon");
ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

FileBox fileSendBox = new("SendBox");
FileBox fileReceiveBox = new("ReceiveBox");

Props sendProps = Props.Create(typeof(SendFileActor), fileSendBox);
Props receiveProps = Props.Create(typeof(ReceiveFileActor), fileReceiveBox);
Props rebuilderProps =Props.Create(typeof(FileRebuilderActor), fileSendBox, repo);

IActorRef sendFileActor = system.ActorOf(sendProps, "send-file-actor");

_ = system.ActorOf(receiveProps, "receive-file-actor");
_ = system.ActorOf(rebuilderProps, "file-rebuilder-actor");

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