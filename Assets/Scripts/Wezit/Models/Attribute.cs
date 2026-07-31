using System;
using System.Collections.Generic;
namespace Wezit
{
	[Serializable]
	public class Attribute
	{
		public string key;
		public string value;
		public string pid;

		public override string ToString()
		{
			return base.ToString() + string.Format(
				"Key: {0}\n" +
				"Value: {1}\n" +
				"pid: {2}\n",
				key,
				value,
				pid
			);
		}
	}

}
