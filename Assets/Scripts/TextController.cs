using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine;

public class TextController : NetworkBehaviour
{
    // Public variables
    public Text scoreText;
    public Text winText;
    public Text restartText;
    public Text powerUpSizeText;
    public Text powerUpSpeedText;

    // Private variables
    //private int winScore = 10000;

    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "";
        powerUpSizeText.text = "";
        powerUpSpeedText.text = "";
        winText.text = "";
        restartText.text = "";

        if (!isLocalPlayer)
        {
            return;
        }
        SetScoreText(0);
    }

    // Sets score text and win-/restartText when game is finished
    public void SetScoreText(int sco)
    {
        scoreText.text = "Score: " + sco.ToString();

        // Doesn't work in multiplayer
        //if (sco == winScore)
        //{
        //    winText.text = "Tillykke du har vundet!";
        //    restartText.text = "Tryk 'r' for at starte forfra\n" +
        //                       "Tryk 'Esc' for at lukke";
        //    Time.timeScale = 0;
        //}
    }
}
