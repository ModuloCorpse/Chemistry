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

        internal Atom? NewAtom(string nucleusName, byte[] id)
        {
            if (m_Nucleuses.TryGetValue(nucleusName, out var nucleus))
            {
                Atom newAtom = new(id, nucleus.CanEntry, (byte)nucleus.Triggers.Length, nucleus.InputsType, nucleus.OutputsType);
                newAtom.SetReaction(nucleus.CreateReaction());
                return newAtom;
            }
            return null;
        }
    }
}
