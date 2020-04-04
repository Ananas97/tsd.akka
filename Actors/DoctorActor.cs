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
            // log.Info($"Created actor {Context.Self.Path}");
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
            Receive<RequestTreatmentMessage>(OnTreatmentRequest);
        }


        private void OnStartDayMessage(PersonActor.StartDayMessage message)
        {
            numberOfPatientsToday = 0;
        }

        private void OnInfectedMessage(PersonActor.InfectedMessage message)
        {
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new PersonActor.InfectedMessage("I'm informing that I'm infected"));

            Become(Infected);
        }

        private void OnTreatmentRequest(RequestTreatmentMessage message)
        {
            System.Console.WriteLine(message.MessageText + " " + Sender.Path);

            numberOfPatientsToday++;

            bool personCanBeTreated = random.NextDouble() <= 1 && numberOfPatientsToday <= 5;

            if (state == PersonActor.PersonState.Uninfected && personCanBeTreated)
            {
                if (random.NextDouble() <= 0.01) 
                {
                    Become(Infected);
                    System.Console.WriteLine("Doctor is infected");
                }

                Sender.Tell(new HealMessage("Lucky you! You received treatment."));
            }



        }

        private void Infected()
        {
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
        }
    }
}
