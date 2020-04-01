using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    class PersonActor : ReceiveActor
    {

        public const int TransmissionProbability = 50;
        public const int IncubationPeriod = 5; //days after infection when patient becomes symptomatic
        public const int TimeToBecomeContagious = 3; // days after infection when patient can infect others, should be lower than IncubationPeriod

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

        public enum PersonState
        {
            Uninfected,
            Incubating,
            Contagious,
            Symptomatic
        }

        private readonly ILoggingAdapter log = Context.GetLogger();

        private PersonState state = PersonState.Uninfected;

        private int daysSinceInfection = -1;

        public PersonActor()
        {
            // log.Info($"Created person {Context.Self.Path}");

            SocialContacts = random.Next(2,15);
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<InfectedMessage>(OnInfectedMessage);
        }


        private void OnStartDayMessage(StartDayMessage message)
        {
            if (daysSinceInfection != -1)
            {
                daysSinceInfection++;
                if (daysSinceInfection == IncubationPeriod && state != PersonState.Symptomatic)
                {
                    var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
                    sanepid.Tell(new InfectedMessage("I'm informing that I'm infected"));

                    state = PersonState.Symptomatic;
                    Become(Symptomatic);
                } else if (daysSinceInfection == TimeToBecomeContagious && state != PersonState.Contagious)
                {
                    state = PersonState.Contagious;
                    Become(Contagious);
                }
            }

            int contacts = random.Next(0, SocialContacts);
            for (int i = 0; i < contacts; i++)
            {
                Chat();
            }
        }

        private void Chat()
        {
            var randomPerson = Context.Parent;

            if (state == PersonState.Uninfected || state == PersonState.Incubating)
            {
                randomPerson.Tell(new ChatMessage("Hello, my friend!"));
            }
            else if (state == PersonState.Symptomatic || state == PersonState.Contagious)
            {
                randomPerson.Tell(new InfectedMessage("Hello, my friend! I'm infected, and I'll infect you too!"));
            }
        }

        private void OnInfectedMessage(InfectedMessage message)
        {

            if ((message.MessageText == "Initial infection." || random.Next() % 100 < TransmissionProbability)
                && state == PersonState.Uninfected)
            {
                daysSinceInfection = 0;
                state = PersonState.Incubating;
                Become(Incubating);
            }
        }

        private void Incubating()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<ChatMessage>(message => Sender.Tell(new ChatMessage("I'm incubating but I can't infect you just yet!"), Context.Self));
        }

        private void Contagious()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm resending you an infection even though I am still asymptomatic."), Context.Self));
        }

        private void Symptomatic()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm resending you an infection!"), Context.Self));
        }
    }
}
