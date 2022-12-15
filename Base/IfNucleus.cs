namespace StreamChemistry.Base
{
    public class IfNucleus : Nucleus
    {
        private class IfReaction : Reaction
        {
            public override byte? Execute()
            {
                bool? condition = GetInput<bool>(0);
                if (condition != null)
                {
                    if ((bool)condition)
                        return 0;
                    else
                        return 1;
                }
                return null;
            }
        }

        public IfNucleus() : base("If", true, 2, 1, 0)
        {
            SetTrigger(0, "True");
            SetTrigger(1, "False");
            SetInput<bool>(0, "Condition");
        }

        protected override Reaction? NewReaction() => new IfReaction();
    }
}
