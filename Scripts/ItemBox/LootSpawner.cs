using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class LootSpawner : NetworkBehaviour
{
    public GameObject[] lootItems;
   
    private GameObject RandomItemChoosen;
    private int index;

    //
    void Start()
    {
        RandomiseLoot();
    }

    private void RandomiseLoot()
    {
        //choose a random item from the loot
        index = Random.Range(0, lootItems.Length);
        RandomItemChoosen = lootItems[index];

        // spawn the item 
        GameObject LootItem = Instantiate(RandomItemChoosen, transform.position, Quaternion.identity);

        NetworkServer.Spawn(LootItem);

        Destroy(gameObject);
    }
}
