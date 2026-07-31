using System;
using System.Collections.Generic;
namespace Wezit
{
	[Serializable]
	public class TransformedAsset : Base
	{
		public string md5;
		public string path;
		public string uri;
		public int size;
		public string related_pid;
		public string transformation;
		public string mimeType;

		public override string ToString()
		{
			return base.ToString() + String.Format(
				"Title: {0}\n" +
                "pid: {1}\n" +
                "md5: {2}\n" +
                "uri: {3}\n" +
                "path: {4}\n" +
                "size: {5}" +
                "mimeType: {6}",
				title,
				pid,
				md5,
				uri,
				path,
				size,
				mimeType
			);
		}
	}

}
