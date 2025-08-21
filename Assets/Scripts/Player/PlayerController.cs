using UnityEngine;

/// <summary>
/// Contrôle le joueur: course automatique, changement de voie (3 voies),
/// détection des entrées clavier et swipe mobile, collecte de pièces,
/// collision avec obstacles, et suivi caméra.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Vitesse & Déplacement")]
    [Tooltip("Vitesse de course vers l'avant en unités/seconde.")]
    public float forwardSpeed = 10f;

    [Tooltip("Vitesse de changement de voie (lissage latéral).")]
    public float laneChangeSpeed = 12f;

    [Tooltip("Largeur entre les voies (distance sur l'axe X).")]
    public float laneWidth = 2.5f;

    [Tooltip("Index de voie actuelle (0 = gauche, 1 = centre, 2 = droite)")]
    [Range(0, 2)]
    public int currentLaneIndex = 1;

    [Header("Caméra Suivi")]
    [Tooltip("Transform de la caméra à suivre (optionnel). Si vide, rien ne sera fait.")]
    public Transform cameraTransform;

    [Tooltip("Décalage caméra par rapport au joueur.")]
    public Vector3 cameraOffset = new Vector3(0f, 6f, -8f);

    [Tooltip("Lissage du suivi caméra.")]
    public float cameraFollowSmooth = 8f;

    [Header("Tags")]
    [Tooltip("Tag utilisé par les obstacles.")]
    public string obstacleTag = "Obstacle";

    [Tooltip("Tag utilisé par les pièces.")]
    public string coinTag = "Coin";

    // Seuils pour la détection de swipe mobile
    private const float SwipeThreshold = 60f; // pixels
    private Vector2 touchStartPosition;
    private bool touchInProgress;

    // Caches
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return; // Stoppe le mouvement si la partie est terminée
        }

        HandleKeyboardInput();
        HandleTouchInput();

        MoveForward();
        MoveLaterallyTowardsTargetLane();
    }

    private void LateUpdate()
    {
        UpdateCameraFollow();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ChangeLane(+1);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            touchInProgress = false;
            return;
        }

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            touchStartPosition = touch.position;
            touchInProgress = true;
        }
        else if (touch.phase == TouchPhase.Ended && touchInProgress)
        {
            Vector2 delta = touch.position - touchStartPosition;
            if (Mathf.Abs(delta.x) > SwipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x < 0f)
                {
                    ChangeLane(-1);
                }
                else
                {
                    ChangeLane(+1);
                }
            }

            touchInProgress = false;
        }
    }

    private void ChangeLane(int direction)
    {
        int targetLane = Mathf.Clamp(currentLaneIndex + direction, 0, 2);
        currentLaneIndex = targetLane;
    }

    private void MoveForward()
    {
        cachedTransform.position += Vector3.forward * (forwardSpeed * Time.deltaTime);
    }

    private void MoveLaterallyTowardsTargetLane()
    {
        float targetX = (currentLaneIndex - 1) * laneWidth; // -laneWidth, 0, +laneWidth
        Vector3 currentPosition = cachedTransform.position;
        float newX = Mathf.Lerp(currentPosition.x, targetX, laneChangeSpeed * Time.deltaTime);
        cachedTransform.position = new Vector3(newX, currentPosition.y, currentPosition.z);
    }

    private void UpdateCameraFollow()
    {
        if (cameraTransform == null)
            return;

        Vector3 desiredPosition = cachedTransform.position + cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, cameraFollowSmooth * Time.deltaTime);
        cameraTransform.LookAt(cachedTransform.position + Vector3.up * 1.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Collision avec obstacle -> Game Over
        if (other.CompareTag(obstacleTag))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        // Collecte de pièce -> +1 score et désactivation de la pièce
        if (other.CompareTag(coinTag))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }

            // On désactive pour permettre le pooling (si utilisé)
            other.gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Gestion des obstacles avec colliders non-trigger
        if (collision.collider != null && collision.collider.CompareTag(obstacleTag))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}


