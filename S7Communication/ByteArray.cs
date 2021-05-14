using System.Collections.Generic;

namespace S7Communication
{
    internal class ByteArray
	{
		private List<byte> list = new List<byte>();

		public byte[] array
		{
			get
			{
				return this.list.ToArray();
			}
		}

		public ByteArray()
		{
			this.list = new List<byte>();
		}

		public ByteArray(int size)
		{
			this.list = new List<byte>(size);
		}

		public void Clear()
		{
			this.list = new List<byte>();
		}

		public void Add(byte item)
		{
			this.list.Add(item);
		}

		public void Add(byte[] items)
		{
			this.list.AddRange(items);
		}

		public void Add(ByteArray byteArray)
		{
			this.list.AddRange(byteArray.array);
		}
	}
}
