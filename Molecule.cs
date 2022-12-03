namespace StreamChemistry
{
    public class Molecule
    {
        private readonly Laboratory m_Laboratory;
        private readonly Type m_ReturnType;
        private Atom? m_EntryPoint = null;
        private readonly List<Atom> m_Returns = new();
        private bool m_IsValid = false;

        internal Molecule(Laboratory laboratory, Type returnType)
        {
            m_Laboratory = laboratory;
            m_ReturnType = returnType;
        }

        public Atom? NewAtom(string name) => m_Laboratory.NewAtom(name);

        public Atom NewValueAtom(object value) => new(new Base.ValueNucleus(value));

        public Atom NewReturnAtom()
        {
            Atom returnAtom = new(new Base.ReturnNucleus(m_ReturnType));
            m_Returns.Add(returnAtom);
            return returnAtom;
        }

        public void SetEntryPoint(Atom entrypoint)
        {
            if (entrypoint.CanBeEntryPoint())
                m_EntryPoint = entrypoint;
        }

        public bool Validate()
        {
            m_IsValid = m_EntryPoint?.Check() ?? false;
            return m_IsValid;
        }

        public void Execute()
        {
            if (m_IsValid)
                m_EntryPoint?.Execute();
        }

        public T? GetResult<T>()
        {
            foreach (Atom atom in m_Returns)
            {
                T? ret = atom.GetInput<T>("Value");
                if (ret != null)
                    return ret;
            }
            return default;
        }
    }
}
