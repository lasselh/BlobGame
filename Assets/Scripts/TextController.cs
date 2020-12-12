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
    public Text highscoreListText;
    public Text winsText;
    public InputField usernameText;

    // Private variables
    //private int winScore = 10000;
    private DatabaseAccess databaseAccess;

    // Start is called before the first frame update
    void Start()
    {
        // Makes sure all text is empty at the start
        scoreText.text = "";
        powerUpSizeText.text = "";
        powerUpSpeedText.text = "";
        winText.text = "";
        restartText.text = "";
        winsText.text = "";
        highscoreListText.text = "";

        if (!isLocalPlayer)
        {
            return;
        }

        SetScoreText(0);

        // Access to singleton database
        databaseAccess = GameObject.FindGameObjectWithTag("DatabaseAccess").GetComponent<DatabaseAccess>();

        DisplayHighscoreList();
    }

    // Sets score text and win-/restartText when game is finished
    public void SetScoreText(int sco)
    {
        scoreText.text = "Score: " + sco.ToString();
    }

    // Displays the highscore list
    public void DisplayHighscoreList()
    {
        highscoreListText.text = "Highscore list:";

        List<Player> players = databaseAccess.GetPlayersFromDatabase();
        foreach (var player in players)
        {
            SetHighscoreText(player.Name, player.Wins);
        }
    }

    // Sets name+wins of a player
    public void SetHighscoreText(string name, int wins)
    {
        highscoreListText.text += $"\n{name}: {wins}";
    }

    // Sets amount of wins of current player
    public void SetAmountOfWinsText()
    {
        Player player = databaseAccess.GetOnePlayerFromDatabase(this.gameObject.transform.name);
        winsText.text = $"Wins: {player.Wins}";
    }

    // Sets the text when a player has won
    public void SetPlayerWonGameText()
    {
        winText.text = "Tillykke!\n" + "Du har vundet!";
    }

    public void SetPowerUpSizeText()
    {
        powerUpSizeText.text = "du fik en size powerup";
    }

    public void RemovePowerUpSizeText()
    {
        powerUpSizeText.text = "";
    }

    public void SetPowerUpSpeedText()
    {
        powerUpSpeedText.text = "du fik en speed powerup";
    }

    public void RemovePowerUpSpeedText()
    {
        powerUpSpeedText.text = "";
    }

}
