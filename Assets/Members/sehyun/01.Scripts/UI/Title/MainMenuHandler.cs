using DG.Tweening;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup playCanvasGroup;
    [SerializeField] private CanvasGroup settingCanvasGroup;
    [SerializeField] private CanvasGroup quitCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    public void OnClickPlay()
    {
        ShowOnly(playCanvasGroup);
    }

    public void OnClickSetting()
    {
        ShowOnly(settingCanvasGroup);
    }

    public void OnClickQuit()
    {
        ShowOnly(quitCanvasGroup);
    }

    private void ShowOnly(CanvasGroup target)
    {
        Fade(mainMenuCanvasGroup, false);

        Fade(playCanvasGroup, target == playCanvasGroup);
        Fade(settingCanvasGroup, target == settingCanvasGroup);
        Fade(quitCanvasGroup, target == quitCanvasGroup);
    }

    private void Fade(CanvasGroup canvasGroup, bool isOn)
    {
        canvasGroup.DOFade(isOn ? 1f : 0f, fadeDuration);
        canvasGroup.interactable = isOn;
        canvasGroup.blocksRaycasts = isOn;
    }
}
