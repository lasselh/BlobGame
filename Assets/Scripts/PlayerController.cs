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
    private DatabaseAccess databaseAccess;

    // Power up variables
    // Power up - Size
    private Vector3 powerUpSizeIncrease = new Vector3(0.1f, 0.1f, 0);
    // Power up - Speed
    private float powerUpSpeedIncrease = 2f;

    // Called once when script starts
    void Start()
    {
        this.gameObject.transform.position = new Vector2(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f));
        rb2d = GetComponent<Rigidbody2D>();

        // Access to singleton database
        databaseAccess = GameObject.FindGameObjectWithTag("DatabaseAccess").GetComponent<DatabaseAccess>();
    }

    // FixedUpdate is called exactly 50 times a second, used for physics (movement)
    void FixedUpdate()
    {
        // Used to ensure a player can ONLY move their own player object
        if (!isLocalPlayer)
        {
            return;
        }
        PlayerMovement();
    }

    // Handles what happens when player collides with other non-player objects (Mad, PowerUps, walls, etc...)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Updates UI (Score, PowerUp text etc.) - Applies on this client only
        Collision(other.gameObject);
        // Updates size of this object - Makes RPC call, applies on all clients
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

            // After 1 second (giving server time to react), this client checks if it has won. 
            Invoke("CheckIfWon", 1);
        }
    }

    // Sends a command from player objects on the client to player objects on the server about player collisions
    [Command]
    void CmdPlayerCollision(GameObject other)
    {
        RpcPlayerCollision(other);
    }
    // Sends a ClientRpc from player objects on the server to player objects on the client about player collisions
    // Updates involved player objects for all player clients (Removes losing player, increases size of winning player etc.)
    [ClientRpc]
    void RpcPlayerCollision(GameObject other)
    {
        Destroy(other.gameObject);
        Destroy(other.gameObject.GetComponent<TextMesh>());

        gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
    }

    // Sends a command from player objects on the client to player objects on the server about non-player collisions
    [Command]
    void CmdCollision(GameObject other)
    {
        RpcCollision(other);
    }
    // Sends a ClientRpc from player objects on the server to player objects on the client about non-player collisions - updating invovled objects for all players
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
            GetComponent<TextController>().SetPowerUpSizeText();

            // Starts a coroutine, which executes after 5 seconds, removing text along with effect
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                GetComponent<TextController>().RemovePowerUpSizeText();
            }));
        }
        else if (other.gameObject.CompareTag("PowerUp - Speed"))
        {
            GetComponent<TextController>().SetPowerUpSpeedText();

            // Starts a coroutine, which executes after 5 seconds, removing text along with effect
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                GetComponent<TextController>().RemovePowerUpSpeedText();
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

    // Checks if player is the only one alive
    void CheckIfWon()
    {
        GameObject[] amountOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        int playerCount = amountOfPlayers.Length;

        if (playerCount == 1)
        {
            // Updates player wins in database
            Player thisPlayer = databaseAccess.GetOnePlayerFromDatabase(this.gameObject.transform.name);
            thisPlayer.Wins++;
            databaseAccess.UpdatePlayerInDatabase(this.gameObject.transform.name, thisPlayer);

            GetComponent<TextController>().SetPlayerWonGameText();
            GetComponent<TextController>().DisplayHighscoreList();
            GetComponent<TextController>().SetAmountOfWinsText();
        }
        else
        {
            // do nothing
        }
    }

    // Waits for (time) seconds before doing (task)
    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        yield return new WaitForSeconds(time);
        task();
    }
}