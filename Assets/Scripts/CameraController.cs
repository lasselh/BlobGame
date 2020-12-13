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
        //int count = (int)databaseAccess.GetCountInDatabase();
        //int rand = Random.Range(0, count);
        //player = databaseAccess.GetRandomPlayerInDatabase(rand);
        //this.gameObject.name = player.Name;

        CmdUpNames(PlayerPrefs.GetString("name"));
    }

    void Update()
    {
        this.transform.Find("LabelHolder").rotation = Quaternion.identity;
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

        //CmdUpNames(this.gameObject.name);
        StartCoroutine(UpdateNames());
    }

    // Zooms camera out slightly
    public void CameraZoomOnSizeIncrease()
    {
        this.mainCamera.orthographicSize += 0.015f;
    }

    IEnumerator UpdateNames()
    {
        yield return new WaitForSeconds(1);
        CmdUpNames(this.gameObject.name);
    }

    [Command]
    public void CmdUpNames(string name)
    {
        RpcUpNames(name);
    }
    [ClientRpc]
    void RpcUpNames(string name)
    {
        this.gameObject.name = name;
        this.gameObject.transform.Find("LabelHolder/Label").GetComponent<TextMesh>().text = name;
    }
 }
