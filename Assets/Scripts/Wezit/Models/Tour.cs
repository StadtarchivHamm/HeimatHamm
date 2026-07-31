using System;

namespace Wezit
{

	[Serializable]
	public class Tour : Node
	{
		public Wezit.Poi FindPoiInTour(string poiPid)
		{
			return FindPoiInChildren(this, poiPid) as Wezit.Poi;
		}

		private Wezit.Node FindPoiInChildren(Wezit.Node parentNode, string poiPid)
		{
			if (parentNode.children != null)
			{
				foreach (Wezit.Node childPoi in parentNode.children)
				{
					if (childPoi.pid == poiPid)
					{
						return childPoi;
					}
					else
					{
						Wezit.Node poi = FindPoiInChildren(childPoi, poiPid);
						if (poi != null)
						{
							return poi;
						}
					}
				}
			}

			return null;
		}
	}

}
