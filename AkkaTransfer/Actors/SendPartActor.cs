using Akka.Actor;
using AkkaTransfer.Common;

namespace AkkaTransfer.Actors
{
    public class SendPartActor : ReceiveActor
    {
        public SendPartActor()
        {
            Receive<FilePartMessage>(message =>
            {
                System.Diagnostics.Debug.WriteLine($"Received FilePartMessage: {message}", nameof(SendPartActor));

                var address = $"akka.tcp://file-transfer-system@{HoconLoader.ReadSendIpAndPort("hocon.send")}/user/receive-file-coordinator-actor";

                var receiveActor = Context.ActorSelection(address);

                receiveActor.Tell(message);

                System.Diagnostics.Debug.WriteLine($"Send part {message.Position} of {message.Count}");
            });
        }
    }
}
