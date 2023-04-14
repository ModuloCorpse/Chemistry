using System.Text;

namespace StreamChemistry.File
{
    public class Writer
    {
        private readonly string m_Path;
        private byte[] m_Bytes = Array.Empty<byte>();

        public Writer(string path)
        {
            m_Path = path;
        }

        public void Save() => System.IO.File.WriteAllBytes(m_Path, m_Bytes);
        public void Save(string password)
        {
            byte[] bytes = new byte[m_Bytes.Length];
            Random rand = new(Helper.GetPassKey(password));
            byte[] randBytes = new byte[bytes.Length];
            rand.NextBytes(randBytes);
            for (int i = 0; i != m_Bytes.Length; i++)
                bytes[i] = (byte)(m_Bytes[i] - randBytes[i]);
            System.IO.File.WriteAllBytes(m_Path, bytes);
        }

        //byte
        public void WriteByte(byte value)
        {
            byte[] tmp = new byte[m_Bytes.Length + 1];
            m_Bytes.CopyTo(tmp, 0);
            tmp[m_Bytes.Length] = value;
            m_Bytes = tmp;
        }
        public void WriteSByte(sbyte value) => WriteByte((byte)value);

        //byte[]
        public void WriteBytes(byte[] bytes)
        {
            byte[] tmp = new byte[m_Bytes.Length + bytes.Length];
            m_Bytes.CopyTo(tmp, 0);
            bytes.CopyTo(tmp, m_Bytes.Length);
            m_Bytes = tmp;
        }

        private void WriteP(dynamic value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void WriteBool(bool value) => WriteP(value);
        public void WriteChar(char value) => WriteP(value);
        public void WriteInt(int value) => WriteP(value);
        public void WriteUInt(uint value) => WriteP(value);
        public void WriteShort(short value) => WriteP(value);
        public void WriteUShort(ushort value) => WriteP(value);
        public void WriteLong(long value) => WriteP(value);
        public void WriteULong(ulong value) => WriteP(value);
        public void WriteFloat(float value) => WriteP(value);
        public void WriteDouble(double value) => WriteP(value);

        //string
        public void WriteString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            Random rand = new(4269);
            byte[] randBytes = new byte[bytes.Length];
            rand.NextBytes(randBytes);
            for (int i = 0; i != bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] - randBytes[i]);
            WriteInt(bytes.Length);
            WriteBytes(bytes);
        }

        //Type
        //TODO Switch to ushort ID to reduce file size
        public void WriteType(Type type)
        {
            string? typeName = type.AssemblyQualifiedName;
            if (typeName != null)
                WriteString(typeName);
        }

        //Object
        public void WriteObject(object obj)
        {
            sbyte code = Helper.GetTypeCodeOf(obj);
            if (code == -1)
                return;
            WriteSByte(code);
            switch (code)
            {
                case 0: WriteP((bool)obj); break;
                case 1: WriteP((byte)obj); break;
                case 2: WriteP((sbyte)obj); break;
                case 3: WriteP((char)obj); break;
                case 4: WriteP((short)obj); break;
                case 5: WriteP((ushort)obj); break;
                case 6: WriteP((int)obj); break;
                case 7: WriteP((uint)obj); break;
                case 8: WriteP((long)obj); break;
                case 9: WriteP((ulong)obj); break;
                case 10: WriteP((float)obj); break;
                case 11: WriteP((double)obj); break;
                case 12: WriteString((string)obj); break;
                case 13: WriteType(obj.GetType()); ((IObject)obj).Save(this); break;
            }
        }
    }
}
