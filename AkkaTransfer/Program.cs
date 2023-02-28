using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using Akka.Util.Internal;
using AkkaTransfer;
using AkkaTransfer.Actors;
using AkkaTransfer.Data;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;
using Microsoft.EntityFrameworkCore;

// Setup ReceiveDb.

var dbContext = new ReceiveDbContext();

if (dbContext.Database.GetPendingMigrations().ToList().Count != 0)
{
    dbContext.Database.Migrate();
}

// Setup ManifestActor

IManifestRepository receiveManifestRepo = new ReceiveManifestRepository(dbContext);
IManifestRepository sendManifestRepo = new SendManifestRepository(dbContext);

FileBox fileSendBox = new("SendBox");
FileBox fileReceiveBox = new("ReceiveBox");

ManifestHelper sendManifestHelper = new ManifestHelper(fileSendBox, sendManifestRepo);
ManifestHelper receiveManifestHelper = new ManifestHelper(fileReceiveBox, receiveManifestRepo);

Config hocon = HoconLoader.FromFile("akka.net.hocon");
ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

Props props = Props.Create<ManifestActor>(() => new ManifestActor(HoconLoader.ReadSendIpAndPort("hocon.send"), sendManifestHelper, receiveManifestHelper));
IActorRef sendFileActor = system.ActorOf(props, "manifest-actor");

// TODO: This doesn't work because async, make a delegate to let you know when receiving files is done.
while (true)
{
    Console.WriteLine();
    Console.WriteLine("Type \"receive\" to request file transfer.");

    var input = Console.ReadLine();

    switch (input)
    {
        case "receive":
            sendFileActor.Tell(new SendManifestRequest());
            break;
    }
}

//////IReceiveFileHeaderRepository repo = new ReceiveFileHeaderRepository(dbContext);

//////// Setup Actors.

//////FileBox fileSendBox = new("SendBox");
//////FileBox fileReceiveBox = new("ReceiveBox");

//////Config hocon = HoconLoader.FromFile("akka.net.hocon");
//////ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

//////Props sendProps = Props.Create(typeof(SendFileActor), fileSendBox);
//////IActorRef sendFileActor = system.ActorOf(sendProps, "send-file-actor");

//////Props rebuilderProps = Props.Create(typeof(FileRebuilderActor), fileSendBox, repo);
//////_ = system.ActorOf(rebuilderProps, "file-rebuilder-actor");

//////Props receiveGatewayProps = Props.Create(typeof(ReceiveFileActor), fileReceiveBox, repo)
//////    .WithRouter(new SmallestMailboxPool(5, new DefaultResizer(5, 1000), SupervisorStrategy.DefaultStrategy, "default-dispatcher"));
//////IActorRef receiveGateway = system.ActorOf(receiveGatewayProps, "receive-file-actor");

//////// Send files.

//////fileSendBox.GetFilesInBox()
//////    .Select(filePath => Path.GetFileName(filePath))
//////    .ForEach(sendFileActor.Tell);
