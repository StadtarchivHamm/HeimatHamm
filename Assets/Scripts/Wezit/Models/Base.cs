using System;

namespace Wezit
{
	[Serializable]
	public class Base
	{
		public string pid;
		public string language;
		public string title;
		public string subject;
		public string description;
		public string tags;
		public string identifier;
		public string format;
		public string contributor;
		public string author;
		public string creator;
		public string rights;
		public string type;
		public string date;
		public string source;
		public string spatial;
		public string location;
		public string extent;

		// Wezit V3 info. ////////////////////
		// =>=>=>=> missing fields in Wezit V3 :
		// - editor
		// - date modification
		// =>=>=>=> objects not managed :
		// - aspects
		// ////////////////////////////////////

		public override string ToString()
		{
			return String.Format(
				"Pid: {0}\n" +
				"Language: {1}\n" +
				"Title: {2}\n" +
				"Subject: {3}\n" +
				"Description: {4}\n" +
				"Tags: {5}\n" +
				"identifier: {6}\n" +
				"format: {7}\n" +
				"contributor: {8}\n" +
				"author: {9}\n" +
				"creator: {10}\n" +
				"Type: {11}\n" +
				"Date: {12}\n" +
				"Source: {13}\n" +
				"spatial: {14}\n" +
				"location: {15}\n" +
				"extent: {16}\n",
				pid, language, title, subject, description, tags, identifier, format, contributor, author, creator, rights, type, date, source, spatial, location, extent
			);
		}

		public string CleanedTitle
		{
			get
			{
				return StringUtils.CleanFromWezit(title);
			}
		}

		public string CleanedSubject
		{
			get
			{
				return StringUtils.CleanFromWezit(subject);
			}
		}

		public string CleanedDescription
		{
			get
			{
				return StringUtils.CleanFromWezit(description);
			}
		}
	}

}
