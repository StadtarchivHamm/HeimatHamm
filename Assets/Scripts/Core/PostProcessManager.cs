using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] private Volume _volume;
    private Coroutine m_coroutine;
    private string m_maxWeightSettingKey = "ar.past.posteffect.potency.value";

    private void OnEnable()
    {
        _volume.weight = 0f;
    }

    public void TogglePostProcess(bool isOn)
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            _volume.weight = isOn ? 0 : 1;
        }
        m_coroutine = StartCoroutine(FadePostProcess(isOn));
    }

    private IEnumerator FadePostProcess(bool isOn)
    {
        float timer = 0;
        float duration = 1;
        float start = isOn ? 0 : Wezit.Settings.GetSettingAsFloat(m_maxWeightSettingKey);
        float goal = isOn ? Wezit.Settings.GetSettingAsFloat(m_maxWeightSettingKey) : 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            _volume.weight = Mathf.Lerp(start, goal, timer / duration);
            yield return null;
        }
        _volume.weight = goal;
    }
}
