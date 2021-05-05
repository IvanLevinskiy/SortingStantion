using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7Communication.Driver
{
	public static class Word
	{
		public static ushort FromByteArray(byte[] bytes)
		{
			if (bytes.Length != 2)
			{
				throw new ArgumentException("Wrong number of bytes. Bytes array must contain 2 bytes.");
			}
			return Word.FromBytes(bytes[1], bytes[0]);
		}

		public static ushort FromBytes(byte LoVal, byte HiVal)
		{
			return (ushort)((int)HiVal * 256 + (int)LoVal);
		}

		public static byte[] ToByteArray(ushort value)
		{
			byte[] array = new byte[2];
			int num = 2;
			long num2 = (long)((ulong)value);
			for (int i = 0; i < num; i++)
			{
				long num3 = (long)Math.Pow(256.0, (double)i);
				long num4 = num2 / num3;
				array[num - i - 1] = (byte)(num4 & 255L);
				num2 -= (long)((ulong)array[num - i - 1] * (ulong)num3);
			}
			return array;
		}

		public static byte[] ToByteArray(ushort[] value)
		{
			ByteArray byteArray = new ByteArray();
			for (int i = 0; i < value.Length; i++)
			{
				ushort value2 = value[i];
				byteArray.Add(Word.ToByteArray(value2));
			}
			return byteArray.array;
		}

		public static ushort[] ToArray(byte[] bytes)
		{
			ushort[] array = new ushort[bytes.Length / 2];
			int num = 0;
			for (int i = 0; i < bytes.Length / 2; i++)
			{
				array[i] = Word.FromByteArray(new byte[]
				{
					bytes[num++],
					bytes[num++]
				});
			}
			return array;
		}
	}
}
