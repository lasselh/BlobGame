using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine;
using System;
using UnityEngine.Events;
using TMPro;
using Random = UnityEngine.Random;

public class TextController : NetworkBehaviour
{
    // Public variables
    public Text scoreText;
    public Text winText;
    public Text restartText;
    public Text powerUpSizeText;
    public Text powerUpSpeedText;
    public Text highscoreListText;
    public Text amountOfWinsText;

    // Private variables
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
        amountOfWinsText.text = "";
        highscoreListText.text = "";

        if (!isLocalPlayer)
        {
            return;
        }

        // Access to singleton database
        databaseAccess = GameObject.FindGameObjectWithTag("DatabaseAccess").GetComponent<DatabaseAccess>();

        SetScoreText(0);
        DisplayHighscoreList();

        this.gameObject.transform.Find("LabelHolder/Label").GetComponent<TextMesh>().color = 
            new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        CmdUpdateNames(PlayerPrefs.GetString("name"));
    }

    void LateUpdate()
    {
        StartCoroutine(UpdateNames());
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
        amountOfWinsText.text = $"Wins: {player.Wins}";
    }

    // Sets the text when a player has won
    public void SetPlayerWonGameText()
    {
        winText.text = "Tillykke!\n" + "Du har vundet!";
    }

    public void SetPowerUpSizeText()
    {
        powerUpSizeText.text = "Du fik en size powerup";
    }

    public void RemovePowerUpSizeText()
    {
        powerUpSizeText.text = "";
    }

    public void SetPowerUpSpeedText()
    {
        powerUpSpeedText.text = "Du fik en speed powerup";
    }

    public void RemovePowerUpSpeedText()
    {
        powerUpSpeedText.text = "";
    }

    // Calls the command to update player names - used to start coroutine to update names
    IEnumerator UpdateNames()
    {
        yield return new WaitForSeconds(1);
        CmdUpdateNames(this.gameObject.name);
    }

    // Command/RPC call to update player names for all players.
    [Command]
    public void CmdUpdateNames(string name)
    {
        RpcUpdateNames(name);
    }
    [ClientRpc]
    void RpcUpdateNames(string name)
    {
        this.gameObject.name = name;
        this.gameObject.transform.Find("LabelHolder/Label").GetComponent<TextMesh>().text = name;
    }
}
