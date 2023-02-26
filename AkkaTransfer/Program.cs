using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
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

IReceiveFileHeaderRepository repo = new ReceiveFileHeaderRepository(dbContext);

// Setup Actors.

FileBox fileSendBox = new("SendBox");
FileBox fileReceiveBox = new("ReceiveBox");

Config hocon = HoconLoader.FromFile("akka.net.hocon");
ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

Props sendProps = Props.Create(typeof(SendFileActor), fileSendBox);
IActorRef sendFileActor = system.ActorOf(sendProps, "send-file-actor");

Props rebuilderProps = Props.Create(typeof(FileRebuilderActor), fileSendBox, repo);
_ = system.ActorOf(rebuilderProps, "file-rebuilder-actor");

Props receiveGatewayProps = Props.Create(typeof(ReceiveFileActor), fileReceiveBox, repo)
    .WithRouter(new SmallestMailboxPool(5, new DefaultResizer(5, 1000), SupervisorStrategy.DefaultStrategy, "default-dispatcher"));
IActorRef receiveGateway = system.ActorOf(receiveGatewayProps, "receive-file-actor");

// Send files.

fileSendBox.GetFilesInBox()
    .Select(filePath => Path.GetFileName(filePath))
    .ForEach(sendFileActor.Tell);

while (true)
{
    Thread.Sleep(1000);
}