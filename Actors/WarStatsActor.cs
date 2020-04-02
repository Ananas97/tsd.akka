using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    public class WarStatsAskMessage {
        public bool PaperSupplied;
        public WarStatsAskMessage(bool paperSupplied)
        {
            PaperSupplied = paperSupplied;
        }
    }

    public class WarStatsAskReplyMessage
    {
        public int FallenPaperKnights { get; }
        public int VictoriousPaperKnights { get; }
        public int DefeatedPaperKnights { get; }
        public WarStatsAskReplyMessage(int fallenPaperKnights, int victoriousPaperKnights, int defeatedPaperKnights)
        {
            FallenPaperKnights = fallenPaperKnights;
            VictoriousPaperKnights = victoriousPaperKnights;
            DefeatedPaperKnights = defeatedPaperKnights;
        }
    }
    public class WarOutcomeMessage
    {
        public WarOutcome MessageText { get; }
        public WarOutcomeMessage(WarOutcome messageText) => MessageText = messageText;
        public enum WarOutcome
        {
            Fallen,
            Victory,
            Defeat
        }
    }

    public class WarStatsActor : ReceiveActor
    {
        public int FallenPaperKnights { get; set; }
        public int VictoriousPaperKnights { get; set; }
        public int DefeatedPaperKnights { get; set; }
        public bool PaperSupplied { get; set; }


        public WarStatsActor()
        {
            Receive<WarOutcomeMessage>(message =>
            {
                switch (message.MessageText)
                {
                    case WarOutcomeMessage.WarOutcome.Fallen:
                        FallenPaperKnights++;
                        break;
                    case WarOutcomeMessage.WarOutcome.Defeat:
                        DefeatedPaperKnights++;
                        break;
                    case WarOutcomeMessage.WarOutcome.Victory:
                        VictoriousPaperKnights++;
                        break;
                }
            });
            Receive<WarStatsAskMessage>(message =>
            {
                if (!PaperSupplied)
                    Sender.Tell(null);
                else
                    Sender.Tell(new WarStatsAskReplyMessage(FallenPaperKnights, VictoriousPaperKnights, DefeatedPaperKnights), Self);
                PaperSupplied = message.PaperSupplied;
                //new day, reset stats
                FallenPaperKnights = 0;
                VictoriousPaperKnights = 0;
                DefeatedPaperKnights = 0;
            });
        }

    }
}
