namespace StreamChemistry
{
    public abstract class Reaction
    {
        private Atom? m_Atom;

        internal void SetAtom(Atom atom) => m_Atom = atom;

        protected void Trigger(byte trigger) => m_Atom!.Trigger(trigger);
        protected T? GetInput<T>(byte input) => m_Atom!.GetInput<T>(input);
        protected void SetOutput(byte output, object? value) => m_Atom!.SetOutput(output, value);

        public abstract byte? Execute();
    }
}
