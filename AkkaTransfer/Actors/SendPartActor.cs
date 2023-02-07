using Akka.Actor;
using AkkaTransfer.Messages;

namespace AkkaTransfer.Actors
{
    public class SendPartActor : ReceiveActor
    {
        public SendPartActor()
        {
            Receive<FilePartMessage>(message =>
            {
                Console.WriteLine($"Send part {message.Position} of {message.Count}");

                var receiveActor = Context.ActorSelection($"akka.tcp://file-transfer-system@{HoconLoader.ReadSendIpAndPort("hocon.send")}/user/receive-file-actor");

                receiveActor.Tell(message);
            });
        }
    }
}
