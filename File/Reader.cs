using System.IO;
using System.Reflection;
using System.Text;

namespace Chemistry.File
{
    public class Reader
    {
        private readonly byte[] m_Bytes = Array.Empty<byte>();
        private int m_Idx = 0;

        public Reader(string path)
        {
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException();
            m_Idx = 0;
            m_Bytes = System.IO.File.ReadAllBytes(path);
        }

        public Reader(string path, string password): this(path)
        {
            Random rand = new(Helper.GetPassKey(password));
            byte[] randBytes = new byte[m_Bytes.Length];
            rand.NextBytes(randBytes);
            for (int i = 0; i != m_Bytes.Length; i++)
                m_Bytes[i] = (byte)(m_Bytes[i] + randBytes[i]);
        }

        public bool CanRead() => m_Bytes.Length > m_Idx;

        //byte
        public byte ReadByte() => m_Bytes[m_Idx++];
        public sbyte ReadSByte() => (sbyte)m_Bytes[m_Idx++];

        //byte[]
        public byte[] ReadBytes(int nb)
        {
            byte[] ret = m_Bytes.Skip(m_Idx).Take(nb).ToArray();
            m_Idx += nb;
            return ret;
        }
        public byte[] ReadPBytes(int nb)
        {
            byte[] bytes = ReadBytes(nb);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        //bool
        public bool ReadBool() => BitConverter.ToBoolean(ReadPBytes(sizeof(bool)), 0);
        public char ReadChar() => BitConverter.ToChar(ReadPBytes(sizeof(char)), 0);
        public int ReadInt() => BitConverter.ToInt32(ReadPBytes(sizeof(int)), 0);
        public uint ReadUInt() => BitConverter.ToUInt32(ReadPBytes(sizeof(uint)), 0);
        public short ReadShort() => BitConverter.ToInt16(ReadPBytes(sizeof(short)), 0);
        public ushort ReadUShort() => BitConverter.ToUInt16(ReadPBytes(sizeof(ushort)), 0);
        public long ReadLong() => BitConverter.ToInt64(ReadPBytes(sizeof(long)), 0);
        public ulong ReadULong() => BitConverter.ToUInt16(ReadPBytes(sizeof(ulong)), 0);
        public float ReadFloat() => BitConverter.ToSingle(ReadPBytes(sizeof(float)), 0);
        public double ReadDouble() => BitConverter.ToDouble(ReadPBytes(sizeof(double)), 0);

        //string
        public string ReadString()
        {
            int length = ReadInt();
            byte[] bytes = ReadBytes(length);
            Random rand = new(4269);
            byte[] randBytes = new byte[bytes.Length];
            rand.NextBytes(randBytes);
            for (int i = 0; i != bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] + randBytes[i]);
            return Encoding.UTF8.GetString(bytes);
        }

        //Type
        //TODO Switch to ushort ID to reduce file size
        public Type? ReadType() => Type.GetType(ReadString());

        public IObject? ReadFileObject()
        {
            Type? type = ReadType();
            if (type != null && type.IsAssignableTo(typeof(IObject)))
            {
                ConstructorInfo? ctor = type.GetConstructor(new Type[] { typeof(Reader) });
                if (ctor != null)
                    return ctor.Invoke(new object[] { this }) as IObject;
            }
            return null;
        }

        //Object
        public object? ReadObject()
        {
            sbyte type = ReadSByte();
            return type switch
            {
                0 => ReadBool(),
                1 => ReadByte(),
                2 => ReadSByte(),
                3 => ReadChar(),
                4 => ReadShort(),
                5 => ReadUShort(),
                6 => ReadInt(),
                7 => ReadUInt(),
                8 => ReadLong(),
                9 => ReadULong(),
                10 => ReadFloat(),
                11 => ReadDouble(),
                12 => ReadString(),
                13 => ReadFileObject(),
                _ => null
            };
        }
    }
}
