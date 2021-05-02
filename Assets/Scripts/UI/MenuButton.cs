using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField]
    AudioClip buttonClickSFX;
    [SerializeField]
    AudioManager audioManager;


    private void Start()
    {
       // Time.timeScale = 0;
        if (audioManager == null)
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public void PlayClickSound()
    {
        Debug.Log("click");
        audioManager.PlaySFX(buttonClickSFX);
    }

    public void StartExitGame()
    {
        Time.timeScale = 1;
        StartCoroutine(ExitGameAfterXSeconds(0.5f));
    }

    private IEnumerator ExitGameAfterXSeconds(float x)
    {
        yield return new WaitForSeconds(x);
        ExitGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadRomeLevel()
    {
        Debug.Log(Time.timeScale);
        Time.timeScale = 1;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
