using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    class DoctorActor : ReceiveActor
    {
        public class HealMessage
        {
            public string MessageText { get; }
            public HealMessage(string messageText) => MessageText = messageText;
        }

        private readonly ILoggingAdapter log = Context.GetLogger();

        private PersonActor.PersonState state = PersonActor.PersonState.Uninfected;
        private Random random = new Random();

        public DoctorActor()
        {
            // log.Info($"Created actor {Context.Self.Path}");
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
            Receive<PersonActor.InfectedMessage>(OnInfectedMessage);
        }


        private void OnStartDayMessage(PersonActor.StartDayMessage message)
        {
            for (int i = 0; i < 30; i++)
            {
                Treatment();
            }
        }

        private void Treatment()
        {
            var randomPerson = Context.ActorSelection($"/user/{ActorNames.People}");
            bool personCanBeTreated = random.Next(100) < 20;

            if (state == PersonActor.PersonState.Uninfected)
            {
                if (personCanBeTreated)
                {
                    randomPerson.Tell(new HealMessage("Lucky you! You received treatment."));
                }
            }
            else if (state == PersonActor.PersonState.Infected)
            {
                randomPerson.Tell(new PersonActor.InfectedMessage("Hello, my friend! I'm infected, and I'll infect you too!"));
            }
        }

        private void OnInfectedMessage(PersonActor.InfectedMessage message)
        {
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new PersonActor.InfectedMessage("I'm informing that I'm infected"));

            Become(Infected);
        }

        private void Infected()
        {
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
            Receive<PersonActor.ChatMessage>(message => Sender.Tell(new PersonActor.InfectedMessage("I'm resending you an infection!"), Context.Self));
        }
    }
}
