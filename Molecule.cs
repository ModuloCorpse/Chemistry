using System.Text;

namespace StreamChemistry
{
    public class Molecule
    {
        private readonly Type m_ReturnType;
        private readonly Laboratory m_Laboratory;
        private Atom? m_EntryPoint = null;
        private readonly List<Atom> m_Atoms = new();
        private readonly List<Atom> m_Returns = new();
        private bool m_IsValid = false;

        internal Molecule(Laboratory laboratory, Type returnType)
        {
            m_Laboratory = laboratory;
            m_ReturnType = returnType;
        }

        internal Atom? NewAtom(string nucleusName, string id)
        {
            if (nucleusName == "Return")
            {
                Atom returnAtom = new(Encoding.ASCII.GetBytes(id), true, 0, new Type[1] { m_ReturnType }, Array.Empty<Type>());
                m_Returns.Add(returnAtom);
                m_Atoms.Add(returnAtom);
                return returnAtom;
            }
            Atom? atom = m_Laboratory.NewAtom(nucleusName, Encoding.ASCII.GetBytes(id));
            if (atom != null)
                m_Atoms.Add(atom);
            return atom;
        }

        public Atom? NewAtom(string nucleusName) => NewAtom(nucleusName, string.Format("{0}", m_Atoms.Count.ToString("X")));

        internal Atom NewValueAtom(object value, string id)
        {
            Atom atom = new(Encoding.ASCII.GetBytes(id), false, 0, Array.Empty<Type>(), new Type[1] { value.GetType() });
            atom.SetOutput(0, value);
            atom.SetCanBeReset(false);
            m_Atoms.Add(atom);
            return atom;
        }

        public Atom NewValueAtom(object value) => NewValueAtom(value, string.Format("{0}", m_Atoms.Count.ToString("X")));

        public void SetEntryPoint(Atom entrypoint)
        {
            if (entrypoint.CanBeEntryPoint())
                m_EntryPoint = entrypoint;
        }

        public bool Execute<T>(out T? ret)
        {
            if (!m_IsValid)
            {
                m_IsValid = m_EntryPoint?.Check() ?? false;
                if (!m_IsValid)
                {
                    ret = default;
                    return false;
                }
            }
            foreach (Atom atom in m_Atoms)
                atom.Reset();
            m_EntryPoint?.Execute();
            foreach (Atom atom in m_Returns)
            {
                if (atom.WasExecuted)
                {
                    ret = atom.GetInput<T>(0);
                    return true;
                }
            }
            ret = default;
            return false;
        }
    }
}
