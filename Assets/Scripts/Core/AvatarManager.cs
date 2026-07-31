using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Wezit;

public class AvatarManager : MonoBehaviour
{
    public enum AvatarType
    {
        None,
        Toni,
        Grete,
        Klippi
    }

    #region Fields
    #region SerializeFields
    [SerializeField] private Transform _avatarRoot;
    [SerializeField] private ARAvatar _toni;
    [SerializeField] private ARAvatar _grete;
    [SerializeField] private ARAvatar _klippi;
    #endregion

    #region Private
    private ARAvatar m_currentAvatar;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion

    #region Public
    public void SelectAvatar(AvatarType avatarType)
    {
        m_currentAvatar = avatarType == AvatarType.Toni ? _toni 
                        : avatarType == AvatarType.Grete ? _grete
                        : _klippi;

        _toni?.gameObject.SetActive(avatarType == AvatarType.Toni);
        _grete?.gameObject.SetActive(avatarType == AvatarType.Grete);
        _klippi?.gameObject.SetActive(avatarType == AvatarType.Klippi);
    }

    public void PlaceAvatar(Vector3 position, float rotationToNorth, Vector3 scale, Camera arCamera, bool lookAtUser)
    {
        _avatarRoot.localPosition = position;
        _avatarRoot.localEulerAngles = rotationToNorth * Vector3.up;

        if (lookAtUser)
        {
            _avatarRoot.LookAt(arCamera.transform);
            _avatarRoot.localEulerAngles = (_avatarRoot.localEulerAngles.y - 90) * Vector3.up;
        }
        _avatarRoot.localScale = scale;
        m_currentAvatar.Animator.SetLayerWeight(1, 0);
    }

    public void StartAvatarAnimation(string animationTag)
    {
        m_currentAvatar.Animator.SetTrigger(animationTag);
    }

    public void ToggleTalking(bool shouldTalk)
    {
        m_currentAvatar.Animator.SetLayerWeight(1, shouldTalk ? 1 : 0);
    }
    #endregion

    #region Private
    #endregion
    #endregion
}
