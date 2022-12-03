namespace StreamChemistry
{
    public abstract class Reaction
    {
        private Atom? m_Atom;

        internal void SetAtom(Atom atom) => m_Atom = atom;

        protected void Trigger(string trigger) => m_Atom!.Trigger(trigger);
        protected T? GetInput<T>(string name) => m_Atom!.GetInput<T>(name);
        protected void SetOutput(string name, object? value) => m_Atom!.SetOutput(name, value);

        public abstract string? Execute();
    }
}
