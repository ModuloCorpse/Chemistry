namespace StreamChemistry.Base
{
    public class PrintNucleus : Nucleus
    {
        private class PrintReaction : Reaction
        {
            public override byte? Execute(Erlenmeyer _)
            {
                object? value = GetInput<object>(0);
                Console.WriteLine(value);
                return 0;
            }
        }

        public PrintNucleus() : base("Print", true, 1, 1, 0)
        {
            SetTrigger(0, "");
            SetInput<object>(0, "Value");
        }

        protected override Reaction? NewReaction() => new PrintReaction();
    }
}
