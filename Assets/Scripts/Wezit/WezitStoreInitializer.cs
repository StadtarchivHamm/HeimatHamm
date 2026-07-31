using UnityEngine;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using System.IO;

namespace Wezit
{
    public class StoreInitializer
    {
        private static Wezit.SqlManager sqlManager = new Wezit.SqlManager();

        private static string JSON_PATH = Path.Combine(DataGrabber.AssetsFolderPath, "StoreJsons");

        public static async UniTask<bool> Init()
        {
            Debug.Log("[Wezit Data] - Init");
            if (FilesDownloader.SqliteUpdated || !FilesChecker() || true)
            {
                //MAIN THREAD
                string inventoryListResult = await sqlManager.GetInventoryList();
                string tourListResult = await sqlManager.GetTourList();
                string poiListResult = await sqlManager.GetPoiList();
                string locationListResult = await sqlManager.GetLocationList();

                List<Poi> poiList = GetDataFromSqlList<Poi>("poiListData.json", poiListResult).data;
                StoreAccessor.Dispatch(Store.PoiList.ActionCreator.Set(poiList));
                StoreAccessor.Dispatch(Store.InventoryList.ActionCreator.Set(GetDataFromSql<Inventory>("inventoryListData.json", inventoryListResult).data));

                List<Tour> tourList = await Initializer.InitTourList(GetDataFromSqlList<Tour>("tourListData.json", tourListResult).data, poiList);
                StoreAccessor.Dispatch(Store.TourList.ActionCreator.Set(tourList));

                StoreAccessor.Dispatch(Store.LocationList.ActionCreator.Set(GetDataFromSql<PoiLocation>("locationListData.json", locationListResult).data));

                //SECONDARY THREAD

                //Observable.WhenAll(sqlManager.GetInventoryList(), sqlManager.GetTourList(), sqlManager.GetPoiList(), sqlManager.GetLocationList()).ObserveOnMainThread().Subscribe(async xs =>
                //{
                string threeDPositionsListResult = await sqlManager.Get3DPositions();
                string categoryListResult = await sqlManager.GetCategoryList();
                string coverListResult = await sqlManager.GetCovers();

                StoreAccessor.Dispatch(Store.ThreeDPositionsList.ActionCreator.Set((GetDataFromSql<ThreeDPosition>("threeDPositionsListData.json", threeDPositionsListResult)).data));

                StoreAccessor.Dispatch(Store.CategoryList.ActionCreator.Set((GetDataFromSql<Category>("categoryListData.json", categoryListResult)).data));

                StoreAccessor.Dispatch(Store.CoverList.ActionCreator.Set((GetDataFromSql<Cover>("coverListData.json", coverListResult)).data));
                //});

            }
            else
            {
                List<Poi> poiList = (await GetDataFromJsonList<Poi>("poiListData.json")).data;
                StoreAccessor.Dispatch(Store.PoiList.ActionCreator.Set(poiList));

                StoreAccessor.Dispatch(Store.InventoryList.ActionCreator.Set((await GetDataFromJson<Inventory>("inventoryListData.json")).data));

                List<Tour> tourList = await Initializer.InitTourList((await GetDataFromJsonList<Tour>("tourListData.json")).data, poiList);
                StoreAccessor.Dispatch(Store.TourList.ActionCreator.Set(tourList));

                StoreAccessor.Dispatch(Store.LocationList.ActionCreator.Set((await GetDataFromJson<PoiLocation>("locationListData.json")).data));

                StoreAccessor.Dispatch(Store.ThreeDPositionsList.ActionCreator.Set((await GetDataFromJson<ThreeDPosition>("threeDPositionsListData.json")).data));

                StoreAccessor.Dispatch(Store.CategoryList.ActionCreator.Set((await GetDataFromJson<Category>("categoryListData.json")).data));

                StoreAccessor.Dispatch(Store.CoverList.ActionCreator.Set((await GetDataFromJson<Cover>("coverListData.json")).data));
            }
            return true;
        }

        public static async UniTask<APIResponse<List<T>>> GetDataFromJson<T>(string fileName)
        {

            if (!Directory.Exists(JSON_PATH))
            {
                Directory.CreateDirectory(JSON_PATH);
            }
            string listJsonPath = Path.Combine(JSON_PATH, fileName);
            string listResult = await Utils.FileUtils.RequestTextContent(listJsonPath, 5);
            APIResponse<List<T>> list = JsonUtility.FromJson<APIResponse<List<T>>>(listResult);
            //StoreAccessor.Dispatch(Store.CoverList.ActionCreator.Set(list.data));
            return list;
        }

        public static async UniTask<APIResponse<List<T>>> GetDataFromJsonList<T>(string fileName)
        {

            if (!Directory.Exists(JSON_PATH))
            {
                Directory.CreateDirectory(JSON_PATH);
            }
            string listJsonPath = Path.Combine(JSON_PATH, fileName);
            string listResult = await Utils.FileUtils.RequestTextContent(listJsonPath, 5);
            APIResponse<List<T>> list = JsonUtility.FromJson<APIResponse<List<T>>>(listResult);
            //List<T> poiList = await Initializer.InitPoiList(list.data);
            //StoreAccessor.Dispatch(Store.PoiList.ActionCreator.Set(poiList));
            return list;
        }

        public static APIResponse<List<T>> GetDataFromSql<T>(string fileName, string fileContent)
        {

            if (!Directory.Exists(JSON_PATH))
            {
                Directory.CreateDirectory(JSON_PATH);
            }
            File.WriteAllText(Path.Combine(JSON_PATH, fileName), fileContent);
            APIResponse<List<T>> list = JsonUtility.FromJson<APIResponse<List<T>>>(fileContent);
            //StoreAccessor.Dispatch(Store.InventoryList.ActionCreator.Set(list.data));
            return list;
        }

        public static APIResponse<List<T>> GetDataFromSqlList<T>(string fileName, string fileContent)
        {
            if (!Directory.Exists(JSON_PATH))
            {
                Directory.CreateDirectory(JSON_PATH);
            }
            File.WriteAllText(Path.Combine(JSON_PATH, fileName), fileContent);
            APIResponse<List<T>> list = JsonUtility.FromJson<APIResponse<List<T>>>(fileContent);
            //List<T> result = await Initializer.InitPoiList(list.data);
            //StoreAccessor.Dispatch(Store.PoiList.ActionCreator.Set(result));
            return list;
        }

        public static async UniTask<List<Poi>> GetPoiVersions(string id)
        {
            var result = await sqlManager.GetVersionByNode(id).ToUniTask();
            APIResponse<List<Poi>> response = JsonUtility.FromJson<APIResponse<List<Poi>>>(result);
            return response.data;
        }

        private static bool FilesChecker()
        {
            return File.Exists(Path.Combine(JSON_PATH, "categoryListData.json")) &&
                   File.Exists(Path.Combine(JSON_PATH, "coverListData.json")) &&
                   File.Exists(Path.Combine(JSON_PATH, "inventoryListData.json")) &&
                   File.Exists(Path.Combine(JSON_PATH, "locationListData.json")) &&
                   File.Exists(Path.Combine(JSON_PATH, "poiListData.json")) &&
                   File.Exists(Path.Combine(JSON_PATH, "threeDPositionsListData.json")) &&
                   File.Exists(Path.Combine(JSON_PATH, "tourListData.json"));
        }
    }

} // End namespace Wezit