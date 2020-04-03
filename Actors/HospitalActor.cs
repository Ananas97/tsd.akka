using System;
using Akka.Actor;
using Akka.Event;
using System.Collections.Generic;

namespace TSD.Akka.Actors
{
    sealed class CreateDoctorMessage
    {
        public string MessageText { get; }
        public CreateDoctorMessage(string messageText) => MessageText = messageText;
    }
    class HospitalActor : ReceiveActor
    {
        private Dictionary<string, IActorRef> doctors = new Dictionary<string, IActorRef>();
        private int numberOfDoctors = 0;
        public HospitalActor(int initialNumberOfDoctors)
        {
            for (int i = 0; i < initialNumberOfDoctors; i++)
            {
                createDoctor();
            }
            Receive<CreateDoctorMessage>(OnCreateDoctorMessage);
        }

        private void OnCreateDoctorMessage(CreateDoctorMessage message)
        {
            createDoctor();
        }

        private void createDoctor()
        {
            string name = $"doctor-{numberOfDoctors}";
            var doctorRef = Context.ActorOf<DoctorActor>(name);
            doctors.Add(name, doctorRef);
            numberOfDoctors++;
        }
    }
}
