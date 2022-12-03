namespace StreamChemistry.Base
{
    public class LambdaNucleus : Nucleus
    {
        public delegate Reaction? CreateBehaviourDelegate();

        private readonly CreateBehaviourDelegate m_Behaviour;

        public LambdaNucleus(string name, bool canEntry, CreateBehaviourDelegate behaviour) : base(name, canEntry)
        {
            m_Behaviour = behaviour;
        }

        protected override Reaction? NewReaction() => m_Behaviour();
    }
}
