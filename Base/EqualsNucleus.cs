namespace StreamChemistry.Base
{
    public class EqualsNucleus : Nucleus
    {
        private class EqualsReaction : Reaction
        {
            public override string? Execute()
            {
                object? a = GetInput<object>("A");
                object? b = GetInput<object>("B");
                if (a != null)
                    SetOutput("Value", a.Equals(b));
                else if (b != null)
                    SetOutput("Value", b.Equals(a));
                else
                    SetOutput("Value", true);
                return null;
            }
        }

        public EqualsNucleus() : base("==", false)
        {
            AddInput<object>("A");
            AddInput<object>("B");
            AddOutput<bool>("Value");
        }

        protected override Reaction? NewReaction() => new EqualsReaction();
    }
}
