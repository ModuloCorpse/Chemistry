namespace StreamChemistry.Base
{
    public class ReturnNucleus : Nucleus
    {
        public ReturnNucleus(Type returnType) : base("Return", true) => AddInput("Value", returnType);
        protected override Reaction? NewReaction() => null;
    }
}
