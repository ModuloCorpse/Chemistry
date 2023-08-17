namespace Chemistry
{
    public abstract class Nucleus
    {
        private readonly bool m_CanEntry;
        private readonly string m_Name;
        private readonly string[] m_Triggers;
        private readonly string[] m_InputsName;
        private readonly Type[] m_InputsType;
        private readonly string[] m_OutputsName;
        private readonly Type[] m_OutputsType;

        internal bool CanEntry => m_CanEntry;
        internal string Name => m_Name;
        public string[] Triggers => m_Triggers;
        public string[] InputsName => m_InputsName;
        public Type[] InputsType => m_InputsType;
        public string[] OutputsName => m_OutputsName;
        public Type[] OutputsType => m_OutputsType;

        public Nucleus(string name, bool canEntry, byte nbTrigger, byte nbInput, byte nbOutput)
        {
            m_Name = name;
            m_CanEntry = canEntry;
            m_Triggers = new string[nbTrigger];
            m_InputsName = new string[nbInput];
            m_InputsType = new Type[nbInput];
            m_OutputsName = new string[nbOutput];
            m_OutputsType = new Type[nbOutput];
        }

        public bool SetTrigger(byte idx, string name)
        {
            if (idx >= m_Triggers.Length || m_Triggers.Any(item => item == name))
                return false;
            m_Triggers[idx] = name;
            return true;
        }

        public bool SetInput<T>(byte idx, string name) => SetInput(idx, name, typeof(T));
        public bool SetInput(byte idx, string name, Type type)
        {
            if (idx >= m_InputsName.Length || m_InputsName.Any(item => item == name))
                return false;
            m_InputsName[idx] = name;
            m_InputsType[idx] = type;
            return true;
        }

        public bool SetOutput<T>(byte idx, string name) => SetOutput(idx, name, typeof(T));
        public bool SetOutput(byte idx, string name, Type type)
        {
            if (idx >= m_OutputsName.Length || m_OutputsName.Any(item => item == name))
                return false;
            m_OutputsName[idx] = name;
            m_OutputsType[idx] = type;
            return true;
        }

        internal Reaction? CreateReaction() => NewReaction();
        protected abstract Reaction? NewReaction();
    }
}
