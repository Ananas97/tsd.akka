using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class PopulationCommand : Command
    {
        [PositionalArgument(ArgumentFlags.Optional, DefaultValue = 1_000, Description = "Number of people to create")]
        public int PeopleCount { get; set; }

        public ActorSystem System { get; }

        public PopulationCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            System.ActorOf(Props.Create<PersonActor>().WithRouter(new RandomPool(PeopleCount)), ActorNames.People);

            Console.WriteLine($"{PeopleCount} people were created");

            System.ActorOf(Props.Create<DoctorActor>().WithRouter(new RandomPool(1)), ActorNames.Doctor);

            Console.WriteLine($"One doctor was created");

            System.ActorOf(Props.Create<SoldierActor>().WithRouter(new RandomPool(10)), ActorNames.Soldier);

            Console.WriteLine($"10 soldiers were created");

            return Task.FromResult(CommandResult.Success);
        }
    }
}
