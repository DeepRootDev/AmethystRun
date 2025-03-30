using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject finishRaceMenu;
     public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    private bool isPaused = false;

    void Start()
    {
         if (fadePanel != null)
        { 
            fadePanel.alpha = 1;
            StartCoroutine(FadeIn()); 
        }
    }
    void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    public void gotoMenu(){
        if (fadePanel != null)
        {
            StartCoroutine(FadeAndLoadScene());
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
     private IEnumerator FadeAndLoadScene()
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(0);
    }
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1; // Pause or resume game
    }

    public void FinishRace()
    {
        finishRaceMenu.SetActive(true);
        Time.timeScale = 0; // Stop game when race is finished
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
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
