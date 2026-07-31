using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UniRx.Async;

namespace Wezit
{
	[Serializable]
	public class Node : Base
	{
		public string aspects;

		[NonSerialized]
		public List<Poi> children;

		[NonSerialized] private bool m_relationsSet = false;
		[NonSerialized] private bool m_initializingRelations;
		[NonSerialized] private UniTask<List<Relation>> m_relationListGetter;


        private List<Relation> relations;
		private List<Relation> showPictureRelations;
		private List<Relation> refPictureRelations;
		private List<Relation> threeSixtyPictureRelations;
		private List<Relation> audioRelations;
		private List<Relation> ambiantAudioRelations;
		private List<Relation> videoRelations;
		private List<Relation> threeSixtyVideoRelations;
		private List<Relation> threeDRelations;
		private List<Relation> activityRelations;
		private List<Relation> documentRelations;
		private List<Relation> subtitlesRelations;
		private List<Relation> otherRelations;

		public List<Relation> Relations
		{
			get
			{
				if (!m_relationsSet)
				{
					UnityEngine.Debug.LogWarning("[Node] - relations are not setted. Call GetRelations() function");
				}

				return relations;
			}
			set
			{
				relations = value;
			}
		}

		public List<Relation> ShowPictureRelations { get => showPictureRelations; }
		public List<Relation> RefPictureRelations { get => refPictureRelations; }
		public List<Relation> ThreeSixtyPictureRelations { get => threeSixtyPictureRelations; }
		public List<Relation> AudioRelations { get => audioRelations; }
		public List<Relation> AmbiantAudioRelations { get => ambiantAudioRelations; }
		public List<Relation> VideoRelations { get => videoRelations; }
		public List<Relation> ThreeSixtyVideoRelations { get => threeSixtyVideoRelations; }
		public List<Relation> ThreeDRelations { get => threeDRelations; }
		public List<Relation> ActivityRelations { get => activityRelations; }
		public List<Relation> DocumentRelations { get => documentRelations; }
		public List<Relation> SubtitlesRelations { get => subtitlesRelations; }
		public List<Relation> OtherRelations { get => otherRelations; }

		public async UniTask<List<Relation>> GetRelations()
		{
			if (!m_relationsSet)
			{
                await InitRelations();
			}

			return relations;
		}

		public async UniTask<bool> AreRelationsSet()
		{
			if (!m_relationsSet)
            {
                await InitRelations();

			}

			return m_relationsSet;
		}

		public async UniTask<bool> HasRelationOfType(string relationName)
		{
			bool hasRelation = false;

			if (!m_relationsSet)
            {
                await InitRelations();
			}

			foreach (Relation relation in relations)
			{
				if (relation.relation == relationName)
				{
					hasRelation = true;
					break;
				}
			}

			return hasRelation;
		}

		private async UniTask InitRelations()
		{
			if (m_initializingRelations)
			{
				await m_relationListGetter;
				return;
			}

			m_initializingRelations = true;
			m_relationListGetter = Initializer.GetAssetList("poi", this);

            relations = await m_relationListGetter;

			foreach (Relation relation in relations)
			{
				// Create sub-lists to accelerate asset browsing later
				switch (relation.relation)
				{
					case RelationName.SHOW_PICTURE:
						if (showPictureRelations == null)
						{
							showPictureRelations = new List<Relation>();
						}
						showPictureRelations.Add(relation);
						break;
					case RelationName.REF_PICTURE:
						if (refPictureRelations == null)
						{
							refPictureRelations = new List<Relation>();
						}
						refPictureRelations.Add(relation);
						break;
					case RelationName.SHOW_360_PICTURE:
						if (threeSixtyPictureRelations == null)
						{
							threeSixtyPictureRelations = new List<Relation>();
						}
						threeSixtyPictureRelations.Add(relation);
						break;
					case RelationName.PLAY_TRACK:
						if (audioRelations == null)
						{
							audioRelations = new List<Relation>();
						}
						audioRelations.Add(relation);
						break;
					case RelationName.PLAY_AMBIANT:
						if (ambiantAudioRelations == null)
						{
							ambiantAudioRelations = new List<Relation>();
						}
                        ambiantAudioRelations.Add(relation);
						break;
					case RelationName.PLAY_VIDEO:
						if (videoRelations == null)
						{
							videoRelations = new List<Relation>();
						}
						videoRelations.Add(relation);
						break;
					case RelationName.PLAY_360_VIDEO:
						if (threeSixtyVideoRelations == null)
						{
							threeSixtyVideoRelations = new List<Relation>();
						}
						threeSixtyVideoRelations.Add(relation);
						break;
					case RelationName.SHOW_3D_MODEL:
						if (threeDRelations == null)
						{
							threeDRelations = new List<Relation>();
						}
						threeDRelations.Add(relation);
						break;
					case RelationName.HAS_ACTIVITY:
						if (activityRelations == null)
						{
							activityRelations = new List<Relation>();
						}
						activityRelations.Add(relation);
						break;
					case RelationName.HAS_DOCUMENT:
						if (documentRelations == null)
						{
							documentRelations = new List<Relation>();
						}
						documentRelations.Add(relation);
						break;
					case RelationName.SET_SUBTITLE:
						if (subtitlesRelations == null)
						{
							subtitlesRelations = new List<Relation>();
						}
						subtitlesRelations.Add(relation);
						break;
					default:
						if (otherRelations == null)
						{
							otherRelations = new List<Relation>();
						}
						otherRelations.Add(relation);
						break;
				}

				relation.InitWezitAsset();
			}

			m_relationsSet = true;
            m_initializingRelations = false;
        }

		public Relation GetRelation(string relationName)
		{
			return relations.Find((relation) => relation.relation == relationName);
		}

		public List<Relation> GetRelationList(string relationName)
		{
			return relations.Where((relation) => relation.relation == relationName).ToList();
		}

		public Relation GetRelationByUsage(string usage)
		{
			return relations.Find((relation) => relation.usage == usage);
		}
	}
}