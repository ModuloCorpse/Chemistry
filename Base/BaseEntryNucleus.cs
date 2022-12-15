namespace StreamChemistry.Base
{
    public class BaseEntryNucleus : Nucleus
    {
        private class BaseEntryReaction : Reaction { public override byte? Execute() => 0; }
        public BaseEntryNucleus() : base("Entry", false, 1, 0, 0) => SetTrigger(0, "");
        protected override sealed Reaction? NewReaction() => new BaseEntryReaction();
    }
}
