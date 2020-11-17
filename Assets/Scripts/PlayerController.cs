using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    // Public variables
    public float speed;
    public float maxSpeed;
    public Text scoreText;
    public Text winText;
    public Text RestartText;
    public Camera camera;
    public float cameraSize;

    // Private variables
    private Rigidbody2D rb2d;
    private int score = 0;
    private int winScore = 30;

    // Power up variables
    // Power up - Size
    private Vector3 powerUpSizeIncrease = new Vector3(0.1f, 0.1f, 0);
    public Text PowerUpSizeText;
    // Power up - Speed
    private float powerUpSpeedIncrease = 2;
    public Text PowerUpSpeedText;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        camera.orthographicSize = cameraSize;

        SetScoreText();
        PowerUpSizeText.text = "";
        PowerUpSpeedText.text = "";
        winText.text = "";
        RestartText.text = "";
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

    // Runs even if TimeScale = 0 (Game paused), checks if player wants to restart/close game when finished
    void Update()
    {
        if(score >= winScore)
            CheckGameEnded();
    }

    // Handles what happens when player collides with other objects
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Mad"))
        {
            // Removes the object collided with (Mad)
            //other.gameObject.SetActive(false);
            Destroy(other.gameObject);

            // Increments size of player
            gameObject.transform.localScale += new Vector3(0.01f,0.01f,0f);
            camera.orthographicSize += 0.02f;

            score++;
            SetScoreText();
        }
        else if (other.gameObject.CompareTag("PowerUp - Size"))
        {
            // Removes the object collided with (Power Up)
            // other.gameObject.SetActive(false);
            Destroy(other.gameObject);

            // Temporarily increases the size of the player
            gameObject.transform.localScale += powerUpSizeIncrease;
            PowerUpSizeText.text = "du fik en size powerup";

            // Starts a coroutine, which executes after 5 seconds, reducing the players size again
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                gameObject.transform.localScale -= powerUpSizeIncrease;
                PowerUpSizeText.text = "";
            }));
        }
        else if (other.gameObject.CompareTag("PowerUp - Speed"))
        {
            // Removes the object collided with (Power Up)
            //other.gameObject.SetActive(false);
            Destroy(other.gameObject);

            // Temporarily increases the speed of the player
            maxSpeed *= powerUpSpeedIncrease;
            speed *= powerUpSpeedIncrease;
            PowerUpSpeedText.text = "du fik en speed powerup";

            // Starts a coroutine, which executes after 5 seconds, reducing the players speed again
            StartCoroutine(ExecuteAfterTime((5), () =>
            {
                maxSpeed /= 2;
                speed /= 2;
                PowerUpSpeedText.text = "";
            }));
        }
    }

    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        // waits for (time) seconds before doing (task)
        yield return new WaitForSeconds(time);
        task();
    }

    // Sets/updates the score text for the UI
    void SetScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
        if (score == winScore)
        {
            winText.text = "Tillykke du har vundet!";
            RestartText.text = "Tryk 'r' for at starte forfra\n" +
                               "Tryk 'Esc' for at lukke";
            Time.timeScale = 0;
        }
    }

    void CheckGameEnded()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Main");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1;
            Application.Quit();
        }
    }
}
