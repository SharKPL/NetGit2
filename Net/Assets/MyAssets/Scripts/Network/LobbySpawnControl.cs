using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class LobbySpawnControl : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [SerializeField]private int spawnCount = 0;

    private static LobbySpawnControl instance;
    public static LobbySpawnControl Instance {  get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject); 
        instance = this;
        var points = GetComponentsInChildren<Transform>();
        for (int i = 1; i < points.Length; i++)
        {
            spawnPoints.Add((Transform)points[i]);
        }
    }
    //private void Start()
    //{
        
    //    var points = GetComponentsInChildren<Transform>();
    //    for (int i = 1; i< points.Length; i++)
    //    {
    //        spawnPoints.Add((Transform)points[i]);
    //    }
    //}

    public Transform GetSpawnPoint(int connectionId)
    {
        Debug.LogError("GetSpawnPoint");
        if (connectionId < spawnPoints.Count)
        {
            return spawnPoints[connectionId];
        }
        return spawnPoints[connectionId % spawnPoints.Count];
    }

    public Transform GetOnlySpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Spawn points list is empty.");
            return null;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex];
    }

    public void RefreshSpawnPoints()
    {
        spawnPoints.Clear();
        NetworkStartPosition[] networkSpawnPositions = FindObjectsOfType<NetworkStartPosition>();
        
        foreach (NetworkStartPosition spawnPosition in networkSpawnPositions)
        {
            spawnPoints.Add(spawnPosition.transform);
        }
        
        spawnCount = spawnPoints.Count;
    }
}
