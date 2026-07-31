using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Wezit;

public static class PlayerManager
{
    public static PlayerData Player;
    public static CurrentStateData CurrentState;

    public static string PlayerDataPath
    {
        get
        {
            return Application.persistentDataPath;
        }
    }

    public static void Init()
    {
        Player = new PlayerData();
        Player.Load();
        CurrentState = new CurrentStateData();
    }
}
