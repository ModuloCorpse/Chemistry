namespace Chemistry.Base
{
    public class EqualsNucleus : Nucleus
    {
        private class EqualsReaction : Reaction
        {
            public override byte? Execute(Erlenmeyer _)
            {
                object? a = GetInput<object>(0);
                object? b = GetInput<object>(1);
                if (a != null)
                    SetOutput(0, a.Equals(b));
                else if (b != null)
                    SetOutput(0, b.Equals(a));
                else
                    SetOutput(0, true);
                return null;
            }
        }

        public EqualsNucleus() : base("==", false, 0, 2, 1)
        {
            SetInput<object>(0, "A");
            SetInput<object>(1, "B");
            SetOutput<bool>(0, "Value");
        }

        protected override Reaction? NewReaction() => new EqualsReaction();
    }
}
