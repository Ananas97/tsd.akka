using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    class PersonActor : ReceiveActor
    {
        public const int TransmissionProbability = 50;
        public const int QuarantinePeriod = 7; //length of quarantine in days

        static Random random = new Random();
        public int SocialContacts { get; private set; }
        public class GoToQuarantineMessage { }
        public class FinishQuarantineMessage { }


        public class StartDayMessage
        {
            public string MessageText { get; }
            public bool PaperSupplied { get; }
            public StartDayMessage(string messageText, bool paperSupplied)
            {
                MessageText = messageText;
                PaperSupplied = paperSupplied;
            }
        }

        public class ChatMessage
        {
            public string MessageText { get; }
            public ChatMessage(string messageText) => MessageText = messageText;
        }

        public class InfectedMessage
        {
            public string MessageText { get; }
            public InfectedMessage(string messageText) => MessageText = messageText;
        }

        public class VaccinationMessage
        {
            public string MessageText { get; }
            public VaccinationMessage(string messageText) => MessageText = messageText;
        }

        public enum PersonState
        {
            Uninfected,
            Infected,
            Vaccinated,
            Dead,
            Carrier
        }

        private readonly ILoggingAdapter log = Context.GetLogger();

        private PersonState state = PersonState.Uninfected;
        private bool _isInQuarantine; //true when person is in quarantine
        private bool _disobeyedQuarantine; //true when person is in quarantine but disobeyed it on current day
        private int _daysSpentInQuarantine; //how many days person has spent in quarantine
        private int _paperRolls;
        private bool _isWanted; //true when person is wanted by the police and the army

        public PersonActor()
        {
            // log.Info($"Created person {Context.Self.Path}");

            SocialContacts = random.Next(2, 15);
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<InfectedMessage>(OnInfectedMessage);
            Receive<VaccinationMessage>(OnVaccinationMessage);
        }

        private void OnCheckBodyTemperatureMessage(SoldierActor.CheckBodyTemperatureMessage message)
        {
            if (random.Next(0, 50) != 1) //little risk that he/she will run away
            {
                _isWanted = false;
                GoToQuarantine();
            }
            else
            {
                _isWanted = true;
            }
        }

        //Soldier actor from issue #11 will be responsible for that
        //quarantine begins next day
        private void GoToQuarantine()
        {
            _isInQuarantine = true;
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new GoToQuarantineMessage());
            Become(InQuarantine);
        }

        private void OnStartDayInQuarantineMessage(StartDayMessage message)
        {
            _daysSpentInQuarantine++;
            if (_daysSpentInQuarantine > QuarantinePeriod)
            {
                FinishQuarantine();
                return;
            }
            // 5% chance that he will not obey quarantine
            if (random.Next(0, 20) == 0)
            {
                _disobeyedQuarantine = true;
                Chat();
            }
            else
            {
                _disobeyedQuarantine = false;
            }
            // opportunity of gaining paper is too lucrative not to consider going to war for it, even when in quarantine
            if (message.PaperSupplied) PaperWarCouncil();
            if (state == PersonState.Infected && random.NextDouble() < 0.2) RequestTreatment();
        }

        private void NotInQuarantine()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<HealMessage>(OnHealMessage);
        }

        private void InQuarantine()
        {
            Receive<StartDayMessage>(OnStartDayInQuarantineMessage);
            Receive<HealMessage>(OnHealMessage);
        }

        private void FinishQuarantine()
        {
            _daysSpentInQuarantine = 0;
            _isInQuarantine = false;
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new FinishQuarantineMessage());
            Become(NotInQuarantine);
        }

        private void OnStartDayMessage(StartDayMessage message)
        {
            if (state == PersonState.Infected && new Random().NextDouble() < 0.05) Become(Dead);
            int contacts = random.Next(0, SocialContacts);
            for (int i = 0; i < contacts; i++)
            {
                Chat();
            }

            if (state == PersonState.Infected && random.NextDouble() < 0.05) Become(Dead);
            if (message.PaperSupplied) PaperWarCouncil();
            if (state == PersonState.Infected && random.NextDouble() < 0.2) RequestTreatment();
        }

        //paper war council decides whether or not go to paper war
        private void PaperWarCouncil()
        {
            //thoroughly thought out random decision
            if (random.Next(2) == 0) return;
            //go to war
            var warOutcome = random.Next(10);
            var warStatsActor = Context.ActorSelection($"/user/{ActorNames.WarStats}");
            if (warOutcome == 0)
            {
                Become(Dead);   //fallen in glorious battle
                warStatsActor.Tell(new WarOutcomeMessage(WarOutcomeMessage.WarOutcome.Fallen));
            }
            else if (warOutcome > 5)
            {
                _paperRolls += 8; //victory, got paper - 8 rolls pack
                warStatsActor.Tell(new WarOutcomeMessage(WarOutcomeMessage.WarOutcome.Victory));
            }
            else
            {
                //had to retreat, no paper gained
                warStatsActor.Tell(new WarOutcomeMessage(WarOutcomeMessage.WarOutcome.Defeat));
            }
        }

        private void RequestTreatment()
        {
            var hospitalActor = Context.ActorSelection($"/user/{ActorNames.Hospital}");
            hospitalActor.Tell(new RequestTreatmentMessage("I request to be treated"));
        }

        private void Chat()
        {
            var randomPerson = Context.Parent;

            if (state == PersonState.Uninfected || state == PersonState.Vaccinated)
            {
                randomPerson.Tell(new ChatMessage("Hello, my friend!"));
            }
            else if (state == PersonState.Infected)
            {
                randomPerson.Tell(new InfectedMessage("Hello, my friend! I'm infected, and I'll infect you too!"));
            }
            else if (state == PersonState.Carrier)
            {
                randomPerson.Tell(new InfectedMessage("Hello, my friend! I wont die but I'll infect you!"));
            }
        }


        private void OnVaccinationMessage(VaccinationMessage message)
        {
            Become(Vaccinated);
        }

        private void OnInfectedMessage(InfectedMessage message)
        {
            if (_isInQuarantine && !_disobeyedQuarantine) return;    //when at home in quarantine no conversations will occur
            if (message.MessageText == "Initial infection." || random.Next() % 100 < TransmissionProbability)
            {
                var paperShield = false;
                if (_paperRolls > 0)
                {
                    _paperRolls--; //used paper roll (gained in war in biedronka)
                    paperShield = random.Next(2) == 0; //50% chance that paper will prevent from being infected
                }
                if (!paperShield)
                {
                    var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
                    sanepid.Tell(new InfectedMessage("I'm informing that I'm infected"));

                    state = PersonState.Infected;
                    Become(Infected);
                }
            }
            else if (random.NextDouble() < 0.05) Become(Carrier);
        }

        private void OnHealMessage(HealMessage message)
        {
            state = PersonState.Uninfected;
            if (_isInQuarantine) FinishQuarantine();
            Become(Uninfected);
        }

        private void Uninfected()
        {
            // Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
            Receive<InfectedMessage>(OnInfectedMessage);
        }

        private void Infected()
        {
            // Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm resending you an infection!"), Context.Self));
            Receive<HealMessage>(OnHealMessage);
            Receive<SoldierActor.CheckBodyTemperatureMessage>(OnCheckBodyTemperatureMessage);
        }

        private void Vaccinated()
        {
            // Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
        }

        private void Dead() { }

        private void Carrier()
        {
            // Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm sending you an infection!"), Context.Self));
        }

    }
}
