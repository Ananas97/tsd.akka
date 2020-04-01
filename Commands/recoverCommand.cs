using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    public class recoverCommand : Command
    {
        public ActorSystem System { get; }

        public recoverCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            var people = System.ActorSelection($"user/{ActorNames.People}/*");
            people.Tell(new PersonActor.RecoveryMessage("Great news! there is a change to be recovered"));

            return Task.FromResult(CommandResult.Success);
        }
    }
}
