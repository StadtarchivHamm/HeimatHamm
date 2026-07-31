using System;

namespace Wezit
{

	[Serializable]
	public class GlossaryEntry
	{
		public string pid;
		public string language;
		public string word;
		public string synonyms;
		public string description;

		public override string ToString()
		{
			return String.Format(
				"Pid: {0}\n" +
				"Language: {1}\n" +
				"Word: {2}\n" +
				"Synonyms: {3}\n" +
				"Description: {4}\n",
				pid, language, word, synonyms, description
			);
		}
	}

}
