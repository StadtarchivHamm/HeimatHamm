using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimePortal : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private CollisionEvents _portalInsideCollider;
    [SerializeField] private CollisionEvents _portalOutsideCollider;

    [SerializeField] private PostProcessManager _postProcessManager;
    #endregion

    #region Private
    private bool m_isInsideInside;
    private bool m_isInsideOutside;
    private bool m_firstEntry = true;
    #endregion
    #endregion

    #region Properties
    public UnityEvent UserEnteredPast = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    private void Awake()
    {
        _portalInsideCollider.TriggerExit.AddListener(OnPortalInsideExit);
    }
    #endregion
    #region Public
    public void PlacePortal(Vector3 position, float RotationToNorth)
    {
        transform.localPosition = position;
        transform.localEulerAngles = RotationToNorth * Vector3.up;
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        StartCoroutine(AppearCoroutine());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    #endregion
    #region Private

    private void OnPortalInsideExit(Collider hit)
    {
        if (hit.CompareTag("Player") && m_firstEntry)
        {
            m_isInsideInside = false;
            m_firstEntry = false;
            _portalOutsideCollider.ToggleCollider(false);

            UserEnteredPast?.Invoke();
            _postProcessManager.TogglePostProcess(true);
        }
    }

    private IEnumerator AppearCoroutine()
    {
        float timer = 0;
        float duration = 2;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / duration);

            yield return null;
        }
    }
    #endregion
    #endregion
}
