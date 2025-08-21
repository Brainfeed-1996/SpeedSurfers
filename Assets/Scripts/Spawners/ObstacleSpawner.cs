using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Génère des obstacles et des segments de décor (piste) de manière infinie
/// devant le joueur et nettoie ceux qui sont trop loin derrière.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Transform du joueur pour connaître la progression en Z.")]
    public Transform player;

    [Header("Voies & Distance")]
    [Tooltip("Largeur entre les voies (doit matcher PlayerController.laneWidth).")]
    public float laneWidth = 2.5f;

    [Tooltip("Distance maximale devant le joueur où générer de nouveaux éléments.")]
    public float spawnDistanceAhead = 60f;

    [Tooltip("Distance derrière le joueur où détruire/retirer les éléments.")]
    public float despawnDistanceBehind = 25f;

    [Header("Obstacles")]
    [Tooltip("Liste de prefabs d'obstacles (doivent avoir tag 'Obstacle' et un collider).")]
    public List<GameObject> obstaclePrefabs = new List<GameObject>();

    [Tooltip("Espacement en Z entre vagues d'obstacles.")]
    public float obstacleZStep = 8f;

    [Tooltip("Probabilité d'apparition d'un obstacle sur une voie (0..1).")]
    [Range(0f, 1f)]
    public float obstacleSpawnChancePerLane = 0.35f;

    [Header("Décor/sol infini")]
    [Tooltip("Prefab de segment de sol/décor allongé dans l'axe Z.")]
    public GameObject groundSegmentPrefab;

    [Tooltip("Longueur d'un segment de sol en unités (Z).")]
    public float groundSegmentLength = 10f;

    [Tooltip("Nombre de segments à maintenir devant le joueur.")]
    public int groundSegmentsAhead = 10;

    private float lastObstacleSpawnZ;

    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private readonly Queue<GameObject> activeGroundSegments = new Queue<GameObject>();

    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("ObstacleSpawner: 'player' n'est pas assigné.");
            enabled = false;
            return;
        }

        lastObstacleSpawnZ = Mathf.Floor(player.position.z / obstacleZStep) * obstacleZStep;

        // Pré-remplir le sol devant le joueur
        InitializeGround();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        SpawnObstaclesIfNeeded();
        MaintainGroundSegments();
        DespawnBehindPlayer();
    }

    private void SpawnObstaclesIfNeeded()
    {
        float playerZ = player.position.z;
        float targetZ = playerZ + spawnDistanceAhead;

        while (lastObstacleSpawnZ < targetZ)
        {
            lastObstacleSpawnZ += obstacleZStep;

            // Pour chaque voie, décider si on spawn un obstacle
            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                if (obstaclePrefabs.Count == 0)
                    break;

                if (Random.value <= obstacleSpawnChancePerLane)
                {
                    GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
                    Vector3 spawnPos = new Vector3((laneIndex - 1) * laneWidth, 0f, lastObstacleSpawnZ);
                    GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, cachedTransform);
                    activeObstacles.Add(instance);
                }
            }
        }
    }

    private void InitializeGround()
    {
        if (groundSegmentPrefab == null)
            return;

        float playerZ = player.position.z;
        float startZ = Mathf.Floor(playerZ / groundSegmentLength) * groundSegmentLength - groundSegmentLength;
        float endZ = playerZ + groundSegmentsAhead * groundSegmentLength;

        for (float z = startZ; z < endZ; z += groundSegmentLength)
        {
            SpawnGroundAtZ(z);
        }
    }

    private void MaintainGroundSegments()
    {
        if (groundSegmentPrefab == null)
            return;

        float playerZ = player.position.z;
        float neededEndZ = playerZ + groundSegmentsAhead * groundSegmentLength;

        // Spawn devant si nécessaire
        if (activeGroundSegments.Count > 0)
        {
            GameObject last = null;
            foreach (var seg in activeGroundSegments)
            {
                last = seg;
            }
            if (last != null)
            {
                float lastZ = last.transform.position.z;
                while (lastZ + groundSegmentLength < neededEndZ)
                {
                    lastZ += groundSegmentLength;
                    SpawnGroundAtZ(lastZ);
                }
            }
        }
        else
        {
            InitializeGround();
        }

        // Despawn derrière
        while (activeGroundSegments.Count > 0)
        {
            GameObject first = activeGroundSegments.Peek();
            if (first.transform.position.z < playerZ - groundSegmentLength * 2f)
            {
                activeGroundSegments.Dequeue();
                Destroy(first);
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnGroundAtZ(float z)
    {
        Vector3 pos = new Vector3(0f, 0f, z);
        GameObject instance = Instantiate(groundSegmentPrefab, pos, Quaternion.identity, cachedTransform);
        activeGroundSegments.Enqueue(instance);
    }

    private void DespawnBehindPlayer()
    {
        float cutoffZ = player.position.z - despawnDistanceBehind;

        // Nettoie les obstacles derrière
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObstacles[i];
            if (obj == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            if (obj.transform.position.z < cutoffZ)
            {
                Destroy(obj);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}


