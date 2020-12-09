using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraController : NetworkBehaviour
{
    private Camera mainCamera;
    private Vector3 offset;
    private GameObject playerName;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        offset = mainCamera.transform.position - this.transform.position;

        SetPlayerName();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        mainCamera.transform.position = this.transform.position + offset;

        // Updates the player name above each player every frame
        CmdUpdatePlayerName();
    }

    // Destroys player name label when player object is destroyed (dies or leaves)
    // Necessary since playerName is technically a different GameObject
    void OnDestroy()
    {
        Destroy(playerName);
    }

    // Command/RPC calls to update player names on all clients
    [Command]
    void CmdUpdatePlayerName()
    {
        RpcUpdatePlayerName();
    }
    [ClientRpc]
    void RpcUpdatePlayerName()
    {
        Vector3 nameOffset = new Vector3(0, (this.gameObject.transform.localScale.y * 2.5f + 0.5f), 0);
        playerName.gameObject.transform.position = this.gameObject.transform.position + nameOffset;
    }

    // Zooms out slightly
    public void CameraZoomOnSizeIncrease()
    {
        this.mainCamera.orthographicSize += 0.015f;
    }

    // Sets the players name
    void SetPlayerName()
    {
        // TO DO: Set name from database

        // Sets random name
        int playerNr = Random.Range(1, 11);
        string playerName1 = $"Player {playerNr}";
        this.transform.name = playerName1;
        //////////////////////////////////

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
