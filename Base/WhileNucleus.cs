namespace Chemistry.Base
{
    public class WhileNucleus : Nucleus
    {
        private class WhileReaction : Reaction
        {
            public override byte? Execute(Erlenmeyer environment)
            {
                while (true)
                {
                    bool? condition = GetInput<bool>(0);
                    if (condition != null)
                    {
                        if ((bool)condition)
                            Trigger(0, environment);
                        else
                            return 1;
                    }
                    else
                        return null;
                }
            }
        }

        public WhileNucleus() : base("While", true, 2, 1, 0)
        {
            SetTrigger(0, string.Empty);
            SetTrigger(1, "Loop");
            SetInput<bool>(0, "Condition");
        }

        protected override Reaction? NewReaction() => new WhileReaction();
    }
}
