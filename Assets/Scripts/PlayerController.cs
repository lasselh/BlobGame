using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.UI;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public variables
    public float speed;
    public float maxSpeed;
    public Text scoreText;

    // Private variables
    private Rigidbody2D rb2d;
    private int score = 0;

    // Power up variables
    private Vector3 powerUpSize = new Vector3(0.1f, 0.1f, 0);
    public Text PowerUpSizeText;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        setScoreText();
        PowerUpSizeText.text = "";
    }

    void FixedUpdate()
    {
        // Gets horizontal/vertical input from player (WASD or arrows)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Creates a new vector and adds the force to player multiplied by speed
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * speed);

        // Limits speed to maxSpeed.
        if(rb2d.velocity.magnitude > maxSpeed)
        {
            rb2d.velocity = Vector3.ClampMagnitude(rb2d.velocity, maxSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Mad"))
        {
            // Removes the object collided with (Mad)
            other.gameObject.SetActive(false);

            // Increments size of player
            gameObject.transform.localScale += new Vector3(0.01f,0.01f,0f);

            score++;
            setScoreText();
        }

        if (other.gameObject.CompareTag("PowerUp - Size"))
        {
            // Removes the object collided with (Pick up)
            other.gameObject.SetActive(false);

            // Temporarily increases the size of the player
            gameObject.transform.localScale += powerUpSize;
            PowerUpSizeText.text = "du fik en powerup";

            // Starts a coroutine, which executes after 5 seconds, reducing the players size again
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                gameObject.transform.localScale -= powerUpSize;
                PowerUpSizeText.text = "";
            }));
        }
    }

    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        // Checks if a coroutine is already executing
        // VIRKER FUCKING IKKE
        //if (isCoroutineExecuting)
           // yield break;

        // Sets variable to true to lock it
        //isCoroutineExecuting = true;

        // waits for (time) seconds before doing (task)
        yield return new WaitForSeconds(time);
        task();

        // Sets variable to false to unlock it
        //isCoroutineExecuting = false;
    }

    // Sets/updates the score text for the UI
    void setScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}
