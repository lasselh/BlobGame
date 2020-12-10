using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraController : NetworkBehaviour
{
    // Private variables
    private Camera mainCamera;
    private Vector3 offset;
    private GameObject playerName;
    private DatabaseAccess databaseAccess;

    // Used to check if player name has been set and is ready to be shown
    private bool isReady = false;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        offset = mainCamera.transform.position - this.transform.position;

        // Access to singleton database
        databaseAccess = GameObject.FindGameObjectWithTag("DatabaseAccess").GetComponent<DatabaseAccess>();

        SetPlayerName();
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

        // Updates the player name above each player for all clients every frame
        CmdUpdatePlayerName();
    }

    // Destroys player name label when player object is destroyed (dies or leaves)
    // Necessary since playerName is technically a different GameObject and is therefor not destroyed on player object destroy
    void OnDestroy()
    {
        Destroy(playerName);
    }

    // Command/RPC calls to update all player names location on all clients
    [Command]
    void CmdUpdatePlayerName()
    {
        if(isReady)
            RpcUpdatePlayerName();
    }
    [ClientRpc]
    void RpcUpdatePlayerName()
    {
        // Name label scales with players size
        Vector3 nameOffset = new Vector3(0, (this.gameObject.transform.localScale.y * 2.5f + 0.5f), 0);
        playerName.gameObject.transform.position = this.gameObject.transform.position + nameOffset;
    }

    // Zooms camera out slightly
    public void CameraZoomOnSizeIncrease()
    {
        this.mainCamera.orthographicSize += 0.015f;
    }

    // Sets the players name
    void SetPlayerName()
    {
        isReady = true;

        // Gets a random player in the database
        int count = (int)databaseAccess.GetCountInDatabase();
        int rand = Random.Range(0, count);
        Player player = databaseAccess.GetRandomPlayerInDatabase(rand);

        string setPlayerName = player.Name;
        this.transform.name = setPlayerName;

        // Sets the player name above the player - Has to be new GameObject, otherwise it would rotate with player object
        playerName = new GameObject("player_label");
        playerName.transform.rotation = Camera.main.transform.rotation; // Causes the text to face the camera
        TextMesh tm = playerName.AddComponent<TextMesh>();
        tm.text = this.gameObject.name;

        // Gets a random color and styles the text
        tm.color = new Color(Random.Range(0.00f, 1f), Random.Range(0.00f, 1f), Random.Range(0.00f, 1f));
        tm.fontStyle = FontStyle.Bold;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.characterSize = 0.065f;
        tm.fontSize = 100;
    }
}
