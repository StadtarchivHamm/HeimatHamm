using System;

namespace Wezit
{

	[Serializable]
	public class Manifest
	{
		[Serializable]
		public class Settings
		{
			public string path;
			public string url;
		}

		[Serializable]
		public class Assets
		{
			public string path;
			public string url;
		}

		[Serializable]
		public class Contents
		{
            [Serializable]
			public class TourSql
            {
				public string path;
				public string url;
            }
			public TourSql toursql;
			public string pid;

		}

		[Serializable]
		public class Metadatas
		{
			public string version;
			public string bundle;
			public string name;
			public string description;
		}

		[Serializable]
		public class Service
		{
			public string urlbase;

            [Serializable]
			public class Entity
            {
				public string name;
				public string id;
				public string desc;
            }
			public Entity entity;
		}

		public class Self
        {
			public string path;
			public string url;
        }

		public Settings settings = new Settings();
		public Assets assets = new Assets();
		public Contents contents = new Contents();
		public Metadatas metadatas = new Metadatas();
		public Service service = new Service();
		public string pid;
		public Self self;

		public override string ToString()
		{
			return String.Format(
				"Contents Pid: {0}\n" +
				"Settings Path: {1}\n",
				contents.pid, settings.path
			);
		}
	}


}
