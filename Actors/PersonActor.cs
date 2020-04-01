using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    class PersonActor : ReceiveActor
    {

        public const int TransmissionProbability = 50;

        static Random random = new Random();
        public int SocialContacts {get; private set;}
        public class StartDayMessage
        {
            public string MessageText { get; }
            public StartDayMessage(string messageText) => MessageText = messageText;
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
            Vaccinated
        }

        private readonly ILoggingAdapter log = Context.GetLogger();

        private PersonState state = PersonState.Uninfected;

        public PersonActor()
        {
            // log.Info($"Created person {Context.Self.Path}");

            SocialContacts = random.Next(2,15);
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<InfectedMessage>(OnInfectedMessage);
            Receive<VaccinationMessage>(OnVaccinationMessage);
        }


        private void OnStartDayMessage(StartDayMessage message)
        {
            int contacts = random.Next(0, SocialContacts);
            for (int i = 0; i < contacts; i++)
            {
                Chat();
            }
        }

        private void Chat()
        {
            var randomPerson = Context.Parent;

            if (state == PersonState.Uninfected || state == PersonState.Vaccinated )
            {
                randomPerson.Tell(new ChatMessage("Hello, my friend!"));
            }
            else if (state == PersonState.Infected)
            {
                randomPerson.Tell(new InfectedMessage("Hello, my friend! I'm infected, and I'll infect you too!"));
            }
        }


        private void OnVaccinationMessage(VaccinationMessage message)
        {
                Become(Vaccinated);
        }

        private void OnInfectedMessage(InfectedMessage message)
        {

            if (message.MessageText == "Initial infection." || random.Next() % 100 < TransmissionProbability)
            {
                var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
                sanepid.Tell(new InfectedMessage("I'm informing that I'm infected"));

                Become(Infected);
            }

        }

        private void Infected()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm resending you an infection!"), Context.Self));
        }

        private void Vaccinated()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
        }
    }
}
