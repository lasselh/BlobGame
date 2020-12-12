using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CameraController : NetworkBehaviour
{
    // Private variables
    private Camera mainCamera;
    private Vector3 offset;
    private GameObject playerName;
    private DatabaseAccess databaseAccess;
    private Player player;
    private TextMesh tm;
    public TextMesh playerNameTextMesh;

    // Start is called before the first frame update
    void Start()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        player = new Player();
        mainCamera = Camera.main;
        offset = mainCamera.transform.position - this.transform.position;
        playerName = new GameObject("player_label");
        tm = playerName.AddComponent<TextMesh>();

        // Access to singleton database
        databaseAccess = GameObject.FindGameObjectWithTag("DatabaseAccess").GetComponent<DatabaseAccess>();

        // Gets a random player in the database
        int count = (int)databaseAccess.GetCountInDatabase();
        int rand = Random.Range(0, count);
        player = databaseAccess.GetRandomPlayerInDatabase(rand);
        this.gameObject.name = player.Name;
    }

    void Update()
    {
        this.transform.Find("LabelHolder").rotation = Quaternion.identity;

        //this.gameObject.name = GetComponent<TextController>().usernameText.text;
    }

    // Update is called late once per frame
    void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Moves camera with player
        mainCamera.transform.position = this.transform.position + offset;

        CmdUpNames(this.gameObject.name);
    }

    // Zooms camera out slightly
    public void CameraZoomOnSizeIncrease()
    {
        this.mainCamera.orthographicSize += 0.015f;
    }



    [Command]
    void CmdUpNames(string name)
    {
        RpcUpNames(name);
    }
    [ClientRpc]
    void RpcUpNames(string name)
    {
        this.gameObject.name = name;
        this.gameObject.transform.Find("LabelHolder/Label").GetComponent<TextMesh>().text = name;
    }









    // Destroys player name label when player object is destroyed (dies or leaves)
    // Necessary since playerName is technically a different GameObject and is therefor not destroyed on player object destroy
    void OnDestroy()
    {
        Destroy(playerName);
    }

    // Command/RPC calls to update all player names location on all clients
    [Command]
    public void CmdUpdatePlayerName(string name)
    {
        //RpcUpdatePlayerName(name);
    }
    [ClientRpc]
    void RpcUpdatePlayerName(string name)
    {
        playerNameTextMesh.text = name;
        // Name label scales with players size
        //SetPlayerName();
    }


    // Sets the players name
    public void SetPlayerName()
    {
        tm.text = player.Name;

        //playerName.transform.rotation = Camera.main.transform.rotation; // Causes the text to face the camera

        // Gets a random color and styles the text
        tm.fontStyle = FontStyle.Bold;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.characterSize = 0.065f;
        tm.fontSize = 100;

        Vector3 nameOffset = new Vector3(0, (this.gameObject.transform.localScale.y * 2.5f + 0.5f), 0);
        playerName.gameObject.transform.position = this.gameObject.transform.position + nameOffset;
    }

    public void GetPlayerName()
    {
        //// Gets a random player in the database
        //int count = (int)databaseAccess.GetCountInDatabase();
        //int rand = Random.Range(0, count);
        //Player player = databaseAccess.GetRandomPlayerInDatabase(rand);

        player = databaseAccess.GetOnePlayerFromDatabase(this.GetComponent<TextController>().usernameText.text);

        //this.gameObject.transform.name = player.Name;

        tm.color = new Color(Random.Range(0.00f, 1f), Random.Range(0.00f, 1f), Random.Range(0.00f, 1f));

        // Fucker helt op hvis man er host af en eller anden grund?
        //this.GetComponent<TextController>().SetAmountOfWinsText();
        //isReady = true;
    }

    // Command/RPC calls to update all player names location on all clients
    [Command]
    public void CmdSetPlayerName()
    {
        //RpcSetPlayerName();
    }
    [ClientRpc]
    void RpcSetPlayerName()
    {
        //GetPlayerName();
    }
}
