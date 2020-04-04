using System;
using Akka.Actor;
using Akka.Event;
using System.Collections.Generic;
using System.Linq;

namespace TSD.Akka.Actors
{
    sealed class CreateDoctorMessage
    {
        public string MessageText { get; }
        public CreateDoctorMessage(string messageText) => MessageText = messageText;
    }
    sealed class RequestTreatmentMessage
    {
        public string MessageText { get; }
        public RequestTreatmentMessage(string messageText) => MessageText = messageText;
    }
    class HospitalActor : ReceiveActor
    {
        private Dictionary<string, IActorRef> doctors = new Dictionary<string, IActorRef>();
        private int numberOfDoctors = 0;
        private Random random = new Random();
        public HospitalActor(int initialNumberOfDoctors)
        {
            for (int i = 0; i < initialNumberOfDoctors; i++)
            {
                createDoctor();
            }
            Receive<CreateDoctorMessage>(OnCreateDoctorMessage);
            Receive<RequestTreatmentMessage>(OnRequestTreatmentMessage);
        }

        private void OnCreateDoctorMessage(CreateDoctorMessage message)
        {
            createDoctor();
        }

        private void OnRequestTreatmentMessage(RequestTreatmentMessage message)
        {
            int index = random.Next(doctors.Count);
            var doctor = doctors.ElementAt(index).Value;
            doctor.Forward(message);
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
