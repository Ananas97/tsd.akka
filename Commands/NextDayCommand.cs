using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class NextDayCommand : Command
    {
        public ActorSystem System { get; }
        private static readonly Random Random = new Random();

        public NextDayCommand(ActorSystem system) => System = system;

        public override async Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            //50% chance for delivery of paper in Biedronka everyday
            var paperSupplied = Random.Next(2) == 0;
            var warStatsActor = System.ActorSelection($"user/{ActorNames.WarStats}");
            var warStats = await warStatsActor.Ask<WarStatsAskReplyMessage>(new WarStatsAskMessage(paperSupplied), TimeSpan.FromSeconds(5));
            if (warStats != null)
                Console.WriteLine($"War stats from previous day: \npeople fallen in battle: {warStats.FallenPaperKnights}, " +
                                  $"people who gained paper: {warStats.VictoriousPaperKnights}, " +
                                  $"defeated people who didn't get paper: {warStats.DefeatedPaperKnights}");
            
            var people = System.ActorSelection($"user/{ActorNames.People}/*");
            people.Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));
            var doctors = System.ActorSelection($"user/{ActorNames.Doctor}/*");
            doctors.Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));
            var soldiers = System.ActorSelection($"user/{ActorNames.Soldier}/*");
            soldiers.Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));
            if (paperSupplied) Console.WriteLine("Paper supplies arrived! Unleashed sheetstorm!");

            return CommandResult.Success;
        }
    }
}
