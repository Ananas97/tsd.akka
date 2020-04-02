using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class IntroduceForeignerCommand : Command
    {
        public ActorSystem System { get; }

        public IntroduceForeignerCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            var foreignerName = ActorNames.Foreigner + "_" + Guid.NewGuid();

            System.ActorOf(Props.Create<ForeignerActor>().WithRouter(new RandomPool(1)), foreignerName);

            if (new Random().Next() % 100 > 70)// 70% chance of being infected
            {
                System.ActorSelection(foreignerName).Tell(new HealthyForeignerMessage("Healthy foreigner"));
                System.ActorSelection($"/user/{ActorNames.Sanepid}").Tell(new HealthyForeignerMessage("Healthy foreigner"));
            }
            else
            {
                System.ActorSelection(foreignerName).Tell(new InfectedForeignerMessage("Infected foreigner"));
                System.ActorSelection($"/user/{ActorNames.Sanepid}").Tell(new InfectedForeignerMessage("Infected foreigner"));
            }
            
            return Task.FromResult(CommandResult.Success);
        }
    }
}