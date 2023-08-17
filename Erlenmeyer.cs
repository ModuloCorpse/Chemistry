namespace Chemistry
{
    public class Erlenmeyer
    {
        private readonly Dictionary<string, object?> m_Variables = new();

        public void SetVariable(string name, object? value) => m_Variables[name] = value;
        public object? GetVariable(string name) => (m_Variables.ContainsKey(name)) ? m_Variables[name] : null;
    }
}
