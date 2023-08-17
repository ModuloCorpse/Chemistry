using System.Reflection;

namespace Chemistry.File
{
    public static class Helper
    {
        private static bool IsFileObject(object obj) => (obj is IObject && obj.GetType().GetConstructor(new Type[] { typeof(Reader) }) != null);

        public static sbyte GetTypeCodeOf(object obj)
        {
            return Type.GetTypeCode(obj.GetType()) switch
            {
                TypeCode.Boolean => 0,
                TypeCode.Byte => 1,
                TypeCode.SByte => 2,
                TypeCode.Char => 3,
                TypeCode.Int16 => 4,
                TypeCode.UInt16 => 5,
                TypeCode.Int32 => 6,
                TypeCode.UInt32 => 7,
                TypeCode.Int64 => 8,
                TypeCode.UInt64 => 9,
                TypeCode.Single => 10,
                TypeCode.Double => 11,
                TypeCode.String => 12,
                _ => (sbyte)(IsFileObject(obj) ? 13 : -1)
            };
        }

        public static int GetPassKey(string password)
        {
            int passKey = 0;
            foreach (char c in password)
                passKey += c;
            return passKey;
        }
    }
}
