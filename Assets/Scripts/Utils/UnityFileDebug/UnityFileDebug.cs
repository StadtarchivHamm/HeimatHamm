using UnityEngine;
using System.Collections;

namespace Unity
{
	// short keynames are used to make json output small
	[System.Serializable]
	public class LogOutput
	{
		public string t;  //type
		public string tm; //time
		public string l;  //log
		public string s;  //stack
	}

	[ExecuteInEditMode]
	public class FileDebug : MonoBehaviour
	{
		[Header("WRITE IN FILE")]
		public bool writeInFile;
		[Space(5)]
		public bool useAbsolutePath;
		public string absolutePath;
		public string fileName;
		public FileType fileType;
		private string filePath;
		private string filePathFull;
		private int count = 0;

		[Header("SHOW ON UI")]
		public bool showUI;
		[Space(5)]
		public uint maxLogsDisplayed;
		public bool addType;
		public bool addTime;
		public bool addLog;
		public bool addStack;
		private string stringLog;
		private Queue logQueue;

		System.IO.StreamWriter fileWriter;

		public void Init(FileDebugConfigModel config)
		{
			writeInFile = config.writeInFile;
			useAbsolutePath = config.useAbsolutePath;
			absolutePath = config.absolutePath;
			fileName = config.fileName;
			fileType = FileTypeFromString(config.fileType);
			showUI = config.showUI;
			maxLogsDisplayed = config.maxLogsDisplayed;
			addType = config.addType;
			addTime = config.addTime;
			addLog = config.addLog;
			addStack = config.addStack;
		}

		private void Awake()
		{
			logQueue = new Queue();
		}

		private FileType FileTypeFromString(string typeStr)
		{
			switch (typeStr)
			{
				case "json": return FileType.JSON;
				case "csv": return FileType.CSV;
				case "tsv": return FileType.TSV;
				case "txt": return FileType.TXT;
				default: return FileType.TXT;
			}
		}

		private string FileExtensionFromType(FileType type)
		{
			switch (type)
			{
				case FileType.JSON: return ".json";
				case FileType.CSV: return ".csv";
				case FileType.TSV: return ".tsv";
				case FileType.TXT: return ".txt";
				default: return ".txt";
			}
		}

		void OnEnable()
		{
			UpdateFilePath();
			if (Application.isPlaying)
			{
				if (writeInFile)
				{
					count = 0;
					fileWriter = new System.IO.StreamWriter(filePathFull, false);
					fileWriter.AutoFlush = true;
					switch (fileType)
					{
						case FileType.CSV:
							fileWriter.WriteLine("type,time,log,stack");
							break;
						case FileType.JSON:
							fileWriter.WriteLine("[");
							break;
						case FileType.TSV:
							fileWriter.WriteLine("type\ttime\tlog\tstack");
							break;
					}
				}

				// Application.logMessageReceived += HandleLog;
				Application.logMessageReceivedThreaded += HandleLog;
			}
		}

		public void UpdateFilePath()
		{
			filePath = useAbsolutePath ? absolutePath : Application.persistentDataPath;
			filePathFull = System.IO.Path.Combine(filePath, fileName + "." + System.DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss") + FileExtensionFromType(fileType));
		}

		void OnDisable()
		{
			if (Application.isPlaying)
			{
				// Application.logMessageReceived -= HandleLog;
				Application.logMessageReceivedThreaded -= HandleLog;

				if (writeInFile && fileWriter != null)
				{
					switch (fileType)
					{
						case FileType.JSON:
							fileWriter.WriteLine("\n]");
							break;
						case FileType.CSV:
						case FileType.TSV:
						default:
							break;
					}
					fileWriter.Close();
				}
			}
		}

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			LogOutput output = new LogOutput();
			if (type == LogType.Assert)
			{
				output.t = "Assert";
				output.l = logString;
			}
			else if (type == LogType.Exception)
			{
				output.t = "Exception";
				output.l = logString;
			}
			else
			{
				int end = logString.IndexOf("]");
				if (end > 1)
				{
					output.t = logString.Substring(1, end - 1);
					output.l = logString.Substring(end + 2);
				}
			}

			output.s = stackTrace;
			output.tm = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			if (writeInFile && fileWriter != null)
			{
				switch (fileType)
				{
					case FileType.CSV:
						fileWriter.WriteLine(output.t + "," + output.tm + "," + output.l.Replace(",", " ").Replace("\n", "") + "," + output.s.Replace(",", " ").Replace("\n", ""));
						break;
					case FileType.JSON:
						fileWriter.Write((count == 0 ? "" : ",\n") + JsonUtility.ToJson(output));
						break;
					case FileType.TSV:
						fileWriter.WriteLine(output.t + "\t" + output.tm + "\t" + output.l.Replace("\t", " ").Replace("\n", "") + "\t" + output.s.Replace("\t", " ").Replace("\n", ""));
						break;
					case FileType.TXT:
						fileWriter.WriteLine("Type: " + output.t);
						fileWriter.WriteLine("Time: " + output.tm);
						fileWriter.WriteLine("Log: " + output.l);
						fileWriter.WriteLine("Stack: " + output.s);
						break;
				}
				count++;
			}

			if (showUI)
			{
				string newStringLog = "";
				if (addType) newStringLog += addBreakInString(newStringLog) + "-TYPE- : " + output.t;
				if (addTime) newStringLog += addBreakInString(newStringLog) + "-TIME- : " + output.tm;
				if (addLog) newStringLog += addBreakInString(newStringLog) + "-LOG- : " + output.l;
				if (addStack) newStringLog += addBreakInString(newStringLog) + "-STACK- : " + output.s;

				logQueue.Enqueue("\n" + newStringLog);
				if (logQueue.Count > maxLogsDisplayed) logQueue.Dequeue();

				stringLog = string.Empty;
				foreach (string log in logQueue)
				{
					stringLog += log;
				}
			}

			string addBreakInString(string stringSource)
			{
				return stringSource != "" ? "\n" : "";
			}
		}

		void OnGUI()
		{
			if (showUI)
			{
				GUILayout.Label(stringLog);
			}
		}
	}
}
