﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UsernameController : MonoBehaviour
{
    public InputField unText;
    public Button submit;
    private DatabaseAccess databaseAccess;

    // Start is called before the first frame update
    void Start()
    {
        submit.onClick.AddListener(LoginUser);
    }

    void LoginUser()
    {
        PlayerPrefs.SetString("name", unText.text);
        SceneManager.LoadScene("Main");
    }
}
