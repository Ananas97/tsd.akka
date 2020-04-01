using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    public class InfectCommand : Command
    {
        public ActorSystem System { get; }

        public InfectCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            var randomPerson = System.ActorSelection($"user/{ActorNames.People}");
            randomPerson.Tell(new PersonActor.InfectedMessage("Initial infection."));

            return Task.FromResult(CommandResult.Success);
        }
    }
}
