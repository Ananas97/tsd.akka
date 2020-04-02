using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class SoldierCommand : Command
    {
        [PositionalArgument(ArgumentFlags.Optional, DefaultValue = 1, Description = "Number of seconded soldiers")]
        public int NumberOfSoldiers { get; set; }

        public ActorSystem System { get; }

        public SoldierCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            System.ActorOf(Props.Create<SoldierActor>().WithRouter(new RandomPool(NumberOfSoldiers)), ActorNames.Soldier);

            Console.WriteLine($"Soldier was created");

            return Task.FromResult(CommandResult.Success);
        }
    }
}
