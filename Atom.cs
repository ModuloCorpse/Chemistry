namespace Chemistry
{
    public class Atom
    {
        private class Input
        {
            private readonly Atom m_Atom;
            private readonly byte m_Idx;
            private readonly Type m_Type;
            private bool m_HasValue = false;
            private object? m_Value = null;
            private Output? m_Output = null;

            internal object? Value => m_Value;
            internal Type Type => m_Type;

            public Input(Atom atom, byte idx, Type type)
            {
                m_Atom = atom;
                m_Idx = idx;
                m_Type = type;
            }

            public Tuple<uint, byte> GetInputInfo() => new(m_Atom.ID, m_Idx);

            public bool CheckInput(HashSet<uint> atomsID) => m_Output?.CheckInput(atomsID) ?? false;

            public bool Evaluate(Erlenmeyer environment)
            {
                Reset();
                if (m_Output != null)
                {
                    if (m_Output.Evaluate(environment, out object ? value))
                        SetValue(value);
                    return m_HasValue;
                }
                return false;
            }

            public bool IsBonded() => m_Output != null;

            public void SetValue(object? value)
            {
                if (value == null || m_Type.IsAssignableFrom(value.GetType()))
                {
                    if (value == null)
                        m_Value = null;
                    else
                        m_Value = value;
                    m_HasValue = true;
                }
            }

            public void Reset()
            {
                m_Value = null;
                m_HasValue = false;
            }

            public void Bond(Output output) => m_Output = output;
        }
        private class Output
        {
            private readonly Atom m_Atom;
            private readonly Type m_Type;
            private readonly List<Input> m_Inputs = new();
            private bool m_HasValue = false;
            private object? m_Value = null;
            internal Type Type => m_Type;

            public Output(Atom atom, Type type)
            {
                m_Atom = atom;
                m_Type = type;
            }

            public List<Tuple<uint, byte>> GetLinkedInputs()
            {
                List<Tuple<uint, byte>> ret = new();
                foreach (var input in m_Inputs)
                    ret.Add(input.GetInputInfo());
                return ret;
            }

            public bool CheckInput(HashSet<uint> atomsID) => m_Atom.CheckInput(atomsID);

            public bool HaveValue() => m_HasValue;
            public object? GetValue() => m_Value;

            public bool SetValue(object? value)
            {
                if (value == null || m_Type.IsAssignableFrom(value.GetType()))
                {
                    if (value == null)
                        m_Value = null;
                    else
                        m_Value = value;
                    m_HasValue = true;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                m_Value = null;
                m_HasValue = false;
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

            public bool Evaluate(Erlenmeyer environment, out object? value)
            {
                if (!m_Atom.IsStatic())
                    m_Atom.Execute(environment);
                if (m_HasValue)
                {
                    value = m_Value;
                    return true;
                }
                value = null;
                return false;
            }
        }

        private bool m_CanBeReset = true;
        private bool m_WasExecuted = false;
        private readonly bool m_CanEntry;
        private readonly uint m_ID;
        private readonly uint m_NucleusID;
        private Reaction? m_Reaction;
        private readonly List<Atom> m_Caller = new();
        private readonly Atom?[] m_Triggers;
        private readonly Input[] m_Inputs;
        private readonly Output[] m_Outputs;

        public uint ID => m_ID;
        internal uint NucleusID => m_NucleusID;
        public bool WasExecuted => m_WasExecuted;

        internal Atom(uint id, uint nucleusID, bool canEntry, byte nbTriggers, Type[] inputsType, Type[] outputsType)
        {
            m_ID = id;
            m_NucleusID = nucleusID;
            m_CanEntry = canEntry;

            m_Triggers = new Atom?[nbTriggers];
            for (byte i = 0; i != nbTriggers; ++i)
                m_Triggers[i] = null;

            m_Inputs = new Input[inputsType.Length];
            for (byte i = 0; i != inputsType.Length; ++i)
                m_Inputs[i] = new(this, i, inputsType[i]);

            m_Outputs = new Output[outputsType.Length];
            for (byte i = 0; i != outputsType.Length; ++i)
                m_Outputs[i] = new(this, outputsType[i]);
        }

        internal void SetReaction(Reaction? reaction)
        {
            m_Reaction = reaction;
            m_Reaction?.SetAtom(this);
        }

        internal bool IsStatic() => m_CanEntry || m_Triggers.Length > 0 || m_Reaction == null;

        internal bool CanBeEntryPoint() => !m_CanEntry && m_Triggers.Length > 0;

        internal bool Check()
        {
            if (!CheckInput(new()))
                return false;
            foreach (var atom in m_Triggers)
            {
                if (atom != null && !atom.Check())
                    return false;
            }
            return true;
        }

        internal bool CheckInput(HashSet<uint> atomsID)
        {
            if (!atomsID.Add(m_ID))
                return false;
            if (m_CanEntry && m_Caller.Count == 0)
                return false;
            foreach (var input in m_Inputs)
            {
                if (!input.CheckInput(atomsID))
                    return false;
            }
            atomsID.Remove(m_ID);
            return true;
        }

        internal bool Trigger(byte idx, Erlenmeyer environment)
        {
            if (idx >= m_Triggers.Length)
                return false;
            Atom? atom = m_Triggers[idx];
            return atom?.Execute(environment) ?? false;
        }

        public T? GetInput<T>(byte idx)
        {
            if (idx >= m_Inputs.Length)
                return default;
            Input input = m_Inputs[idx];
            if (typeof(T).IsAssignableFrom(input.Type))
                return (T?)input.Value;
            return default;
        }

        public bool SetOutputs(object?[] values)
        {
            byte i = 0;
            foreach (object? value in values)
            {
                if (!SetOutput(i++, value))
                    return false;
            }
            return true;
        }

        public bool SetOutputs(List<Tuple<byte, object?>> outputs)
        {
            foreach (Tuple<byte, object?> output in outputs)
            {
                if (!SetOutput(output.Item1, output.Item2))
                    return false;
            }
            return true;
        }

        public bool SetOutput(byte idx, object? value)
        {
            if (idx >= m_Outputs.Length)
                return false;
            return m_Outputs[idx].SetValue(value);
        }


        internal bool Execute(Erlenmeyer environment)
        {
            foreach (Input input in m_Inputs)
            {
                if (!input.Evaluate(environment))
                    return false;
            }
            byte? exitPoint = m_Reaction?.Execute(environment);
            foreach (Output output in m_Outputs)
            {
                if (!output.HaveValue())
                    return false;
            }
            if (exitPoint != null)
            {
                m_WasExecuted = Trigger((byte)exitPoint, environment);
                return m_WasExecuted;
            }
            m_WasExecuted = true;
            return true;
        }

        public bool Bond(byte idx, Atom atom)
        {
            if (!atom.m_CanEntry)
                return false;
            if (idx >= m_Triggers.Length)
                return false;
            atom.m_Caller.Add(this);
            m_Triggers[idx] = atom;
            return true;
        }

        public bool BondTo(byte outputIdx, Atom atom, byte inputIdx)
        {
            if (outputIdx >= m_Outputs.Length ||
                inputIdx >= atom.m_Inputs.Length)
                return false;
            return m_Outputs[outputIdx].Bond(atom.m_Inputs[inputIdx]);
        }

        internal List<Tuple<uint, byte, uint>> GetBonds()
        {
            List<Tuple<uint, byte, uint>> ret = new();
            for (byte i = 0; i != m_Triggers.Length; ++i)
            {
                Atom? atom = m_Triggers[i];
                if (atom != null)
                    ret.Add(new(m_ID, i, atom.ID));
            }
            return ret;
        }

        internal List<Tuple<uint, byte, uint, byte>> GetInputBonds()
        {
            List<Tuple<uint, byte, uint, byte>> ret = new();
            for (byte i = 0; i != m_Outputs.Length; ++i)
            {
                List<Tuple<uint, byte>> linkedInputs = m_Outputs[i].GetLinkedInputs();
                foreach (var linkedInput in linkedInputs)
                    ret.Add(new(m_ID, i, linkedInput.Item1, linkedInput.Item2));
            }
            return ret;
        }

        internal List<Type> GetInputsType()
        {
            List<Type> ret = new();
            foreach (Input input in m_Inputs)
                ret.Add(input.Type);
            return ret;
        }

        internal List<Type> GetOutputsType()
        {
            List<Type> ret = new();
            foreach (Output output in m_Outputs)
                ret.Add(output.Type);
            return ret;
        }

        internal object? GetOutputValue()
        {
            if (m_NucleusID == 0)
                return m_Outputs[0].GetValue();
            return null;
        }

        public void Reset()
        {
            if (m_CanBeReset)
            {
                m_WasExecuted = false;
                foreach (Input input in m_Inputs)
                    input.Reset();
                foreach (Output output in m_Outputs)
                    output.Reset();
            }
        }

        public void SetCanBeReset(bool canBeReset) => m_CanBeReset = canBeReset;
    }
}
