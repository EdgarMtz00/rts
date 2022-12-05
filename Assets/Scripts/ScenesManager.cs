using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public static void LoadWin() {
        SceneManager.LoadScene("Win");
    }
    
    public static void LoadLose() {
        SceneManager.LoadScene("Lose");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
