using System;

namespace Wezit
{

	[Serializable]
	public class Artwork
	{
		public string acquisition;
		public string bibliography;
		public string date_creation;
		public string dimensions;
		public string domain;
		public string graphic_description;
		public string historic_description;
		public string inventory_reference;
		public string pid;
		public string society_description;
		public string subdomain;
		public string title;
		public string type;
		public string usage_description;

		public override string ToString()
		{
			return String.Format(
				"Acquisition: {0}\n" +
				"Bibliography: {1}\n" +
				"Date creation: {2}\n" +
				"Dimensions: {3}\n" +
				"Domain: {4}\n" +
				"Graphic description: {5}\n" +
				"Historic description: {6}\n" +
				"Inventory reference: {7}\n" +
				"Pid: {8}\n" +
				"Society description: {9}\n" +
				"Subdomain: {10}\n" +
				"Title: {11}\n" +
				"Type: {12}\n" +
				"Usage description: {13}\n",
				acquisition, bibliography, date_creation, dimensions,
				domain, graphic_description, historic_description, inventory_reference,
				pid, society_description, subdomain, title, type, usage_description
			);
		}
	}

}
