namespace StreamChemistry
{
    public class Atom
    {
        private class Input
        {
            private readonly Atom m_Atom;
            private readonly string m_Name;
            private readonly Type m_Type;
            private bool m_HasValue = false;
            private object? m_Value = null;
            private Output? m_Output = null;

            internal object? Value => m_Value;
            internal Type Type => m_Type;

            public Input(Atom atom, string name, Type type)
            {
                m_Atom = atom;
                m_Name = name;
                m_Type = type;
            }

            public Tuple<string, string> GetInputInfo() => new(m_Atom.ID, m_Name);

            public bool CheckInput(ref HashSet<string> atomsID) => m_Output?.CheckInput(ref atomsID) ?? false;

            public bool Evaluate()
            {
                if (m_HasValue)
                    return true;
                if (m_Output != null)
                {
                    m_Output.Evaluate();
                    return m_HasValue;
                }
                return false;
            }

            public bool IsBonded() => m_Output != null;

            public void SetValue(object? value)
            {
                if (value == null || m_Type.IsAssignableFrom(value.GetType()))
                {
                    m_Value = value;
                    m_HasValue = true;
                }
            }

            public void Bond(Output output) => m_Output = output;
        }
        private class Output
        {
            private readonly Atom m_Atom;
            private readonly string m_Name;
            private readonly Type m_Type;
            private readonly List<Input> m_Inputs = new();
            private bool m_HasValue = false;
            private object? m_Value = null;

            public Output(Atom atom, string name, Type type)
            {
                m_Atom = atom;
                m_Name = name;
                m_Type = type;
            }

            public List<Tuple<string, string>> GetLinkedInputs()
            {
                List<Tuple<string, string>> ret = new();
                foreach (var input in m_Inputs)
                    ret.Add(input.GetInputInfo());
                return ret;
            }

            public bool CheckInput(ref HashSet<string> atomsID) => m_Atom.CheckInput(ref atomsID);

            public bool HaveValue() => m_HasValue;

            public bool SetValue(object? value)
            {
                if (value == null || m_Type.IsAssignableFrom(value.GetType()))
                {
                    m_Value = value;
                    m_HasValue = true;
                    foreach (var input in m_Inputs)
                        input.SetValue(m_Value);
                    return true;
                }
                return false;
            }

            public bool Bond(Input input)
            {
                if (!input.IsBonded() && input.Type.IsAssignableFrom(m_Type))
                {
                    m_Inputs.Add(input);
                    input.Bond(this);
                    if (m_HasValue)
                        input.SetValue(m_Value);
                    return true;
                }
                return false;
            }

            public void Evaluate() => m_Atom.Execute();
        }

        private readonly bool m_CanEntry;
        private readonly string m_ID = Guid.NewGuid().ToString();
        private Reaction? m_Reaction;
        private Atom? m_Caller = null;
        private readonly Dictionary<string, Atom?> m_Triggers = new();
        private readonly Dictionary<string, Input> m_Inputs = new();
        private readonly Dictionary<string, Output> m_Outputs = new();

        public string ID => m_ID;

        internal Atom(Nucleus nucleus)
        {
            m_CanEntry = nucleus.CanEntry;
            SetReaction(nucleus.CreateReaction());
            foreach (Tuple<string, Type> input in nucleus.Inputs)
                m_Inputs[input.Item1] = new(this, input.Item1, input.Item2);
            foreach (Tuple<string, Type> output in nucleus.Outputs)
                m_Outputs[output.Item1] = new(this, output.Item1, output.Item2);
            foreach (string triggers in nucleus.Triggers)
                m_Triggers[triggers] = null;
        }

        internal void SetReaction(Reaction? reaction)
        {
            m_Reaction = reaction;
            m_Reaction?.SetAtom(this);
        }

        internal bool CanBeEntryPoint() => !m_CanEntry && m_Triggers.Count > 0;

        internal bool Check()
        {
            if (m_CanEntry && m_Caller == null)
                return false;
            HashSet<string> recursiveCheck = new();
            if (!CheckInput(ref recursiveCheck))
                return false;
            foreach (var atom in m_Triggers.Values)
            {
                if (atom != null && !atom.Check())
                    return false;
            }
            return true;
        }

        internal bool CheckInput(ref HashSet<string> atomsID)
        {
            if (!atomsID.Add(m_ID))
                return false;
            foreach (var input in m_Inputs.Values)
            {
                if (!input.CheckInput(ref atomsID))
                    return false;
            }
            return true;
        }

        internal bool Trigger(string trigger)
        {
            if (m_Triggers.TryGetValue(trigger, out var atom))
                return atom?.Execute() ?? false;
            return false;
        }

        public T? GetInput<T>(string name)
        {
            if (m_Inputs.TryGetValue(name, out var input))
            {
                if (typeof(T).IsAssignableFrom(input.Type))
                    return (T?)input.Value;
            }
            return default;
        }

        public void SetOutput(string name, object? value)
        {
            if (m_Outputs.TryGetValue(name, out var output))
                output.SetValue(value);
        }

        internal bool Execute()
        {
            foreach (var input in m_Inputs)
            {
                if (!input.Value.Evaluate())
                    return false;
            }
            string? exitPoint = m_Reaction?.Execute();
            foreach (var output in m_Outputs)
            {
                if (!output.Value.HaveValue())
                    return false;
            }
            if (exitPoint != null)
                Trigger(exitPoint);
            return true;
        }

        public bool Bond(string trigger, Atom atom)
        {
            if (!atom.m_CanEntry)
                return false;
            if (m_Triggers.ContainsKey(trigger))
            {
                atom.m_Caller = this;
                m_Triggers[trigger] = atom;
                return true;
            }
            return false;
        }

        internal List<Tuple<string, string>> GetBonds()
        {
            List<Tuple<string, string>> ret = new();
            foreach (var trigger in m_Triggers)
            {
                if (trigger.Value != null)
                    ret.Add(new(trigger.Key, trigger.Value.ID));
            }
            return ret;
        }

        public bool BondTo(string outputName, Atom atom, string inputName)
        {
            if (m_Outputs.TryGetValue(outputName, out var output))
            {
                if (atom.m_Inputs.TryGetValue(inputName, out var input))
                    return output.Bond(input);
            }
            return false;
        }

        internal List<Tuple<string, List<Tuple<string, string>>>> GetInputBonds()
        {
            List<Tuple<string, List<Tuple<string, string>>>> ret = new();
            foreach (var output in m_Outputs)
                ret.Add(new(output.Key, output.Value.GetLinkedInputs()));
            return ret;
        }
    }
}
