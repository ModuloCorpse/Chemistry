namespace StreamChemistry.Base
{
    public class BaseEntryNucleus : Nucleus
    {
        private class BaseEntryReaction : Reaction { public override string? Execute() => ""; }
        public BaseEntryNucleus() : base("Entry", false) => AddTrigger("");
        protected override sealed Reaction? NewReaction() => new BaseEntryReaction();
    }
}
