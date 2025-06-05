using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

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

    }
    private void Start()
    {
        
        var points = GetComponentsInChildren<Transform>();
        for (int i = 1; i< points.Length; i++)
        {
            spawnPoints.Add((Transform)points[i]);
        }
    }

    public Transform GetSpawnPoint(int connectionId)
    {
        if (connectionId < spawnPoints.Count)
        {
            return spawnPoints[connectionId];
        }
        return spawnPoints[connectionId % spawnPoints.Count];
    }


}
