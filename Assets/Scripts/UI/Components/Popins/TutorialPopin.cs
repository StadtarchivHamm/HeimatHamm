using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopin : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _goUpButton;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private Transform _scrollViewContent;
    [SerializeField] private GameObject[] _arSpecificContent;

    private void Awake()
    {
        if (_closeButton)
        {
            _closeButton.onClick.RemoveListener(ClosePopin);
            _closeButton.onClick.AddListener(ClosePopin);
        }

        if (_goUpButton)
        {
            _goUpButton.onClick.RemoveListener(OnGoUpButton);
            _goUpButton.onClick.AddListener(OnGoUpButton);
        }
    }

    public void TogglePopin(bool isOn)
    {
        gameObject.SetActive(isOn);

        foreach (GameObject arSpecificContent in _arSpecificContent)
        {
            arSpecificContent.SetActive(PlayerManager.Player.IsARCompatible);
        }
    }

    private void ClosePopin()
    {
        TogglePopin(false);
        PlayerManager.Player.HasSeenTutorial = true;
        PlayerManager.Player.Save();
    }

    private void OnGoUpButton()
    {
        StartCoroutine(ScrollBackUp());
    }

    private IEnumerator ScrollBackUp()
    {
        Vector3 start = _scrollViewContent.localPosition;
        Vector3 goal = new Vector3(_scrollViewContent.localPosition.x, 0, _scrollViewContent.localPosition.z);
        float timer = 0;
        float duration = 1;
        _scrollRect.inertia = false;

        while (timer < duration)
        {
            _scrollViewContent.localPosition = Vector3.Lerp(start, goal, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        
        _scrollViewContent.localPosition = goal;
        _scrollRect.inertia = true;
    }
}
