using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Génère des pièces sur les voies à intervalles réguliers avec une chance donnée.
/// Les pièces doivent avoir un collider en IsTrigger et le tag "Coin".
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    [Header("Références")]
    public Transform player;

    [Header("Voies & Distance")]
    public float laneWidth = 2.5f;
    public float spawnDistanceAhead = 60f;
    public float despawnDistanceBehind = 25f;

    [Header("Pièces")]
    public GameObject coinPrefab;
    public float coinZStep = 4f;

    [Range(0f, 1f)]
    public float coinSpawnChancePerLane = 0.5f;

    [Tooltip("Si vrai, peut créer de petites lignes de 3 pièces sur la même voie.")]
    public bool spawnTriplets = true;

    private float lastCoinSpawnZ;
    private readonly List<GameObject> activeCoins = new List<GameObject>();
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("CoinSpawner: 'player' n'est pas assigné.");
            enabled = false;
            return;
        }

        if (coinPrefab == null)
        {
            Debug.LogError("CoinSpawner: 'coinPrefab' n'est pas assigné.");
            enabled = false;
            return;
        }

        lastCoinSpawnZ = Mathf.Floor(player.position.z / coinZStep) * coinZStep;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        SpawnCoinsIfNeeded();
        DespawnBehindPlayer();
    }

    private void SpawnCoinsIfNeeded()
    {
        float playerZ = player.position.z;
        float targetZ = playerZ + spawnDistanceAhead;

        while (lastCoinSpawnZ < targetZ)
        {
            lastCoinSpawnZ += coinZStep;

            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                if (Random.value <= coinSpawnChancePerLane)
                {
                    if (spawnTriplets && Random.value > 0.6f)
                    {
                        // Petite série de 3 pièces espacées de 1.5 unités en Z
                        for (int k = 0; k < 3; k++)
                        {
                            Vector3 pos = new Vector3((laneIndex - 1) * laneWidth, 1f, lastCoinSpawnZ + k * 1.5f);
                            SpawnCoin(pos);
                        }
                    }
                    else
                    {
                        Vector3 pos = new Vector3((laneIndex - 1) * laneWidth, 1f, lastCoinSpawnZ);
                        SpawnCoin(pos);
                    }
                }
            }
        }
    }

    private void SpawnCoin(Vector3 position)
    {
        GameObject instance = Instantiate(coinPrefab, position, Quaternion.identity, cachedTransform);
        activeCoins.Add(instance);
    }

    private void DespawnBehindPlayer()
    {
        float cutoffZ = player.position.z - despawnDistanceBehind;
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeCoins[i];
            if (obj == null)
            {
                activeCoins.RemoveAt(i);
                continue;
            }

            if (!obj.activeSelf || obj.transform.position.z < cutoffZ)
            {
                Destroy(obj);
                activeCoins.RemoveAt(i);
            }
        }
    }
}


