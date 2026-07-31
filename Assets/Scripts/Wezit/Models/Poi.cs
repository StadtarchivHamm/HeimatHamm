using System;
using System.Collections.Generic;
using System.Linq;

namespace Wezit
{
	[Serializable]
	public class PoiRelation
	{
		public string pid;
		public string relationName;
		public string relation;
		public int order;

		public bool IsAsset()
		{
			return IsAsset(relationName);
		}

		private bool IsAsset(string aRelationName)
		{
			return aRelationName == RelationName.SHOW_PICTURE
				|| aRelationName == RelationName.REF_PICTURE
				|| aRelationName == RelationName.PLAY_TRACK
				|| aRelationName == RelationName.PLAY_VIDEO
				|| aRelationName == RelationName.ZIP_FILE;
		}
	}

	[Serializable]
	public class Poi : Node
	{
		public List<PoiRelation> relationList;

		public void SetChildren()
		{
            if (children != null && children.Count > 0)
            {
                return;
            }
            else
            {
                if (relationList != null && relationList.Count > 0)
                {
                    relationList = relationList.OrderBy(relation => relation.order).ToList();

                    List<Wezit.Poi> childPois = new List<Wezit.Poi>();
                    foreach (Wezit.PoiRelation childRelation in relationList)
                    {
                        if (childRelation.relationName == Wezit.RelationName.HAS_NODE)
                        {
                            childPois.Add(PoiStore.GetPoiById(childRelation.pid));
                        }
                    }
                    children = childPois;
                    return;
                }
            }
			return;
        }

		public List<Poi> GetChildren()
		{
			SetChildren();
			return children;
		}
	}

}
