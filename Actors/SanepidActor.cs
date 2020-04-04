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
        public int Recovered { get; }

        public StatsReplyMessage(int infected, int inQuarantine, int recovered)
        {
            Infected = infected;
            InQuarantine = inQuarantine;
            Recovered = recovered;
        }
    }

    public class SanepidActor : ReceiveActor
    {
        public int Infected { get; set; }
        public int InQuarantaie { get; set; }
        public int Recovered { get; set; }

        public SanepidActor()
        {
            Receive<PersonActor.InfectedMessage>(message => Infected++);
            Receive<PersonActor.GoToQuarantineMessage>(message => InQuarantaie++);
            Receive<PersonActor.FinishQuarantineMessage>(message => InQuarantaie--);
            Receive<HealMessage>(message => Recovered++);
            Receive<StatsAskMessage>(message => Sender.Tell(new StatsReplyMessage(Infected, InQuarantaie, Recovered), Self));
        }
    }
}
