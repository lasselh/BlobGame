using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class SpawnController : NetworkBehaviour
{
    public GameObject mad;

    // Power up variables and list containing them all
    private List<GameObject> PowerUpList = new List<GameObject>();
    public GameObject powerUpSize;
    public GameObject powerUpSpeed;

    // Start is called before the first frame update
    void Start()
    {
        PowerUpList.Add(powerUpSize);
        PowerUpList.Add(powerUpSpeed);

        // Spawns 50 Mad at random locations at the start of the game
        //for (int i = 0; i < 50; i++)
        //{
        //    GameObject ServerMad = Instantiate(mad, new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f), 0), Quaternion.identity);
        //    NetworkServer.Spawn(ServerMad);
        //}

        StartCoroutine(SpawnMadRandomly());
        StartCoroutine(SpawnPowerUpRandomly());
    }

    // Spawns an amount of Mad at random locations on the map every x seconds
    IEnumerator SpawnMadRandomly()
    {
        while (true)
        {
            // How often it spawns Mad
            yield return new WaitForSeconds(5f);

            GameObject[] AmountOfMadOnMap = GameObject.FindGameObjectsWithTag("Mad");
            int MadCount = AmountOfMadOnMap.Length;

            // Max amount of Mad on map at a time
            if (MadCount <= 250)
            {
                // How much Mad it spawns at a time
                for (int i = 0; i < 20; i++)
                {
                    GameObject ServerMad = Instantiate(mad, new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f), 0), Quaternion.identity);
                    NetworkServer.Spawn(ServerMad);
                }
            }
        }
    }

    // Spawns random amount of random PowerUps at random locations on the map every x seconds
    IEnumerator SpawnPowerUpRandomly()
    {
        while (true)
        {
            // How often it spawns PowerUps
            yield return new WaitForSeconds(10f);

            GameObject[] AmountOfSizeOnMap = GameObject.FindGameObjectsWithTag("PowerUp - Size");
            GameObject[] AmountOfSpeedOnMap = GameObject.FindGameObjectsWithTag("PowerUp - Speed");
            int PowerUpCount = AmountOfSizeOnMap.Length;
            PowerUpCount += AmountOfSpeedOnMap.Length;

            // Max amount of powerUps on map at a time
            if (PowerUpCount <= 5)
            {
                // How many it spawns at a time (Currently between 1 and 3)
                int r = Random.Range(1, 4);
                // Which PowerUp to spawn from PowerUpList
                int powerupran = Random.Range(0, 2);

                for (int i = 0; i < r; i++)
                {
                    GameObject ServerPowerUp = Instantiate(PowerUpList[powerupran], new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-15.0f, 15.0f), 0), Quaternion.identity);
                    NetworkServer.Spawn(ServerPowerUp);
                }
            }
        }
    }
}
