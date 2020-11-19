using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraController : NetworkBehaviour
{
    private Camera mainCamera;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        offset = mainCamera.transform.position - this.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        mainCamera.transform.position = this.transform.position + offset;
    }

    // lortet bugger ad helvede til i multiplayer, gider ikke fikse det
    public void CameraZoomOnSizeIncrease()
    {
        mainCamera.orthographicSize += 0.02f;
    }
}
