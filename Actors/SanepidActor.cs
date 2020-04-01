using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    public class StatsAskMessage { }

    public class StatsReplyMessage
    {
        public int Infected { get; }
        public int InQuarantine { get; }
        public StatsReplyMessage(int infected, int inQuarantine)
        {
            Infected = infected;
            InQuarantine = inQuarantine;
        }
    }

    public class SanepidActor : ReceiveActor
    {
        public int Infected { get; set; }
        public int InQuarantaie { get; set; }

        public SanepidActor()
        {
            Receive<PersonActor.InfectedMessage>(message => Infected++);
            Receive<PersonActor.GoToQuarantineMessage>(message => InQuarantaie++);
            Receive<PersonActor.FinishQuarantineMessage>(message => InQuarantaie--);
            Receive<StatsAskMessage>(message => Sender.Tell(new StatsReplyMessage(Infected, InQuarantaie), Self));
        }
    }
}
