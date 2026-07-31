using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wezit
{

    [Serializable]
    public class PoiLocation
    {
        public string pid;
        public float x;
        public float y;
        public float lng;
        public float lat;
        public float relX;
        public float relY;
        public float radius;
        public string tour_pid;
        public string language;
        public string map_name;
        public string map_pid;

        public WezitAssets.Asset wezitAsset;

        public void InitWezitAsset()
        {
            wezitAsset = AssetsLoader.GetAssetById(map_pid);
        }

        public string GetMapSourceByTransformation(string transformation)
        {
            if (wezitAsset == null)
            {
                return "";
            }

            string source = wezitAsset.GetAssetSourceByTransformation(transformation);

            if (transformation == WezitSourceTransformation.tilesZip && source.Contains("http"))
            {
                source = wezitAsset.GetAssetSourceByTransformation(WezitSourceTransformation.tiles);
            }

            return source;
        }
    }

}
