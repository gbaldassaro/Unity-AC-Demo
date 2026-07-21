using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathMenu : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    private CanvasGroup canvasGroup;
    private ColorAdjustments colorAdjustments;

    public float fadeInOutTime;
    private float elapsedTime;

    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        StartCoroutine(ShowDeathMenu());
    }

    private IEnumerator ShowDeathMenu()
    {
        gameObject.SetActive(true);

        globalVolume.profile.TryGet(out colorAdjustments);
        elapsedTime = 0;
        while (elapsedTime < fadeInOutTime)
        {
            canvasGroup.alpha = elapsedTime/fadeInOutTime;
            if (colorAdjustments != null) colorAdjustments.saturation.value = -100 * (elapsedTime/fadeInOutTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0;
        CursorHider.singleton.Show();
    }

    public void Hide()
    {
        StartCoroutine(HideDeathMenu());
    }

    public IEnumerator HideDeathMenu()
    {
        CursorHider.singleton.Hide();
        globalVolume.profile.TryGet(out colorAdjustments);
        elapsedTime = 0;
        while (elapsedTime < fadeInOutTime)
        {
            canvasGroup.alpha = 1 - elapsedTime/fadeInOutTime;
            if (colorAdjustments != null) colorAdjustments.saturation.value = -100 - (-100 * (elapsedTime/fadeInOutTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0;

        gameObject.SetActive(false);
    }

    public void Retry()
    {
        Hide();
        Debug.Log("Retry!");
    }

    public void MainMenu()
    {
        Debug.Log("Main Menu!");
    }
}
