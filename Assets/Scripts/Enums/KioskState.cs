
public enum KioskState
{
    NONE = 0,
    SPLASH = 1,
    LANGUAGE_SELECTION = 2,
    HOME = 3,
    TOUR_INTRO = 4,
    DOWNLOAD = 5,
    MAP = 6,
    LIST = 7,
    AR = 8,
    POI_DETAILS = 9,

    #region Minigames
    MINIGAME_AR = 10,
    MINIGAME_DRAGDROP = 11,
    MINIGAME_SLIDING_PUZZLE = 12,
    MINIGAME_DIAPORAMA = 13,
    MINIGAME_TOUCH = 14,
    MINIGAME_QUIZ = 15,
    MINIGAME_MUSIC = 16,
    #endregion

    #region MenuLinks
    ACCESSIBILITY = 50,
    INVENTORY = 51,
    HIDDEN_OBJECT = 60,
    TUTORIAL = 52,
    FAQ = 53,
    RESET = 54,
    DATA_PROTECTION = 55,
    LEGAL_NOTICE = 56,
    TERMS_OF_USE = 57,
    CONTACT = 58,
    CREDITS = 59,
    #endregion

    SECRET_POI = 70,
}
