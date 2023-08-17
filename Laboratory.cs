using Chemistry.Base;
using Chemistry.File;

namespace Chemistry
{
    public class Laboratory
    {
        private readonly List<Molecule> m_Molecules = new();
        private readonly Dictionary<string, Nucleus> m_Nucleuses = new();
        private readonly Dictionary<string, uint> m_NucleusesToIdx = new();
        private readonly Dictionary<uint, string> m_IdxToNucleuses = new();
        private uint m_NucleusIdx = 10; //ID 0 to 9 are reserved

        public Laboratory()
        {
            AddNucleus(new BaseEntryNucleus()); //Entry
            AddNucleus(new EqualsNucleus()); //==
            AddNucleus(new IfNucleus()); //If
        }

        public void AddNucleus(Nucleus nucleus)
        {
            uint idx = m_NucleusIdx;
            m_NucleusIdx++;
            m_Nucleuses[nucleus.Name] = nucleus;
            m_IdxToNucleuses[idx] = nucleus.Name;
            m_NucleusesToIdx[nucleus.Name] = idx;
        }

        public List<Molecule> Search<T>(HashSet<int> tags)
        {
            List<Molecule> ret = new();
            foreach (Molecule molecule in m_Molecules)
            {
                if (molecule.IsOfType<T>() && molecule.HasTags(tags))
                    ret.Add(molecule);
            }
            return ret;
        }

        public List<Molecule> Search<T>() => Search<T>(new());

        public Molecule NewMolecule(Type[] parametersType)
        {
            Molecule molecule = new(this, parametersType);
            m_Molecules.Add(molecule);
            return molecule;
        }
        public Molecule NewMolecule() => NewMolecule(Array.Empty<Type>());

        public Molecule NewMolecule<T>(Type[] parametersType)
        {
            Molecule molecule = new(this, typeof(T), parametersType);
            m_Molecules.Add(molecule);
            return molecule;
        }
        public Molecule NewMolecule<T>() => NewMolecule<T>(Array.Empty<Type>());

        internal Atom? NewAtom(string nucleusName, uint id)
        {
            if (m_Nucleuses.TryGetValue(nucleusName, out var nucleus) && m_NucleusesToIdx.TryGetValue(nucleus.Name, out var nucleusIdx))
            {
                Atom newAtom = new(id, nucleusIdx, nucleus.CanEntry, (byte)nucleus.Triggers.Length, nucleus.InputsType, nucleus.OutputsType);
                newAtom.SetReaction(nucleus.CreateReaction());
                return newAtom;
            }
            return null;
        }

        public void Save(string path, Molecule molecule)
        {
            Type? returnType = molecule.ReturnType;
            List<Type> parametersType = new();
            List<Tuple<uint, byte, uint>> executionBonds = new();
            List<Tuple<uint, byte, uint, byte>> ioBonds = new();
            HashSet<int> tags = molecule.Tags;
            Dictionary<string, List<uint>> atoms = new();
            List<Tuple<uint, object>> valueAtoms = new();
            List<Tuple<uint, Type>> getValueAtoms = new();
            List<Tuple<uint, Type>> setValueAtoms = new();

            List<Atom> molAtoms = molecule.Atoms;
            foreach (var atom in molAtoms)
            {
                executionBonds.AddRange(atom.GetBonds());
                ioBonds.AddRange(atom.GetInputBonds());
                if (atom.NucleusID == 0)
                    valueAtoms.Add(new(atom.ID, atom.GetOutputValue()!));
                else if (atom.NucleusID == 1)
                    parametersType = atom.GetOutputsType();
                else if (atom.NucleusID == 2)
                {
                    string nucleus = "Return";
                    if (!atoms.ContainsKey(nucleus))
                        atoms[nucleus] = new();
                    atoms[nucleus].Add(atom.ID);

                }
                else if (atom.NucleusID == 3)
                    getValueAtoms.Add(new(atom.ID, atom.GetOutputsType()[0]));
                else if (atom.NucleusID == 4)
                    setValueAtoms.Add(new(atom.ID, atom.GetInputsType()[0]));
                else
                {
                    string nucleus = m_IdxToNucleuses[atom.NucleusID];
                    if (!atoms.ContainsKey(nucleus))
                        atoms[nucleus] = new();
                    atoms[nucleus].Add(atom.ID);

                }
            }

            Writer writer = new(path);

            //Code 0 & 1
            foreach (var atom in atoms)
            {
                writer.WriteByte(0);
                writer.WriteString(atom.Key);
                foreach (uint id in atom.Value)
                {
                    writer.WriteByte(1);
                    writer.WriteUInt(id);
                }
            }

            //Code 2
            foreach (var valueAtom in valueAtoms)
            {
                writer.WriteByte(2);
                writer.WriteUInt(valueAtom.Item1);
                writer.WriteObject(valueAtom.Item2);
            }

            //Code 3
            if (parametersType.Count != 0)
            {
                writer.WriteByte(3);
                //TODO Write ushort
                writer.WriteInt(parametersType.Count);
                foreach (Type parameterType in parametersType)
                    writer.WriteType(parameterType);
            }

            //Code 4
            foreach (var executionBond in executionBonds)
            {
                writer.WriteByte(4);
                writer.WriteUInt(executionBond.Item1);
                writer.WriteByte(executionBond.Item2);
                writer.WriteUInt(executionBond.Item3);
            }

            //Code 5
            foreach (var ioBond in ioBonds)
            {
                writer.WriteByte(5);
                writer.WriteUInt(ioBond.Item1);
                writer.WriteByte(ioBond.Item2);
                writer.WriteUInt(ioBond.Item3);
                writer.WriteByte(ioBond.Item4);
            }

            //Code 6
            if (returnType != null)
            {
                writer.WriteByte(6);
                writer.WriteType(returnType);
            }

            //Code 7
            if (tags.Count!= 0)
            {
                writer.WriteByte(7);
                //TODO Write ushort
                writer.WriteInt(tags.Count);
                foreach (int tag in tags)
                    writer.WriteInt(tag);
            }

            //Code 8
            foreach (var getValueAtom in getValueAtoms)
            {
                writer.WriteByte(8);
                writer.WriteUInt(getValueAtom.Item1);
                writer.WriteType(getValueAtom.Item2);
            }

            //Code 9
            foreach (var setValueAtom in setValueAtoms)
            {
                writer.WriteByte(9);
                writer.WriteUInt(setValueAtom.Item1);
                writer.WriteType(setValueAtom.Item2);
            }

            writer.Save();
        }

