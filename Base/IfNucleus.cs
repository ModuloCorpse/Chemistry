namespace StreamChemistry.Base
{
    public class IfNucleus : Nucleus
    {
        private class IfReaction : Reaction
        {
            public override string? Execute()
            {
                bool? condition = GetInput<bool>("Condition");
                if (condition != null)
                {
                    if ((bool)condition)
                        return "True";
                    else
                        return "False";
                }
                return null;
            }
        }

        public IfNucleus() : base("If", true)
        {
            AddTrigger("True");
            AddTrigger("False");
            AddInput<bool>("Condition");
        }

        protected override Reaction? NewReaction() => new IfReaction();
    }
}
