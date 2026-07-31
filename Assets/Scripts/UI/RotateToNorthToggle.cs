using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Toggle))]
public class RotateToNorthToggle : MonoBehaviour
{
    [SerializeField] private Transform _iconTransform;
    private Toggle m_toggle;
    private RotateToNorth m_rotateToNorth;
    private Coroutine m_rotationCoroutine;

    private void Awake()
    {
        m_toggle = GetComponent<Toggle>();
        //m_toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        m_toggle.onValueChanged.AddListener(OnToggleValueChanged);

        m_rotateToNorth = GetComponentInChildren<RotateToNorth>();
    }

    public void OnToggleValueChanged(bool isOn)
    {

        if (m_rotationCoroutine != null)
        {
            StopCoroutine(m_rotationCoroutine);
        }
        m_rotationCoroutine = StartCoroutine(RotateToGoal(isOn ? -Utils.MapUtils.RotationToNorth * Vector3.forward : Vector3.zero, isOn));
    }

    private IEnumerator RotateToGoal(Vector3 goal, bool enableRotateToNorth)
    {
        float timer = 0;
        float duration = 2;

        if (!enableRotateToNorth)
        {
            m_rotateToNorth.enabled = false;
        }

        while (_iconTransform.localEulerAngles != goal && timer < duration)
        {
            timer += Time.deltaTime;
            _iconTransform.localEulerAngles = Vector3.Lerp(_iconTransform.localEulerAngles, goal, Time.deltaTime * 2f);
            yield return null;
        }

        if (enableRotateToNorth)
        {
            m_rotateToNorth.enabled = true;
        }

        m_rotationCoroutine = null;
    }
}
