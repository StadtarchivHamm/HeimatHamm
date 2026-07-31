using UnityEngine;
using UnityEngine.UI;

public class DownloadView : BaseView
{
    #region Fields
    #region SerializeFields
    [Header("Download")]
    [SerializeField] private GameObject _downloadingRoot;
    [SerializeField] private Image _progressBar;
    [SerializeField] private CanvasGroup _logo;
    [SerializeField] private Button _cancelButton;
    [Header("Completed")]
    [SerializeField] private GameObject _completedRoot;
    [SerializeField] private Button _continueButton;
    #endregion
    #region Private
    private int m_downloadSize;
    private string m_tourPid;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    protected override void InitViewContentByLang(Language language)
    {
        base.InitViewContentByLang(language);

        m_tourPid = PlayerManager.CurrentState.CurrentTour.pid;
        m_downloadSize = PlayerManager.CurrentState.tourDownloadSize;

        StartDownloading();
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

        _downloadingRoot.SetActive(true);
        _completedRoot.SetActive(false);
        _progressBar.fillAmount = 0;
        _logo.alpha = 0;
    }
    #endregion
    #region Private
    protected override void AddListeners()
    {
        base.AddListeners();

        Wezit.DataGrabber.Instance.DownloadProgress.AddListener(UpdateProgress);
        Wezit.DataGrabber.Instance.DownloadOver.AddListener(OnDownloadOver);

        _cancelButton.onClick.AddListener(OnCancelButton);
        _continueButton.onClick.AddListener(OnContinueButton);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        Wezit.DataGrabber.Instance.DownloadProgress.RemoveAllListeners();
        Wezit.DataGrabber.Instance.DownloadOver.RemoveListener(OnDownloadOver);

        _cancelButton.onClick.RemoveAllListeners();
        _continueButton.onClick.RemoveAllListeners();
    }

    private void StartDownloading()
    {
        Wezit.DataGrabber.Instance.GetAssetsForTour(m_tourPid, "default");
    }

    private void UpdateProgress(int progress, string assetName)
    {
        _progressBar.fillAmount = _logo.alpha = progress / (float)m_downloadSize;
    }

    private void OnDownloadOver()
    {
        _progressBar.fillAmount = _logo.alpha = 1;
        PlayerManager.Player.GetTourProgression(m_tourPid).HasBeenDownloaded = true;
        PlayerManager.Player.Save();

        _downloadingRoot.SetActive(false);
        _completedRoot.SetActive(true);
    }

    private void OnContinueButton()
    {
        MatomoAnalyticsManager.Instance.RecordTourStarted(PlayerManager.CurrentState.CurrentTour.CleanedTitle, PlayerManager.CurrentState.CurrentTour.pid);
        SetState(PlayerManager.CurrentState.IsAudioDescription ? KioskState.LIST : KioskState.MAP);
    }

    private void OnCancelButton()
    {
        Wezit.DataGrabber.Instance.AbortDownload();
        SetState(KioskState.TOUR_INTRO);
    }
    #endregion
    #endregion
}
