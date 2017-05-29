using System;
using System.Text;
using System.Collections;

namespace IDEAChipher
{
    public static class ExtensionClass
    {
        public static string ToLiteralString(this ushort[] digitArray)
		{
			string result = string.Empty;
            //UTF8Encoding enc = new UTF8Encoding();

			//result = Encoding.ASCII.GetString(digitArray);

			for (int i = 0; i < digitArray.Length; i++)
			{
                //result += enc.GetBytes()
                char c = Convert.ToChar(digitArray[i]);
                if ((digitArray[i] >= '\xD800') && (digitArray[i] <= '\xDBFF'))
                {
                    string leftPart = Convert.ToString(c);
                    string rightPart = Convert.ToString('\uDC00');
                    result += leftPart + rightPart;
                }
                else if ((digitArray[i] >= '\xDC00') && (digitArray[i] <= '\xDFFF'))
                {
                    string leftPart = Convert.ToString('\uD800');
                    string rightPart = Convert.ToString(c);
                    result += leftPart + rightPart;
                }
                else
                {
                    result += c;
                }
				//result += Convert.ToChar(digitArray[i]);
			}

			return result;
		}

        public static BitArray Append(BitArray current, BitArray after)
        {
            var bools = new bool[current.Count + after.Count];
            var bools1 = new bool[current.Count];
            var bools2 = new bool[after.Count];
            current.CopyTo(bools1, 0);
            after.CopyTo(bools2, 0);

            Reverse(ref bools1);
            Reverse(ref bools2);

            bools1.CopyTo(bools, 0);
            bools2.CopyTo(bools, current.Count);
            Reverse(ref bools);
            return new BitArray(bools);
        }

        public static void Reverse(ref bool[] bools)
        {
            bool[] newBool = new bool[bools.Length];
            for (int i = 0; i < bools.Length; i++)
            {
                newBool[newBool.Length - i - 1] = bools[i];
            }
            bools = newBool;
        }

        public static ushort GetUshortFromBitArray(BitArray bitArray)
        {
            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return (ushort)array[0];
        }

        //expites 32-elems byte array as a parameter
        public static string ByteArrayToString(byte[] old)
        {
            string result = string.Empty;

            StringBuilder builder = new StringBuilder(old.Length / 2);
            if (old != null)
            {
                //result = System.Text.Encoding.Default.GetString(old);
                result = System.Text.Encoding.ASCII.GetString(old);
                char buf;
                for (int i = 0; i < old.Length; i += 2)
                {
                    BitArray bits = new BitArray(new byte[] { old[i] });
                    BitArray bits2 = new BitArray(new byte[] { old[i + 1] });
                    bits = ExtensionClass.Append(bits, bits2);
                    ushort num = ExtensionClass.GetUshortFromBitArray(bits);
                    buf = Convert.ToChar(num);
                    builder.Append(buf);
                }
                result = builder.ToString();
            }
            else
            {
                throw new NullReferenceException("Method ByteArrayToString. Null parameter was given!");
            }

            return result;
        }
    }
}
