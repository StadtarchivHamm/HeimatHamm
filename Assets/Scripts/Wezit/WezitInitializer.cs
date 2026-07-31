using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using System;

namespace Wezit
{
	public class Initializer
	{
        #region Fields
        private static SqlManager sqlManager = new SqlManager();
        #endregion

        #region Methods
        #region Public
		public static async UniTask<List<Tour>> InitTourList(List<Tour> datas, List<Poi> poiList)
		{
			foreach (Tour tour in datas)
			{
				tour.Relations = await GetAssetList("tour", tour);

				if (poiList != null && poiList.Count > 0)
				{
					tour.children = await GetTourChildren(tour, poiList);
				}
			};

			return datas;
		}

		public static async UniTask<List<Relation>> GetAssetList(string type, Node node)
		{
			var result = await sqlManager.GetAssetListByNodeId(type, node.pid).ToUniTask();
			APIResponse<List<Relation>> response = JsonUtility.FromJson<APIResponse<List<Relation>>>(result);
			return response.data;
        }

        public static bool SetPoiChildren(Poi poi)
        {
            if (poi.children == null || poi.children.Count == 0)
            {
                GetPoiChildren(poi, StoreAccessor.State.PoiList);
            }
            return poi.children.Count > 0;
        }

        public static List<Poi> GetPoiChildren(Poi poi)
        {
            return GetPoiChildren(poi, StoreAccessor.State.PoiList);
        }

        public static List<Poi> GetPoiChildren(Poi poi, List<Poi> poiList)
        {
            if (poi.children != null && poi.children.Count > 0)
            {
                return poi.children;
            }

#if !UNITY_WEBGL
            List<PoiRelation> childrenRelations = poi.relationList.Where(r => r.relationName == RelationName.HAS_NODE).ToList();
#else
			List<PoiRelation> childrenRelations = poi.relationList.Where(r => r.relationName == RelationName.HAS_NODE).ToList();
#endif
            List<Poi> children = new List<Poi>();
            foreach (PoiRelation relation in childrenRelations)
            {
                Poi childPoi = poiList.Find(n => n.pid == relation.pid);
                children.Add(childPoi); ;
            }
            poi.children = children;

            return children;
        }
        #endregion

        #region Private
        private static async UniTask<List<Poi>> GetTourChildren(Tour tour, List<Poi> poiList)
		{
			var result = await sqlManager.GetPoiListByTourId(tour.pid).ToUniTask();
			List<Poi> childList = JsonUtility.FromJson<APIResponse<List<Poi>>>(result).data;

            for (int i = 0; i < childList.Count; i++)
            {
				if (poiList.Count > 0)
				{
					Poi foundPoi = poiList.Find(p => p.pid == childList[i].pid);
					childList[i] = foundPoi;

                    childList[i].children = GetPoiChildren(childList[i], poiList);
					foundPoi.children = childList[i].children;
				}
            }

			return childList;
		}
        #endregion
        #endregion
    }

}