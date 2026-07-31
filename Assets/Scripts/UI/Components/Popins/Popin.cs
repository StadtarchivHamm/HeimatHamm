using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Popin : MonoBehaviour
{
    #region Fields
    #region Serialize Fields
    [SerializeField] internal Transform _panelRoot;
    [SerializeField] internal Button _darkenBackground;
    [SerializeField] internal Button _closeButton;
    [SerializeField] internal RawImage _topIcon;
    [SerializeField] internal TMPro.TextMeshProUGUI _title;
    [SerializeField] internal TMPro.TextMeshProUGUI _description;
    [SerializeField] internal Button _popinButton;
    [SerializeField] internal TMPro.TextMeshProUGUI _popinButtonText;
    [SerializeField] internal Button _popinSecondaryButton;
    #endregion
    #region Private
    #endregion
    #endregion

    #region Properties
    public UnityEvent PopinButtonClicked = new UnityEvent();
    public UnityEvent PopinSecondaryButtonClicked = new UnityEvent();
    public UnityEvent PopinClosed = new UnityEvent();

    public RawImage PopinTopIcon { get => _topIcon; }
    #endregion

    #region Methods
    #region Public
    public void Inflate(bool open, bool animateOpening = false, bool setButtonToClose = false)
    {
        if(open)
        {
            Open(animateOpening);
        }

        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(OnCloseButton);

        _darkenBackground.onClick.RemoveAllListeners();
        _darkenBackground.onClick.AddListener(OnCloseButton);

        if (_popinButton != null)
        {
            _popinButton.onClick.RemoveAllListeners();

            if (setButtonToClose)
            {
                _popinButton.onClick.AddListener(OnCloseButton);
            }
            else
            {
                _popinButton.onClick.AddListener(OnPopinButton);
            }
        }

        if (_popinSecondaryButton != null)
        {
            _popinSecondaryButton.onClick.RemoveAllListeners();

            if (setButtonToClose)
            {
                _popinSecondaryButton.onClick.AddListener(OnCloseButton);
            }
            else
            {
                _popinSecondaryButton.onClick.AddListener(OnPopinSecondaryButton);
            }
        }

    }

    public void Inflate(bool open, string title, string description, string buttonText, bool animateOpening = false)
    {
        Inflate(open, animateOpening);

        if(_title != null && !string.IsNullOrEmpty(title))
        {
            _title.text = title;
            _title.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }

        if(_description != null && !string.IsNullOrEmpty(description))
        {
            _description.text = description;
            _description.gameObject.SetActive(!string.IsNullOrEmpty(description));
        }

        if(_popinButtonText != null && !string.IsNullOrEmpty(buttonText))
        {
            _popinButtonText.text = buttonText;
        }
    }

    public void Inflate(string description)
    {
        Inflate(true, "", description, "", true);
    }

    public void Close(bool invokeEvent = true)
    {
        if(invokeEvent)
        {
            PopinClosed?.Invoke();
        }
        gameObject.SetActive(false);

        if(MenuManager.Instance.CurrentStatus == MenuManager.MenuStatus.Darken)
        {
            MenuManager.Instance.SetMenuStatus(MenuManager.Instance.CurrentViewMenuStatus);
        }
    }

    public void Close()
    {
        Close(true);
    }

    public void Open(bool animateOpening = false)
    {
        gameObject.SetActive(true);

        if (_darkenBackground.gameObject.activeInHierarchy)
        {
            MenuManager.Instance.SetMenuStatus(MenuManager.MenuStatus.Darken);
        }

        if (animateOpening)
        {
            StartCoroutine(OpenCoroutine());
        }
    }
    #endregion

    #region Private
    private IEnumerator OpenCoroutine()
    {
        float timer = 0;
        float duration = 0.2f;
        Vector3 scale = Vector3.one;
        scale.y = 0;
        _panelRoot.localScale = scale;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            scale.y = Mathf.Lerp(0, 1, timer / duration);
            _panelRoot.localScale = scale;

            yield return null;
        }

        _panelRoot.localScale = Vector3.one;
    }

    internal virtual void OnPopinButton()
    {
        PopinButtonClicked?.Invoke();
    }

    internal virtual void OnPopinSecondaryButton()
    {
        PopinSecondaryButtonClicked?.Invoke();
    }

    private void OnCloseButton()
    {
        Close(true);
    }
    #endregion
    #endregion
}
