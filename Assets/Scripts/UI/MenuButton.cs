using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField]
    AudioClip buttonClickSFX;
    AudioManager audioManager;

    private void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public void PlayClickSound()
    {
        Debug.Log("click");
        audioManager.PlaySFX(buttonClickSFX);
    }

    public void StartExitGame()
    {
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
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
