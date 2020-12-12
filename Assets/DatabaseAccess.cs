using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class DatabaseAccess : MonoBehaviour
{
    // Connects to the database
    MongoClient client = new MongoClient("mongodb+srv://Lasse:Lasse123@cluster0.jrhi2.mongodb.net/<dbname>?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<Player> collection;

    // Start is called before the first frame update
    void Start()
    {
        // Connects to the database and gets collection
        database = client.GetDatabase("BlobDb");
        collection = database.GetCollection<Player>("Players");

        //// Seeds database
        //SaveToDatabase("Lasse", Random.Range(0, 100));
        //SaveToDatabase("Mikkel", Random.Range(0, 100));
        //SaveToDatabase("Mathias", Random.Range(0, 100));
        //SaveToDatabase("Morten", Random.Range(0, 100));
        //SaveToDatabase("Mehran", Random.Range(0, 100));
        //SaveToDatabase("Martin", Random.Range(0, 100));
        //SaveToDatabase("Lucas", Random.Range(0, 100));
        //SaveToDatabase("Lars", Random.Range(0, 100));
    }

    // Adds a Player to database
    public void SaveToDatabase(string Name, int Wins)
    {
        var document = new Player { 
            Name = Name,
            Wins = Wins
        };
        collection.InsertOne(document);
    }

    // Gets all players from database
    public List<Player> GetPlayersFromDatabase()
    {
        List<Player> players = collection.Find<Player>(p => true).ToList();
        return players;
    }

    // Gets a specific player from database based on name
    public Player GetOnePlayerFromDatabase(string name)
    {
        Player player = new Player();

        player = collection.Find<Player>(p => p.Name == name).SingleOrDefault();

        return player;
    }

    // Updates a specific player in database
    public void UpdatePlayerInDatabase(string name, Player player)
    {
        collection.ReplaceOne<Player>(p => p.Name == name, player);
    }

    public void DeletePlayerFromDatabase(string name)
    {
        collection.DeleteOne<Player>(p => p.Name == name);
    }

    public long GetCountInDatabase()
    {
        return collection.CountDocuments<Player>(p => true);
    }

    public Player GetRandomPlayerInDatabase(int count)
    {
        return collection.Find<Player>(p => true).Limit(-1).Skip(count).FirstOrDefault();
    }




    // Deserialize the Json retrived from the database. Splits the Bson document into "Name" and "Wins" and adds this to a Player object
    // Skal nok slettes - Bruges ikke længere, da det ikke længere gemmes som Bson Documents i db, men som Player objekter.
    public Player Deserialize(string Json)
    {
        // "{ \"_id\" : ObjectId(\"5fd148473dd22a73605921f8\"), \"Lasse\" : 5}"
        var player = new Player();

        var playerWithoutId = Json.Substring(Json.IndexOf("),") + 4);
        var playerName = playerWithoutId.Substring(0, playerWithoutId.IndexOf(":") - 2);
        var playerWins = playerWithoutId.Substring(playerWithoutId.IndexOf(":") + 2, 1);

        player.Name = playerName;
        player.Wins = Convert.ToInt32(playerWins);

        return player;
    }
}
