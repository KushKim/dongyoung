using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class startScene : MonoBehaviour
{
    
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("gamescene");
        Debug.Log("시작띠");
    }

   
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("종료띠");
    }
}
