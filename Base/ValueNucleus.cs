namespace StreamChemistry.Base
{
    internal class ValueNucleus : Nucleus
    {
        private class ValueReaction : Reaction
        {
            private readonly object m_Value;

            public ValueReaction(object value) => m_Value = value;

            public override string? Execute()
            {
                SetOutput("Value", m_Value);
                return null;
            }
        }

        private readonly object m_Value;

        public ValueNucleus(object value) : base("Value", false)
        {
            m_Value = value;
            AddOutput("Value", value.GetType());
        }

        protected override Reaction NewReaction() => new ValueReaction(m_Value);
    }
}
