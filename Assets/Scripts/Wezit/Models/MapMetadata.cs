using System.Collections.Generic;
using UnityEngine;

namespace Wezit
{
    public class MapMetadata
    {
        public string bounds;
        public string center;
        public float minzoom;
        public float maxzoom;
        public string name;
        public string description;
        public string attribution;
        public string template;
        public string version;

        public override string ToString()
        {
            return string.Format(
                "bounds: {0}\n" +
                "center: {1}\n" +
                "minzoom: {2}\n" +
                "maxzoom: {3}\n" +
                "name: {4}\n" +
                "description: {5}\n" +
                "attribution: {6}\n" +
                "template: {7}\n" +
                "version: {8}\n",
                GetBounds().ToString("F4"),
                GetCenter().ToString("F4"),
                minzoom.ToString(),
                maxzoom.ToString(),
                name,
                description,
                attribution,
                template,
                version
                );
        }

        public Vector3 GetCenter()
        {
            return StringUtils.StringToVector3(center);
        }

        public Vector4 GetBounds()
        {
            string[] members = bounds.Split(',');
            float.TryParse(members[0],
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           new System.Globalization.CultureInfo("en-US"),
                           out float boundMinX);
            float.TryParse(members[1],
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           new System.Globalization.CultureInfo("en-US"),
                           out float boundMinY);
            float.TryParse(members[2],
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           new System.Globalization.CultureInfo("en-US"),
                           out float boundMaxX);
            float.TryParse(members[3],
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           new System.Globalization.CultureInfo("en-US"),
                           out float boundMaxY);
            return new Vector4(boundMinX, boundMinY, boundMaxX, boundMaxY);
        }
    }
}