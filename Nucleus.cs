namespace StreamChemistry
{
    public abstract class Nucleus
    {
        private readonly bool m_CanEntry;
        private readonly string m_Name;
        private readonly HashSet<string> m_Triggers = new();
        private readonly List<Tuple<string, Type>> m_Inputs = new();
        private readonly List<Tuple<string, Type>> m_Outputs = new();

        internal bool CanEntry => m_CanEntry;
        internal string Name => m_Name;
        internal List<Tuple<string, Type>> Inputs => m_Inputs;
        internal List<Tuple<string, Type>> Outputs => m_Outputs;
        internal HashSet<string> Triggers => m_Triggers;

        public Nucleus(string name, bool canEntry)
        {
            m_CanEntry = canEntry;
            m_Name = name;
        }

        public bool AddTrigger(string name) => m_Triggers.Add(name);

        public bool AddInput<T>(string name) => AddInput(name, typeof(T));
        public bool AddInput(string name, Type type)
        {
            if (m_Inputs.Any(item => item.Item1 == name))
                return false;
            m_Inputs.Add(new(name, type));
            return true;
        }

        public bool AddOutput<T>(string name) => AddOutput(name, typeof(T));
        public bool AddOutput(string name, Type type)
        {
            if (m_Outputs.Any(item => item.Item1 == name))
                return false;
            m_Outputs.Add(new(name, type));
            return true;
        }

        internal Reaction? CreateReaction() => NewReaction();
        protected abstract Reaction? NewReaction();
    }
}
