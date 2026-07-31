using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using Wezit;

public class TourPoiLongLat
{
    public Poi Poi;
    public Vector2 LongLat;

    public TourPoiLongLat(Poi poi, Vector2 longLat)
    {
        Poi = poi;
        LongLat = longLat;
    }
}

public class CurrentStateData
{
    public Tour CurrentTour;
    public List<TourPoiLongLat> CurrentTourPoisLongLat = new List<TourPoiLongLat>();
    public Sprite CurrentCharacterSprite;
    public int tourDownloadSize;
    public Poi CurrentPoi;
    public Poi CurrentHiddenObjectPoi;
    public Poi CurrentStationLocationPoi;
    public AvatarManager.AvatarType CurrentAvatarType;
    public RuntimeReferenceImageLibrary RuntimeReferenceImageLibrary;

    public bool IsInDevMode;

    public bool IsFromOnMapButton;

    public bool IsAudioDescription;
    public bool IsEasyToRead;

    public bool ShowSecretPoiPopin;

    public bool IsGPSOn = true;
    public bool IsUserInTheArea;
    public Poi LastPOIInRange;
    public Vector2 LastKnownPosition = Vector2.zero;
    public Vector2 NorthPoiLocation;
    public bool NavigationIsOn;
    public Vector2 NavigationGoalPOIPosition;
}