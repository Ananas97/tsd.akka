namespace TSD.Akka.Actors
{
    class ForeignerActor : PersonActor
    {
        public ForeignerActor()
        {
            aForeigner = true;
            Receive<InfectedForeignerMessage>(onInfectedForeignerMessage);
            Receive<HealthyForeignerMessage>(onHealthyForeignerMessage);
        }
        
        protected override void NotifySanepid()
        {
            Context.ActorSelection($"/user/{ActorNames.Sanepid}")
            .Tell(new InfectedForeignerMessage("I'm informing that I'm infected"));
        }

        protected override void GoToQuarantine()
        {
            _isInQuarantine = true;
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new ForeignerGoToQuarantineMessage(""));
            Become(InQuarantine);
        }

        protected override void FinishQuarantine()
        {
            _daysSpentInQuarantine = 0;
            _isInQuarantine = false;
            var sanepid = Context.ActorSelection($"/user/{ActorNames.Sanepid}");
            sanepid.Tell(new FinishForeignerQuarantineMessage(""));
            Become(NotInQuarantine);
        }

        void onInfectedForeignerMessage(InfectedForeignerMessage message)
        {
            this.OnInfectedMessage(new InfectedMessage(message.MessageText));
        }
        
        void onHealthyForeignerMessage(HealthyForeignerMessage message)
        {
            Context.ActorSelection($"/user/{ActorNames.Sanepid}")
                .Tell(new HealthyForeignerMessage(""));
        }
    }

    public class ForeignerGoToQuarantineMessage
    {
        public string MessageText { get; }

        public ForeignerGoToQuarantineMessage(string messageText)
        {
            MessageText = messageText;
        }
    }

    public class FinishForeignerQuarantineMessage
    {
        public string MessageText { get; }

        public FinishForeignerQuarantineMessage(string messageText)
        {
            MessageText = messageText;
        }
    }
    
    public class InfectedForeignerMessage
    {
        public string MessageText { get; }
        public InfectedForeignerMessage(string messageText) => MessageText = messageText;
    }

    public class HealthyForeignerMessage
    {
        public string MessageText { get; }
        public HealthyForeignerMessage(string messageText) => MessageText = messageText;
    }
}