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
        
        public int Foreigners { get; }
        
        public int InfectedForeigners { get; }
        
        public int QuarantinedForeigners { get; }

        public StatsReplyMessage(int infected, int inQuarantaie, int foreigners, int infectedForeigners, int inQuarantineForeigners)
        {
            Infected = infected;
            InQuarantine = inQuarantaie;
            InfectedForeigners = infectedForeigners;
            Foreigners = foreigners;
            QuarantinedForeigners = inQuarantineForeigners;
            QuarantinedForeigners = inQuarantineForeigners;
        }
    }

    public class SanepidActor : ReceiveActor
    {
        public int Infected { get; set; }
        public int InQuarantaie { get; set; }
        
        public int Foreigners { get; set; }
        
        public int InfectedForeigners { get; set; }

        public int ForeignerInQuarantine { get; set; }

        public SanepidActor()
        {
            Receive<PersonActor.InfectedMessage>(message => Infected++);
            Receive<PersonActor.GoToQuarantineMessage>(message => InQuarantaie++);
            Receive<PersonActor.FinishQuarantineMessage>(message => InQuarantaie--);
            
            Receive<ForeignerGoToQuarantineMessage>(message => ForeignerInQuarantine++);
            Receive<FinishForeignerQuarantineMessage>(message => ForeignerInQuarantine--);
            
            Receive<HealthyForeignerMessage>(message => Foreigners++);
            Receive<InfectedForeignerMessage>(message =>
            {
                Foreigners++;
                InfectedForeigners++;
            });
            
            Receive<StatsAskMessage>(message => Sender.Tell(new StatsReplyMessage(Infected, InQuarantaie, Foreigners, InfectedForeigners, ForeignerInQuarantine), Self));
        }
    }
}
