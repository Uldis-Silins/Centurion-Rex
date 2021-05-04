using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    private void Awake()
    {
        Player_Controller.currentGameState = GameState.GameOver;
    }
}
