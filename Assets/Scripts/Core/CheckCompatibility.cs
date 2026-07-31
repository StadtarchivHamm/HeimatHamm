using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx.Async;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public static class CheckCompatibility
{
    #region Fields
    #region Private
    #endregion

    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Public
    public static async UniTask<bool> IsCompatible(MonoBehaviour monoBehaviour)
    {
        if (Application.isEditor)
        {
            return false; // no real AR in-editor; predictable for testing
        }

        if (SystemInfo.systemMemorySize <= 2048)
        {
            return false; // don't try to run AR on low-end devices
        }

        return await CheckARCompatibility(monoBehaviour);
    }
    #endregion

    #region Private
    private static async UniTask<bool> CheckARCompatibility(MonoBehaviour monoBehaviour)
    {
        while ((ARSession.state == ARSessionState.None) || (ARSession.state == ARSessionState.CheckingAvailability))
        {
            await monoBehaviour.StartCoroutine(ARSession.CheckAvailability());
        }

        bool isARCompatible = !(ARSession.state == ARSessionState.Unsupported);

        PlayerManager.Player.IsARCompatible = isARCompatible;
        PlayerManager.Player.Save();

        return isARCompatible;
    }
    #endregion
    #endregion
}
