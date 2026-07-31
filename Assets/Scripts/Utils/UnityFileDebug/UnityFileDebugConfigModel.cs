using System;

namespace Unity
{
	public enum FileType
	{
		CSV,
		JSON,
		TSV,
		TXT
	}

	[Serializable]
	public class FileDebugConfigModel
	{
		public bool writeInFile = false;
		public bool useAbsolutePath = false;
		public string absolutePath = "";
		public string fileName = "";
		public string fileType = "";
		public bool showUI = true;
		public uint maxLogsDisplayed = 30;
		public bool addType = false;
		public bool addTime = true;
		public bool addLog = true;
		public bool addStack = false;

		public override string ToString()
		{
			return String.Format(
				"writeInFile: {0}\n" +
				"useAbsolutePath: {1}\n" +
				"absolutePath: {2}\n" +
				"fileName: {3}\n" +
				"fileType: {4}\n" +
				"showUI: {5}\n" +
				"addType: {6}\n" +
				"addTime: {7}\n" +
				"addLog: {8}\n" +
				"addStack {9}\n",
				writeInFile,
				useAbsolutePath,
				absolutePath,
				fileName,
				fileType,
				showUI,
				maxLogsDisplayed,
				addType,
				addTime,
				addLog,
				addStack
			);
		}
	}

} // end namespace Unity
