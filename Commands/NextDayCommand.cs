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

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            //20% chance for delivery of paper in Biedronka everyday
            bool paperSupplied = Random.Next(0, 5) == 0;
            var people = System.ActorSelection($"user/{ActorNames.People}/*");
            people.Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));
            var doctors = System.ActorSelection($"user/{ActorNames.Doctor}/*");
            doctors.Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));
            var soldiers = System.ActorSelection($"user/{ActorNames.Soldier}/*");
            soldiers.Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));
            
            System.ActorSelection($"*{ActorNames.Foreigner}*")
                .Tell(new PersonActor.StartDayMessage("Brand new day", paperSupplied));

            return Task.FromResult(CommandResult.Success);
        }
    }
}
