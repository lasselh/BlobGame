using System;
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

    // Power up variables
    // Power up - Size
    private Vector3 powerUpSizeIncrease = new Vector3(0.1f, 0.1f, 0);
    // Power up - Speed
    private float powerUpSpeedIncrease = 2;

    // Called once when script starts
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
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

        Collision(other.gameObject);
        if (isServer == true)
        {
            CmdCollision(other.gameObject);
            //RpcCollision(other.gameObject);
        }
        else if (isClient && isServer == false)
        {
            CmdCollision(other.gameObject);
        }
    }

    // Handles collisions between 2 players
    void OnCollisionEnter2D(Collision2D other)
    {
        Vector3 thisSize = this.gameObject.transform.localScale;
        Vector3 otherSize = other.gameObject.transform.localScale;

        if (thisSize.magnitude > otherSize.magnitude)
        {
            Destroy(other.gameObject);
            gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
            score += 10;
            GetComponent<TextController>().SetScoreText(score);
            if (isServer)
            {
                RpcPlayerCollision(other.gameObject);
            }
            else if (isClient)
            {
                CmdPlayerCollision(other.gameObject);
            }
        }
    }

    // Sends a command from player objects on the client to player objects on the server about player collision
    [Command]
    void CmdPlayerCollision(GameObject other)
    {
        Destroy(other);
        gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
    }

    // Sends a ClientRpc from player objects on the server to player objects on the client about player collision
    [ClientRpc]
    void RpcPlayerCollision(GameObject other)
    {
        Destroy(other);
        gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
    }

    // Sends a command from player objects on the client to player objects on the server about collision
    [Command]
    void CmdCollision(GameObject other)
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

        RpcCollision(other);

        if (isServer == true)
        {
            gameObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0f);
        }
    }

    // Sends a ClientRpc from player objects on the server to player objects on the client about collision
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
            // Removes the object collided with (Mad)
            Destroy(other.gameObject);

            // Increments size of player
            gameObject.transform.localScale += new Vector3(0.01f, 0.01f, 0f);

            score++;
            GetComponent<TextController>().SetScoreText(score);
        }
        else if (other.gameObject.CompareTag("PowerUp - Size"))
        {
            // Removes the object collided with (Power Up)
            Destroy(other.gameObject);

            // Temporarily increases the size of the player
            gameObject.transform.localScale += powerUpSizeIncrease;
            GetComponent<TextController>().powerUpSizeText.text = "du fik en size powerup";

            // Starts a coroutine, which executes after 5 seconds, reducing the players size again
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                gameObject.transform.localScale -= powerUpSizeIncrease;
                GetComponent<TextController>().powerUpSizeText.text = "";
            }));
        }
        else if (other.gameObject.CompareTag("PowerUp - Speed"))
        {
            // Removes the object collided with (Power Up)
            Destroy(other.gameObject);

            // Temporarily increases the speed of the player
            maxSpeed *= powerUpSpeedIncrease;
            speed *= powerUpSpeedIncrease;
            GetComponent<TextController>().powerUpSpeedText.text = "du fik en speed powerup";

            // Starts a coroutine, which executes after 5 seconds, reducing the players speed again
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                maxSpeed /= 2;
                speed /= 2;
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
        if(movement == new Vector2(0, 0))
            rb2d.velocity = new Vector2(0,0);
    }

    // Waits for (time) seconds before doing (task)
    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        yield return new WaitForSeconds(time);
        task();
    }

    //GetComponent<TextController>().SetScoreText(score);
    // Runs even if TimeScale = 0 (Game paused), checks if player wants to restart/close game when finished
    /////////////////////////////////
    // Doesnt work in multiplayer //
    ////////////////////////////////
    //void Update()
    //{
    //    if(score >= winScore)
    //        CheckGameEnded();
    //}

    // Checks key input when game is over
    // !!! Doesnt work in multiplayer !!!
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
    //}
}
