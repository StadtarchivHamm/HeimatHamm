using System;

namespace Wezit
{

	[Serializable]
	public class Cover
	{
		public string pid_src;
		public string pid_dest;

		public override string ToString()
		{
			return string.Format(
				"Covered POI id: {0}\n" +
				"Cover id: {1}\n",
				pid_src, 
				pid_dest
			);
		}
	}

}
