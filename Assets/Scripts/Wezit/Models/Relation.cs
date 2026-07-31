using System;
using System.Collections.Generic;

namespace Wezit
{
	class RelationName
	{
		public const string HAS_NODE = "hasNode";
		public const string SHOW_PICTURE = "relationForShowPicture";
		public const string REF_PICTURE = "relationForSetRefPicture";
		public const string PLAY_VIDEO = "relationForPlayVideo";
		public const string PLAY_TRACK = "relationForPlayTrack";
		public const string PLAY_AMBIANT = "relationForPlayAmbiant";
		public const string ZIP_FILE = "relationForSetDataInZip";
		public const string AUTHORED_BY = "authoredBy";
		public const string SHOW_360_PICTURE = "relationForShow360Picture";
		public const string PLAY_360_VIDEO = "relationForPlay360Video";
		public const string SHOW_3D_MODEL = "relationForShowModel3D";
		public const string HAS_ACTIVITY = "relationForActivity";
		public const string HAS_DOCUMENT = "relationForSetDocument";
		public const string SET_SUBTITLE = "relationForSetSubtitle";
	}

	[Serializable]
	public class Relation : Base
	{
		public string relation;
		public string usage;
		public int ord;

		public WezitAssets.Asset wezitAsset = null;

		public override string ToString()
		{
			return base.ToString() + String.Format(
				"Relation: {0}\n",
				relation
			);
		}

		public void InitWezitAsset()
		{
			wezitAsset = AssetsLoader.GetAssetById(pid);
		}

		public WezitAssets.File GetAssetByTransformation(string transformation)
		{
			if (wezitAsset == null)
			{
				return null;
			}

			WezitAssets.File file = wezitAsset.files.Find(file => file.label == transformation);

			if (file != null)
			{
				return file;
			}
			else
			{
				return wezitAsset.files.Find(file => file.label == "original");
			}
		}

		public string GetAssetSourceByTransformation(string transformation)
		{
			return wezitAsset.GetAssetSourceByTransformation(transformation);
		}

		public string GetAssetMimeTypeByTransformation(string transformation)
		{
			return wezitAsset.GetAssetMimeTypeByTransformation(transformation);
		}
	}
}
