using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    public class StatsAskMessage { }

    public class StatsReplyMessage
    {
        public int Infected { get; }
        public StatsReplyMessage(int infected) => Infected = infected;
    }

    public class SanepidActor : ReceiveActor
    {
        public int Infected { get; set; }

        public SanepidActor()
        {
            Receive<PersonActor.InfectedMessage>(message => Infected++);
            Receive<StatsAskMessage>(message => Sender.Tell(new StatsReplyMessage(Infected), Self));
        }
    }
}
