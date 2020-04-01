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

        public NextDayCommand(ActorSystem system) => System = system;

        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            var people = System.ActorSelection($"user/{ActorNames.People}/*");
            people.Tell(new PersonActor.StartDayMessage("Brand new day"));
            var doctors = System.ActorSelection($"user/{ActorNames.Doctor}/*");
            doctors.Tell(new PersonActor.StartDayMessage("Brand new day"));

            return Task.FromResult(CommandResult.Success);
        }
    }
}
