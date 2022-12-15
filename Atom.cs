using System.Text;

namespace StreamChemistry
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

            public Tuple<byte[], byte> GetInputInfo() => new(m_Atom.ID, m_Idx);

            public bool CheckInput(ref HashSet<string> atomsID) => m_Output?.CheckInput(ref atomsID) ?? false;

            public bool Evaluate()
            {
                if (m_HasValue)
                    return true;
                if (m_Output != null)
                {
                    if (m_Output.HaveValue())
                        SetValue(m_Output.GetValue());
                    else
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

            public Output(Atom atom, Type type)
            {
                m_Atom = atom;
                m_Type = type;
            }

            public List<Tuple<byte[], byte>> GetLinkedInputs()
            {
                List<Tuple<byte[], byte>> ret = new();
                foreach (var input in m_Inputs)
                    ret.Add(input.GetInputInfo());
                return ret;
            }

            public bool CheckInput(ref HashSet<string> atomsID) => m_Atom.CheckInput(ref atomsID);

            public bool HaveValue() => m_HasValue;
            public object? GetValue() => m_Value;

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

            public void Evaluate() => m_Atom.Execute();
        }

        private bool m_CanBeReset = true;
        private bool m_WasExecuted = false;
        private readonly bool m_CanEntry;
        private readonly byte[] m_ID;
        private Reaction? m_Reaction;
        private readonly List<Atom> m_Caller = new();
        private readonly Atom?[] m_Triggers;
        private readonly Input[] m_Inputs;
        private readonly Output[] m_Outputs;

        public byte[] ID => m_ID;
        public bool WasExecuted => m_WasExecuted;

        internal Atom(byte[] id, bool canEntry, byte nbTriggers, Type[] inputsType, Type[] outputsType)
        {
            //TODO Store nucleus, and at serialization if no nucleus is stored (value or return) save inputs type or outputs value and type
            m_ID = id;
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

        internal bool CanBeEntryPoint() => !m_CanEntry && m_Triggers.Length > 0;

        internal bool Check()
        {
            HashSet<string> recursiveCheck = new();
            if (!CheckInput(ref recursiveCheck))
                return false;
            foreach (var atom in m_Triggers)
            {
                if (atom != null && !atom.Check())
                    return false;
            }
            return true;
        }

        internal bool CheckInput(ref HashSet<string> atomsID)
        {
            if (!atomsID.Add(Encoding.ASCII.GetString(m_ID)))
                return false;
            if (m_CanEntry && m_Caller.Count == 0)
                return false;
            foreach (var input in m_Inputs)
            {
                if (!input.CheckInput(ref atomsID))
                    return false;
            }
            return true;
        }

        internal bool Trigger(byte idx)
        {
            if (idx >= m_Triggers.Length)
                return false;
            Atom? atom = m_Triggers[idx];
            return atom?.Execute() ?? false;
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

        public void SetOutput(byte idx, object? value)
        {
            if (idx >= m_Outputs.Length)
                return;
            m_Outputs[idx].SetValue(value);
        }

        internal bool Execute()
        {
            foreach (Input input in m_Inputs)
            {
                if (!input.Evaluate())
                    return false;
            }
            byte? exitPoint = m_Reaction?.Execute();
            foreach (Output output in m_Outputs)
            {
                if (!output.HaveValue())
                    return false;
            }
            if (exitPoint != null)
            {
                m_WasExecuted = Trigger((byte)exitPoint);
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

        internal List<Tuple<byte, byte[]>> GetBonds()
        {
            List<Tuple<byte, byte[]>> ret = new();
            for (byte i = 0; i != m_Triggers.Length; ++i)
            {
                Atom? atom = m_Triggers[i];
                if (atom != null)
                    ret.Add(new(i, atom.ID));
            }
            return ret;
        }

        public bool BondTo(byte outputIdx, Atom atom, byte inputIdx)
        {
            if (outputIdx >= m_Outputs.Length ||
                inputIdx >= atom.m_Inputs.Length)
                return false;
            return m_Outputs[outputIdx].Bond(atom.m_Inputs[inputIdx]);
        }

        internal List<Tuple<byte, List<Tuple<byte[], byte>>>> GetInputBonds()
        {
            List<Tuple<byte, List<Tuple<byte[], byte>>>> ret = new();
            for (byte i = 0; i != m_Outputs.Length; ++i)
                ret.Add(new(i, m_Outputs[i].GetLinkedInputs()));
            return ret;
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
