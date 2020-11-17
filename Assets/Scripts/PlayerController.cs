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
    public GameObject mad;
    public Camera camera;
    public float cameraSize;

    // Private variables
    private Rigidbody2D rb2d;
    private int score = 0;
    private int winScore = 30;

    // Power up variables
    List<GameObject> PowerUpList = new List<GameObject>();
    // Power up - Size
    private Vector3 powerUpSizeIncrease = new Vector3(0.1f, 0.1f, 0);
    public Text PowerUpSizeText;
    public GameObject powerUpSize;
    // Power up - Speed
    private float powerUpSpeedIncrease = 2;
    public Text PowerUpSpeedText;
    public GameObject powerUpSpeed;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        camera.orthographicSize = cameraSize;

        SetScoreText();
        PowerUpSizeText.text = "";
        PowerUpSpeedText.text = "";
        winText.text = "";
        RestartText.text = "";


        PowerUpList.Add(powerUpSize);
        PowerUpList.Add(powerUpSpeed);

        // Spawns 50 Mad at the start of the game
        for (int i = 0; i < 50; i++)
        {
            Instantiate(mad, new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f), 0), Quaternion.identity);
        }
        StartCoroutine(SpawnMadRandomly());
        StartCoroutine(SpawnPowerUpRandomly());
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

    void Update()
    {
        if(score >= winScore)
            CheckGameEnded();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Mad"))
        {
            // Removes the object collided with (Mad)
            other.gameObject.SetActive(false);

            // Increments size of player
            gameObject.transform.localScale += new Vector3(0.01f,0.01f,0f);
            camera.orthographicSize += 0.02f;

            score++;
            SetScoreText();
        }

        if (other.gameObject.CompareTag("PowerUp - Size"))
        {
            // Removes the object collided with (Pick up)
            other.gameObject.SetActive(false);

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

        if (other.gameObject.CompareTag("PowerUp - Speed"))
        {
            // Removes the object collided with (Pick up)
            other.gameObject.SetActive(false);

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

    IEnumerator SpawnMadRandomly()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            GameObject[] AmountOfMadOnMap = GameObject.FindGameObjectsWithTag("Mad");
            int MadCount = AmountOfMadOnMap.Length;

            if (MadCount <= 250)
            {
                for (int i = 0; i < 20; i++)
                {
                    Instantiate(mad, new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f), 0), Quaternion.identity);
                }
            }
        }
    }

    IEnumerator SpawnPowerUpRandomly()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            GameObject[] AmountOfSizeOnMap = GameObject.FindGameObjectsWithTag("PowerUp - Size");
            GameObject[] AmountOfSpeedOnMap = GameObject.FindGameObjectsWithTag("PowerUp - Speed");
            int PowerUpCount = AmountOfSizeOnMap.Length;
            PowerUpCount += AmountOfSpeedOnMap.Length;

            if (PowerUpCount <= 5)
            {
                int r = Random.Range(1, 4);
                int powerupran = Random.Range(0, 1);

                for (int i = 0; i < r; i++)
                {
                    Instantiate(PowerUpList[powerupran], new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f), 0), Quaternion.identity);
                }
            }
        }
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
