using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Forces the iOS audio session to the Playback category so audio plays at full
/// volume through the speaker and ignores the physical silent/mute switch, without
/// requiring microphone access. No-op on every platform other than a device iOS build.
/// </summary>
public class IOSAudioSession : MonoBehaviour
{
    #region Fields
    #region SerializeFields

    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    private void Start()
    {
        ConfigureAudioSession();
    }

    private void OnApplicationPause(bool isPaused)
    {
        // Interruptions (calls, Siri, the AR camera, video playback) can reset the
        // session category, so re-apply it whenever the app comes back to the foreground.
        if (!isPaused)
        {
            ConfigureAudioSession();
        }
    }
    #endregion
    #region Public

    #endregion
    #region Private
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void _ConfigureAudioSessionPlayback();
#endif

    private void ConfigureAudioSession()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _ConfigureAudioSessionPlayback();
#endif
    }
    #endregion
    #endregion
}
