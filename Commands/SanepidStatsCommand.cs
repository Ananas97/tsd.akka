using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NClap.Metadata;
using TSD.Akka.Actors;

namespace TSD.Akka.Commands
{
    class SanepidStatsCommand : Command
    {
        public ActorSystem System { get; }

        public SanepidStatsCommand(ActorSystem system) => System = system;

        public override async Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            var sanepid = System.ActorSelection($"user/{ActorNames.Sanepid}");
            var stats = await sanepid.Ask<StatsReplyMessage>(new StatsAskMessage(), TimeSpan.FromSeconds(5));
            Console.WriteLine($"Infected people: {stats.Infected}, people in quarantine: {stats.InQuarantine}, people recovered: {stats.Recovered}");

            return CommandResult.Success;
        }
    }
}
