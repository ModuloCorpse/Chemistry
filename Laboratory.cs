using StreamChemistry.Base;

namespace StreamChemistry
{
    public class Laboratory
    {
        private readonly Dictionary<string, Nucleus> m_Nucleuses = new();

        public Laboratory()
        {
            AddNucleus(new BaseEntryNucleus()); //Entry
            AddNucleus(new EqualsNucleus()); //==
            AddNucleus(new IfNucleus()); //If
        }

        public void AddNucleus(Nucleus nucleus) => m_Nucleuses[nucleus.Name] = nucleus;

        public Molecule NewMolecule<T>() => new(this, typeof(T));

        internal Atom? NewAtom(string name) => (m_Nucleuses.TryGetValue(name, out var nucleus)) ? new(nucleus) : null;
    }
}
