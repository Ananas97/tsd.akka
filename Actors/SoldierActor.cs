using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using Akka.Actor;

namespace TSD.Akka.Actors
{
    class SoldierActor : ReceiveActor
    {
        public class CheckBodyTemperatureMessage
        {
            public string MessageText { get; }
            public CheckBodyTemperatureMessage(string messageText) => MessageText = messageText;
        }

        private Random random = new Random();
        private int numberOfTested = 40;

        public SoldierActor()
        {
            Receive<PersonActor.StartDayMessage>(OnStartDayMessage);
        }

        private void OnStartDayMessage(PersonActor.StartDayMessage message)
        {
            for (int i = 0; i < numberOfTested; i++)
            {
                CheckBodyTemperature();
            }
        }

        private void CheckBodyTemperature()
        {
            var randomPerson = Context.ActorSelection($"/user/{ActorNames.People}");
            randomPerson.Tell(new CheckBodyTemperatureMessage("You are examined."));
            
        }
    }
}
