using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UsernameController : MonoBehaviour
{
    public InputField unText;
    public Button submit;

    // Start is called before the first frame update
    void Start()
    {
        submit.onClick.AddListener(UsernameUp);
    }

    void UsernameUp()
    {
        PlayerPrefs.SetString("name", unText.text);
        SceneManager.LoadScene("Main");
    }
}
