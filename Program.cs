using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NClap;
using NClap.Repl;
using TSD.Akka.Actors;
using TSD.Akka.Commands;

namespace TSD.Akka
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            RunInteractiveShell();
        }

        private static void RunInteractiveShell()
        {
            using var system = ActorSystem.Create("virus-simulation");
            RegisterGlobalActors(system);

            var parserOptions = new CommandLineParserOptions().With()
                .ConfigureServices(s => s.AddSingleton(system));

            var loopOptions = new LoopOptions { ParserOptions = parserOptions };

            var loop = new Loop(typeof(CommandList), options: loopOptions);

            Console.WriteLine("Virus simulation");
            loop.Execute();
            Console.WriteLine("Exiting the application...");
        }

        private static void RegisterGlobalActors(ActorSystem system)
        {
            system.ActorOf<SanepidActor>(ActorNames.Sanepid);
            system.ActorOf(Props.Create(() => new HospitalActor(1)), ActorNames.Hospital);
            system.ActorOf<WarStatsActor>(ActorNames.WarStats);
        }
    }
}
