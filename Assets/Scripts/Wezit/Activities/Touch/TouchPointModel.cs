using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TouchItemModel
{
    public Color color;
    public TouchPointModel point;
    public bool responseStatus;
    public bool responseValidationStatus;
    public string responseImage;
    public string responseValidationTitle;
    public string responseValidationDescription;

    public TouchItemModel(string color, string pointJson, bool responseStatus, bool responseValidationStatus, string responseImage, string responseValidationTitle, string responseValidationDescription)
    {
        this.color = StringUtils.GetStringAsColor(color);
        this.point = JsonUtility.FromJson<TouchPointModel>(pointJson);
        this.responseStatus = responseStatus;
        this.responseValidationStatus = responseValidationStatus;
        this.responseImage = responseImage;
        this.responseValidationTitle = responseValidationTitle;
        this.responseValidationDescription = responseValidationDescription;
    }
}

[Serializable]
public class TouchPointModel
{
    public float circle;
    public TouchPointMap map;
    public float x;
    public float y;
    public float relX;
    public float relY;
}

[Serializable]
public class TouchPointMap
{
    public string pid;
}
