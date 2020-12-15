using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

public class CameraController : NetworkBehaviour
{
    // Private variables
    private Camera mainCamera;
    private Vector3 offset;
    public TextMesh playerNameTextMesh;

    // Start is called before the first frame update
    void Start()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        mainCamera = Camera.main;
        offset = mainCamera.transform.position;
    }

    void Update()
    {
        // Ensures player name doesn't rotate with player object for all players
        this.transform.Find("LabelHolder").rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Moves camera with player
        mainCamera.transform.position = this.transform.position + offset;
    }

    // Zooms camera out slightly
    public void CameraZoomOnSizeIncrease()
    {
        this.mainCamera.orthographicSize += 0.015f;
    }
 }
