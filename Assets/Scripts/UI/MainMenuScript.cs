using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuScript : MonoBehaviour
{

    public CanvasGroup fadePanel;  // Optional: Assign a UI Panel with CanvasGroup for fade effect
    public float fadeDuration = 1f;

    private void Start()
    {
        if (fadePanel != null)
        { 
            fadePanel.alpha = 1;
            StartCoroutine(FadeIn()); 
        }
    }

    public void PlayGame()
    {
        if (fadePanel != null)
        {
            StartCoroutine(FadeAndLoadScene());
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Allows quitting in the editor
#endif
    }

    private IEnumerator FadeAndLoadScene()
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(1);
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadePanel.alpha = 1;
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadePanel.alpha = 0;
    }
}