        public Molecule? Load(string path)
        {
            Type? returnType = null;
            List<Type> parametersType = new();
            List<Tuple<uint, byte, uint>> executionBonds = new();
            List<Tuple<uint, byte, uint, byte>> ioBonds = new();
            List<int> tags = new();
            Dictionary<string, List<uint>> atoms = new();
            List<Tuple<uint, object>> valueAtoms = new();
            List<Tuple<uint, Type>> getValueAtoms = new();
            List<Tuple<uint, Type>> setValueAtoms = new();

            string currentNucleus = "";
            Dictionary<uint, Atom> idToAtom = new();

            Reader reader = new(path);
            while (reader.CanRead())
            {
                byte code = reader.ReadByte();
                switch (code)
                {
                    case 0: //Nucleus
                        {
                            currentNucleus = reader.ReadString();
                            break;
                        }
                    case 1: //Atom
                        {
                            if (!atoms.ContainsKey(currentNucleus))
                                atoms[currentNucleus] = new();
                            atoms[currentNucleus].Add(reader.ReadUInt());
                            break;
                        }
                    case 2: //Value Atom
                        {
                            valueAtoms.Add(new(reader.ReadUInt(), reader.ReadObject()!));
                            break;
                        }
                    case 3: //Parameters type
                        {
                            //TODO Read ushort
                            int nbTypes = reader.ReadInt();
                            for (int i = 0; i < nbTypes; i++)
                                parametersType.Add(reader.ReadType()!);
                            break;
                        }
                    case 4: //Execution bond
                        {
                            executionBonds.Add(new(reader.ReadUInt(), reader.ReadByte(), reader.ReadUInt()));
                            break;
                        }
                    case 5: //IO bond
                        {
                            ioBonds.Add(new(reader.ReadUInt(), reader.ReadByte(), reader.ReadUInt(), reader.ReadByte()));
                            break;
                        }
                    case 6: //Return type
                        {
                            returnType = reader.ReadType();
                            break;
                        }
                    case 7: //Tags
                        {
                            //TODO Read ushort
                            int nbTags = reader.ReadInt();
                            for (int i = 0; i < nbTags; i++)
                                tags.Add(reader.ReadInt()!);
                            break;
                        }
                    case 8: //Get Value Type
                        {
                            uint id = reader.ReadUInt();
                            Type? type = reader.ReadType();
                            if (type != null)
                                getValueAtoms.Add(new(id, type!));
                            break;
                        }
                    case 9: //Set Value Type
                        {
                            uint id = reader.ReadUInt();
                            Type? type = reader.ReadType();
                            if (type != null)
                                setValueAtoms.Add(new(id, type!));
                            break;
                        }
                }
            }

            Molecule newMolecule;
            if (returnType == null)
                newMolecule = new(this, parametersType.ToArray());
            else
                newMolecule = new(this, returnType, parametersType.ToArray());

            idToAtom[0] = newMolecule.EntryPoint;

            foreach (int tag in tags)
                newMolecule.AddTag(tag);

            foreach (var atom in atoms)
            {
                foreach (uint atomID in atom.Value)
                {
                    Atom? ret = newMolecule.NewAtom(atom.Key);
                    if (ret != null)
                        idToAtom[atomID] = ret;
                }
            }

            foreach (var valueAtom in valueAtoms)
            {
                Atom? ret = newMolecule.NewValueAtom(valueAtom.Item2);
                if (ret != null)
                    idToAtom[valueAtom.Item1] = ret;
            }

            foreach (var getValueAtom in getValueAtoms)
            {
                Atom? ret = newMolecule.NewGetValueAtom(getValueAtom.Item2);
                if (ret != null)
                    idToAtom[getValueAtom.Item1] = ret;
            }

            foreach (var setValueAtom in setValueAtoms)
            {
                Atom? ret = newMolecule.NewSetValueAtom(setValueAtom.Item2);
                if (ret != null)
                    idToAtom[setValueAtom.Item1] = ret;
            }

            foreach (var executionBond in executionBonds)
            {
                Atom from = idToAtom[executionBond.Item1];
                Atom to = idToAtom[executionBond.Item3];
                from.Bond(executionBond.Item2, to);
            }

            foreach (var ioBond in ioBonds)
            {
                Atom from = idToAtom[ioBond.Item1];
                Atom to = idToAtom[ioBond.Item3];
                from.BondTo(ioBond.Item2, to, ioBond.Item4);
            }

            m_Molecules.Add(newMolecule);
            return newMolecule;
        }
    }
}
