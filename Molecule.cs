using System.Text;

namespace StreamChemistry
{
    public class Molecule
    {
        public delegate T? MoleculeDelegate<T>(params object?[] parameters);
        private class EntryPointReaction : Reaction { public override byte? Execute(Erlenmeyer _) => 0; }

        private class GetValueReaction : Reaction
        {
            public override byte? Execute(Erlenmeyer environment)
            {
                string? name = GetInput<string>(0);
                if (name != null)
                    SetOutput(0, environment.GetVariable(name));
                return null;
            }
        }

        private class SetValueReaction : Reaction
        {
            public override byte? Execute(Erlenmeyer environment)
            {
                object? value = GetInput<object>(0);
                string? name = GetInput<string>(1);
                if (value != null && name != null)
                {
                    environment.SetVariable(name, value);
                    return 0;
                }
                return null;
            }
        }

        private readonly Type? m_ReturnType;
        private readonly Laboratory m_Laboratory;
        private readonly Atom m_EntryPoint;
        private readonly int m_NbParameters;
        private readonly List<Atom> m_Atoms = new();
        private readonly List<Atom> m_Returns = new();
        private readonly HashSet<int> m_Tags = new();
        private bool m_IsValid = false;

        private static Atom CreateEntryPoint(Type[] parametersType)
        {
            Atom entry = new(0, 1, false, 1, Array.Empty<Type>(), parametersType);
            entry.SetReaction(new EntryPointReaction());
            return entry;
        }

        internal Molecule(Laboratory laboratory, Type[] parametersType)
        {
            m_Laboratory = laboratory;
            m_ReturnType = null;
            m_EntryPoint = CreateEntryPoint(parametersType);
            m_NbParameters = parametersType.Length;
            m_Atoms.Add(m_EntryPoint);
        }

        internal Molecule(Laboratory laboratory, Type returnType, Type[] parametersType) : this(laboratory, parametersType)
        {
            m_ReturnType = returnType;
        }

        public Atom EntryPoint => m_EntryPoint;
        internal Type? ReturnType => m_ReturnType;
        internal List<Atom> Atoms => m_Atoms;
        internal HashSet<int> Tags => m_Tags;

        public bool HasTags(HashSet<int> tags)
        {
            foreach (int tag in tags)
            {
                if (!m_Tags.Contains(tag))
                    return false;
            }
            return true;
        }

        public bool HasTag(dynamic tag) => m_Tags.Contains((int)tag);
        public void AddTag(dynamic tag) => m_Tags.Add((int)tag);
        public void RemoveTag(dynamic tag) => m_Tags.Remove((int)tag);

        internal bool IsOfType<T>() => typeof(T).IsAssignableFrom(m_ReturnType);

        public Atom? NewAtom(string nucleusName)
        {
            if (nucleusName == "Return")
            {
                if (m_ReturnType == null)
                    return null;
                Atom returnAtom = new((uint)m_Atoms.Count, 2, true, 0, new Type[1] { m_ReturnType }, Array.Empty<Type>());
                m_Returns.Add(returnAtom);
                m_Atoms.Add(returnAtom);
                return returnAtom;
            }
            Atom? atom = m_Laboratory.NewAtom(nucleusName, (uint)m_Atoms.Count);
            if (atom != null)
                m_Atoms.Add(atom);
            return atom;
        }

        public Atom? NewGetValueAtom(Type valueType)
        {
            Atom atom = new((uint)m_Atoms.Count, 3, false, 0, new Type[1] { typeof(string) }, new Type[1] { valueType });
            atom.SetReaction(new GetValueReaction());
            m_Atoms.Add(atom);
            return atom;
        }
        public Atom? NewGetValueAtom<T>() => NewGetValueAtom(typeof(T));

        public Atom? NewSetValueAtom(Type valueType)
        {
            Atom atom = new((uint)m_Atoms.Count, 4, true, 1, new Type[2] { valueType, typeof(string) }, Array.Empty<Type>());
            atom.SetReaction(new SetValueReaction());
            m_Atoms.Add(atom);
            return atom;
        }
        public Atom? NewSetValueAtom<T>() => NewSetValueAtom(typeof(T));

        public Atom? NewValueAtom(object value)
        {
            if (File.Helper.GetTypeCodeOf(value) == -1)
                return null;
            Atom atom = new((uint)m_Atoms.Count, 0, false, 0, Array.Empty<Type>(), new Type[1] { value.GetType() });
            atom.SetOutput(0, value);
            atom.SetCanBeReset(false);
            m_Atoms.Add(atom);
            return atom;
        }

        private Atom? InternalExecute(object?[] parameters)
        {
            if (parameters.Length != m_NbParameters)
                return null;
            if (!m_IsValid)
            {
                m_IsValid = m_EntryPoint.Check();
                if (!m_IsValid)
                    return null;
            }
            foreach (Atom atom in m_Atoms)
                atom.Reset();
            if (!m_EntryPoint.SetOutputs(parameters))
                return null;
            Erlenmeyer environment = new();
            m_EntryPoint.Execute(environment);
            foreach (Atom atom in m_Returns)
            {
                if (atom.WasExecuted)
                    return atom;
            }
            return null;
        }

        public bool Execute(params object?[] parameters)
        {
            if (m_ReturnType == null)
            {
                InternalExecute(parameters);
                return (parameters.Length == m_NbParameters && m_IsValid);
            }
            return false;
        }

        public T? Execute<T>(params object?[] parameters)
        {
            if (IsOfType<T>())
            {
                Atom? returnAtom = InternalExecute(parameters);
                if (returnAtom != null)
                    return returnAtom.GetInput<T>(0);
            }
            return default;
        }

        public bool Execute<T>(out T? ret, params object?[] parameters)
        {
            if (IsOfType<T>())
            {
                Atom? returnAtom = InternalExecute(parameters);
                if (returnAtom != null)
                {
                    ret = returnAtom.GetInput<T>(0);
                    return true;
                }
            }
            ret = default;
            return false;
        }
    }
}
