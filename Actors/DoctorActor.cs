using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    sealed class HealMessage
    {
        public string MessageText { get; }
        public HealMessage(string messageText) => MessageText = messageText;
    }

    class DoctorActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();
        private PersonActor.PersonState state = PersonActor.PersonState.Uninfected;
        private int numberOfPatientsToday = 0;
        private Random random = new Random();

        public DoctorActor()
        {
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
            Receive<RequestTreatmentMessage>(OnTreatmentRequest);
        }


        private void OnStartDayMessage(PersonActor.StartDayMessage message)
        {
            numberOfPatientsToday = 0;
        }

        private void OnTreatmentRequest(RequestTreatmentMessage message)
        {
            numberOfPatientsToday++;
            if (state == PersonActor.PersonState.Uninfected && numberOfPatientsToday <= 5)
            {
                if (random.NextDouble() <= 0.01)
                {
                    System.Console.WriteLine("Doctor is infected");
                    Become(Infected);
                }

                bool personWillBeTreated = random.NextDouble() <= 0.8;
                if (personWillBeTreated)
                {
                    Sender.Tell(new HealMessage("Lucky you! You received treatment."));

                    var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
                    sanepid.Tell(new HealMessage("A person has been healed."));
                }
            }
        }

        private void Infected()
        {
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
        }
    }
}
