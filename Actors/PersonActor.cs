﻿using System;
using Akka.Actor;
using Akka.Event;

namespace TSD.Akka.Actors
{
    class PersonActor : ReceiveActor
    {
        public const int TransmissionProbability = 50;
        public const int QuarantinePeriod = 7; //length of quarantine in days

        static Random random = new Random();
        public int SocialContacts {get; private set;}
        public class GoToQuarantineMessage { }
        public class FinishQuarantineMessage { }


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
            Vaccinated,
            Dead,
            Carrier
        }

        private readonly ILoggingAdapter log = Context.GetLogger();

        private PersonState state = PersonState.Uninfected;
        private bool _isInQuarantine; //true when person is in quarantine
        private bool _disobeyedQuarantine; //true when person is in quarantine but disobeyed it on current day
        private int _daysSpentInQuarantine; //how many days person has spent in quarantine

        public PersonActor()
        {
            // log.Info($"Created person {Context.Self.Path}");

            SocialContacts = random.Next(2,15);
            Receive<StartDayMessage>(OnStartDayMessage);
            Receive<InfectedMessage>(OnInfectedMessage);
            Receive<VaccinationMessage>(OnVaccinationMessage);
            Receive<GoToQuarantineMessage>(OnGoToQuarantineMessage);
        }

        //Soldier actor from issue #11 will be responsible for that
        //quarantine begins next day
        private void OnGoToQuarantineMessage(GoToQuarantineMessage message)
        {
            _isInQuarantine = true;
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new GoToQuarantineMessage());
            Become(InQuarantine);
        }

        private void OnStartDayInQuarantineMessage(StartDayMessage obj)
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
        }

        private void NotInQuarantine()
        {
            Receive<StartDayMessage>(OnStartDayMessage);
        }

        private void InQuarantine()
        {
            Receive<StartDayMessage>(OnStartDayInQuarantineMessage);
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
                var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
                sanepid.Tell(new InfectedMessage("I'm informing that I'm infected"));

                state = PersonState.Infected;
                Become(Infected);
            }
            else if(random.NextDouble()<0.05) Become(Carrier);
        }

        private void OnHealMessage(DoctorActor.HealMessage message)
        {
            state = PersonState.Uninfected;
            if(_isInQuarantine) FinishQuarantine();
            Become(Uninfected);
        }

        private void Uninfected()
        {
//            Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
            Receive<InfectedMessage>(OnInfectedMessage);
        }

        private void Infected()
        {
//            Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm resending you an infection!"), Context.Self));
            Receive<DoctorActor.HealMessage>(OnHealMessage);
        }

        private void Vaccinated()
        {
//            Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
        }
        
        private void Dead(){}

        private void Carrier()
        {
//            Receive<StartDayMessage>(OnStartDayMessage); //it is already set in constructor, becoming uninfected doesn't affect this message handling
            Receive<ChatMessage>(message => Sender.Tell(new InfectedMessage("I'm sending you an infection!"), Context.Self));
        }
    }
}
