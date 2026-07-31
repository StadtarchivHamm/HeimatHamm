using System;

namespace Wezit
{

	[Serializable]
	public class Analytics
	{
		public string logVersion = "v0.2";
		public string timeStamp = TimeStamp;
		public string applicationVersion = "1.0.0";
		public string systemUserID = "";
		public string wezitUserID = "";
		public string screenID;
		public string screenEvent;
		public string screenEventAttrs = "";

		private static string TimeStamp
		{
			get
			{
				DateTime date = DateTime.Now;
				return date.ToString("yyyy-MM-dd HH:mm:ss");
			}
		}

		public Analytics(string aScreenID, string aScreenEvent, string aScreenEventAttrs)
		{
			screenID = aScreenID;
			screenEvent = aScreenEvent;
			screenEventAttrs = aScreenEventAttrs;
		}
	}

}
