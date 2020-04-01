using NClap.Metadata;

namespace TSD.Akka.Commands
{
    enum CommandList
    {
        [Command(typeof(PopulationCommand), LongName = "population_create", Description = "Creates people")]
        CreatePopulation,

        [Command(typeof(InfectCommand), LongName = "infect_somebody", Description = "Infects a random person")]
        InfectSomebody,

        [Command(typeof(VaccinationCommand), LongName = "vaccinate_somebody", Description = "Vaccinate a random person")]
        VaccinateSomebody,

        [Command(typeof(NextDayCommand), LongName = "next_day", Description = "Start a next day, people are meeting people")]
        NextDay,

        [Command(typeof(SanepidStatsCommand), LongName = "sanepid_stats", Description = "Show Sanepid's statistics for today")]
        SanepidStats,

        [Command(typeof(ExitCommand), Description = "Exits the application")]
        Exit
    }
}
