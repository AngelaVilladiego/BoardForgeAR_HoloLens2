using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAsset : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject prefabToSpawn;

    // Method to handle the click
    public void RequestSpawn()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("No prefab defined to spawn.");
            return;
        }

        ItemPlaceManager.Instance.SpawnPrefab(prefabToSpawn);
    }
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
