﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    public void HomeSceneloader()
    {
        SceneManager.LoadScene("Home");
    }

    public void ARSceneloader()
    {

        SceneManager.LoadScene("V5.1");
    }

    public void AboutSceneLoader()
    {

        SceneManager.LoadScene("About page");
    }
    public void QuitApp()
    {
        Application.Quit();

    }
}
