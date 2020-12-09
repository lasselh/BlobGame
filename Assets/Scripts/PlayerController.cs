using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour
{
    // Public variables
    public float speed;
    public float maxSpeed;

    // Private variables
    private Rigidbody2D rb2d;
    private int score;
    private PlayerData _playerData;

    // Power up variables
    // Power up - Size
    private Vector3 powerUpSizeIncrease = new Vector3(0.1f, 0.1f, 0);
    // Power up - Speed
    private float powerUpSpeedIncrease = 2;

    // Called once when script starts
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        StartCoroutine(CheckIfWon());

        _playerData = new PlayerData();
        _playerData.Name = "Lasse";
        StartCoroutine(Download(_playerData.Name, result => {
            Debug.Log(result);
        }));
    }

    // FixedUpdate is called exactly 50 times a second, used for physics (movement)
    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        PlayerMovement();
    }

    // Handles what happens when player collides with other objects (Mad, PowerUps, walls, etc...)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Updates UI (Score, PowerUp text) - Applies on the client only
        Collision(other.gameObject);
        // Updates size - Makes RPC call, applies on all clients
        CmdCollision(other.gameObject);
    }

    // Handles collisions between 2 players
    void OnCollisionEnter2D(Collision2D other)
    {
        Vector3 thisSize = this.gameObject.transform.localScale;
        Vector3 otherSize = other.gameObject.transform.localScale;

        if (thisSize.magnitude > otherSize.magnitude)
        {
            // Updates UI (score)
            score += 10;
            GetComponent<TextController>().SetScoreText(score);
            // Sends command to server about collision, updating for all other clients
            CmdPlayerCollision(other.gameObject);
        }
    }

    // Sends a command from player objects on the client to player objects on the server about player collision
    [Command]
    void CmdPlayerCollision(GameObject other)
    {
        RpcPlayerCollision(other);
    }
    // Sends a ClientRpc from player objects on the server to player objects on the client about player collision
    // Updates involved player objects for all player clients (Removes losing player, increases size of winning player etc.)
    [ClientRpc]
    void RpcPlayerCollision(GameObject other)
    {
        // Server handles destruction of other player, updating on all other clients
        Destroy(other);
        Destroy(other.GetComponent<TextMesh>());
        gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
    }

    // Sends a command from player objects on the client to player objects on the server about collision
    [Command]
    void CmdCollision(GameObject other)
    {
        RpcCollision(other);
    }
    // Sends a ClientRpc from player objects on the server to player objects on the client about collision - updating invovled objects for all players
    // Updates involved objects for all player clients (Removes mad from map, increases size of relevant player etc.)
    [ClientRpc]
    void RpcCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Mad"))
        {
            Destroy(other.gameObject);
            gameObject.transform.localScale += new Vector3(0.01f, 0.01f, 0f);
        }
        else if (other.gameObject.CompareTag("PowerUp - Size"))
        {
            Destroy(other.gameObject);
            gameObject.transform.localScale += powerUpSizeIncrease;
            StartCoroutine(ExecuteAfterTime((5), () => { gameObject.transform.localScale -= powerUpSizeIncrease; }));
        }
        else if (other.gameObject.CompareTag("PowerUp - Speed"))
        {
            Destroy(other.gameObject);
            maxSpeed *= powerUpSpeedIncrease;
            speed *= powerUpSpeedIncrease;
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                maxSpeed /= 2;
                speed /= 2;
            }));
        }
    }

    // Handles collisions between player and other objects and writes the UI
    void Collision(GameObject other)
    {
        if (other.gameObject.CompareTag("Mad"))
        {
            // Zooms out slightly and updates score text
            GetComponent<CameraController>().CameraZoomOnSizeIncrease();

            score++;
            GetComponent<TextController>().SetScoreText(score);
        }
        else if (other.gameObject.CompareTag("PowerUp - Size"))
        {
            GetComponent<TextController>().powerUpSizeText.text = "du fik en size powerup";

            // Starts a coroutine, which executes after 5 seconds, removing text
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                GetComponent<TextController>().powerUpSizeText.text = "";
            }));
        }
        else if (other.gameObject.CompareTag("PowerUp - Speed"))
        {
            GetComponent<TextController>().powerUpSpeedText.text = "du fik en speed powerup";

            // Starts a coroutine, which executes after 5 seconds, removing text
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                GetComponent<TextController>().powerUpSpeedText.text = "";
            }));
        }
    }

    // Handles player movement
    void PlayerMovement()
    {
        // Gets horizontal/vertical input from player (WASD or arrows)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Creates a new vector and adds the force to player multiplied by speed
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * speed);

        // Limits speed to maxSpeed.
        if (rb2d.velocity.magnitude > maxSpeed)
        {
            rb2d.velocity = Vector3.ClampMagnitude(rb2d.velocity, maxSpeed);
        }

        // Stops player instantly after not pressing any movement keys (WASD or arrows)
        if (movement == new Vector2(0, 0))
            rb2d.velocity = new Vector2(0, 0);
    }

    // Checks if player is the only one alive every second
    IEnumerator CheckIfWon()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            GameObject[] amountOfPlayers = GameObject.FindGameObjectsWithTag("Player");
            int playerCount = amountOfPlayers.Length;

            if (playerCount == 1)
            {
                GetComponent<TextController>().winText.text = "Tillykke!\n" +
                                                              "Du har vundet!";

                StartCoroutine(Upload(_playerData.Stringify(), result => {
                    Debug.Log(result);
                }));
            }
            else
            {
                GetComponent<TextController>().winText.text = "";
            }
        }
    }

    // Waits for (time) seconds before doing (task)
    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        yield return new WaitForSeconds(time);
        task();
    }

    IEnumerator Download(string id, System.Action<PlayerData> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:27017/MongoCoronaDb/" + id))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(null);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(PlayerData.Parse(request.downloadHandler.text));
                }
            }
        }
    }

    IEnumerator Upload(string profile, System.Action<bool> callback = null)
    {
        using (UnityWebRequest request = new UnityWebRequest("http://localhost:27017/MongoCoronaDb/", "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(profile);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
                if (callback != null)
                {
                    callback.Invoke(false);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(request.downloadHandler.text != "{}");
                }
            }
        }
    }

    //// Runs even if TimeScale = 0 (Game paused), checks if player wants to restart/close game when finished
    ///////////////////////////////////
    //// Doesn't work in multiplayer //
    ///////////////////////////////////
    //void Update()
    //{
    //    if (!isLocalPlayer)
    //    {
    //        return;
    //    }
    //    if (score >= 0)
    //        CheckGameEnded();
    //}

    ////Checks key input when game is over
    //// Doesn't work in multiplayer //
    //void CheckGameEnded()
    //{
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        Time.timeScale = 1;
    //        SceneManager.LoadScene("Main");
    //    }

    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        Time.timeScale = 1;
    //        Application.Quit();
    //    }
    //    }
    //}
}