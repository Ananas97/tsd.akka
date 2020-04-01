using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class DoctorCommand : Command
    {
        [PositionalArgument(ArgumentFlags.Optional, DefaultValue = 1, Description = "Number of doctors to hire")]
        public int DoctorCount { get; set; }

        public ActorSystem System { get; }

        public DoctorCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            System.ActorOf(Props.Create<DoctorActor>().WithRouter(new RandomPool(DoctorCount)), ActorNames.Doctor);

            Console.WriteLine($"Doctor was created");

            return Task.FromResult(CommandResult.Success);
        }
    }
}
