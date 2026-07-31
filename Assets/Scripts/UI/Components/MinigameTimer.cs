using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameTimer : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _bestTime;
    #endregion
    #region Private
    private float m_timer;
    private bool m_countTime;
    #endregion
    #endregion

    #region Properties
    public int Time { get => Mathf.CeilToInt(m_timer); }
    #endregion

    #region Methods
    #region Monobehaviours
    private void OnDisable()
    {
        ToggleTimer(false);
    }
    #endregion

    #region Public
    public void Inflate(int bestTime)
    {
        int bestMinutes = bestTime / 60;
        int bestSeconds = bestTime - bestMinutes * 60;

        m_timer = 0;
        _time.text = "00:00";
        _bestTime.text = bestTime == 20000 ? "--:--" : string.Format("{0}:{1}", bestMinutes.ToString("00"), bestSeconds.ToString("00"));
    }

    public void ToggleTimer(bool isOn)
    {
        m_countTime = isOn;
        if (isOn)
        {
            StartCoroutine(TimerCoroutine());
        }
    }
    #endregion
    #region Private
    private IEnumerator TimerCoroutine()
    {
        int minutes = 0;
        int seconds = 0;
        m_countTime = true;

        while (m_countTime) 
        {
            yield return new WaitForSeconds(1);
            
            m_timer ++;
            seconds++;

            if (seconds == 60)
            {
                minutes++;
                seconds = 0;
            }

            _time.text = string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00"));
        }
    }
    #endregion
    #endregion
}
