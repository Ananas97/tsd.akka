using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class VaccinationCommand : Command
    {
            public ActorSystem System { get; }

            public VaccinationCommand(ActorSystem system) => System = system;

            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                var randomPerson = System.ActorSelection($"user/{ActorNames.People}");
                randomPerson.Tell(new PersonActor.VaccinationMessage("Calm down and take this vaccine!"));

                return Task.FromResult(CommandResult.Success);
            }
        }
}
