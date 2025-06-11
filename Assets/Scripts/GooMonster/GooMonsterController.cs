// 26/05/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEditor;

[RequireComponent(typeof(NavMeshAgent))]
public class GooMonsterController : MonoBehaviour
{
    [Header("Tentacle Settings")]
    public GameObject[] tentaclePrefabs;
    public int maxSimultaneousTentacles = 10;
    public float tentacleReachRadius = 2f;
    public float tentacleGrowthDuration = 0.5f;
    public float tentacleRetractDuration = 0.3f;
    public LayerMask solidSurfaceLayerMask;
    public float minAttachmentPointDistance = 0.5f;
    public int maxTentaclesPerGameObject = 2;

    [Header("Movement Settings")]
    public float engagementRadius = 15f;
    public float disengageRadius = 20f;
    public float maxDistanceFromOrigin = 20f;
    public float monsterChaseSpeed = 3f;
    public float monsterReturnSpeed = 2f;

    [Header("Player Settings")]
    public Transform playerTransform;
    public string playerTag = "Player";

    [Header("Debug Settings")]
    public bool showGizmos = true;
    public bool debugLineOfSightRays = false;
    public Color tentacleReachGizmoColor = Color.yellow;
    public Color playerDetectionGizmoColor = Color.blue;
    public Color engagementGizmoColor = new Color(0.5f, 0f, 1f);
    public Color disengageGizmoColor = new Color(1f, 0.5f, 0f);
    public Color maxOriginDistanceGizmoColor = Color.magenta;
    public Color losClearRayColor = Color.green;
    public Color losObstructedRayColor = Color.red;
    public Color losMissRayColor = Color.yellow;

    private class PooledTentacle
    {
        public GameObject Instance;
        public Transform InstanceTransform;
        public Vector3 WorldAttachmentPoint;
        public GameObject AttachedGameObject;
        public bool IsActiveAndAttached;
        public Tween CurrentTween;

        public PooledTentacle(GameObject prefab, Transform parent)
        {
            Instance = Instantiate(prefab, parent.position, Quaternion.identity, parent);
            InstanceTransform = Instance.transform;
            Instance.SetActive(false);
            IsActiveAndAttached = false;
        }

        public void Deploy(Vector3 monsterCenter, Vector3 targetPoint, GameObject targetGameObject, float duration, Ease easeType, System.Action onCompleteDeploy)
        {
            InstanceTransform.position = monsterCenter;
            InstanceTransform.LookAt(targetPoint);
            float length = Mathf.Max(Vector3.Distance(monsterCenter, targetPoint), 0.01f);

            InstanceTransform.localScale = new Vector3(InstanceTransform.localScale.x, InstanceTransform.localScale.y, 0.01f);
            Instance.SetActive(true);
            WorldAttachmentPoint = targetPoint;
            AttachedGameObject = targetGameObject;
            IsActiveAndAttached = false;

            CurrentTween?.Kill();
            CurrentTween = InstanceTransform.DOScaleZ(length, duration).SetEase(easeType).OnComplete(() =>
            {
                CurrentTween = null;
                IsActiveAndAttached = true;
                onCompleteDeploy?.Invoke();
            });
        }

        public void UpdateVisual(Vector3 monsterCenter)
        {
            if (!IsActiveAndAttached || (CurrentTween != null && CurrentTween.IsActive())) return;

            InstanceTransform.position = monsterCenter;
            InstanceTransform.LookAt(WorldAttachmentPoint);
            float requiredLength = Mathf.Max(Vector3.Distance(monsterCenter, WorldAttachmentPoint), 0.01f);

            InstanceTransform.localScale = new Vector3(InstanceTransform.localScale.x, InstanceTransform.localScale.y, requiredLength);
        }

        public void Retract(float duration, Ease easeType, System.Action onCompleteRetract)
        {
            IsActiveAndAttached = false;
            CurrentTween?.Kill();
            CurrentTween = InstanceTransform.DOScaleZ(0.01f, duration).SetEase(easeType).OnComplete(() =>
            {
                Instance.SetActive(false);
                AttachedGameObject = null;
                CurrentTween = null;
                onCompleteRetract?.Invoke();
            });
        }
    }

    private List<PooledTentacle> _tentaclePool = new List<PooledTentacle>();
    private List<PooledTentacle> _activeTentacles = new List<PooledTentacle>();
    private Dictionary<GameObject, int> _tentaclesAttachedToGameObject = new Dictionary<GameObject, int>();
    private Vector3 _originPosition;
    private bool _isChasingPlayer = false;

    private Collider[] _overlapColliders = new Collider[20];
    private List<(Vector3 point, GameObject obj)> _foundAttachmentPoints = new List<(Vector3, GameObject)>();
    private NavMeshAgent _navMeshAgent;

    private bool _stop;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        if (playerTransform == null || tentaclePrefabs == null || tentaclePrefabs.Length == 0 || _navMeshAgent == null || _stop) return;

        HandlePlayerState();
        HandleMovement();
        ManageTentacles();
    }

    void LateUpdate()
    {
        foreach (var tentacle in _activeTentacles)
        {
            if (tentacle.Instance.activeSelf)
            {
                tentacle.UpdateVisual(transform.position);
            }
        }
    }

    private void Initialize()
    {
        _originPosition = transform.position;
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            Debug.LogError("GooMonsterController: NavMeshAgent not found! Add a NavMeshAgent.");
            enabled = false;
            return;
        }

        _navMeshAgent.stoppingDistance = 0.1f;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) playerTransform = playerObj.transform;
            else Debug.LogError("GooMonsterController: Player not found!");
        }

        if (tentaclePrefabs == null || tentaclePrefabs.Length == 0)
        {
            Debug.LogError("GooMonsterController: No tentacle prefabs assigned!");
            enabled = false;
            return;
        }

        for (int i = 0; i < maxSimultaneousTentacles; i++)
        {
            GameObject prefabToSpawn = tentaclePrefabs[i % tentaclePrefabs.Length];
            _tentaclePool.Add(new PooledTentacle(prefabToSpawn, transform));
        }
    }

    private void HandlePlayerState()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        _isChasingPlayer = distanceToPlayer < engagementRadius || (_isChasingPlayer && distanceToPlayer <= disengageRadius);
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = _isChasingPlayer && playerTransform != null ? playerTransform.position : _originPosition;
        float currentSpeed = _isChasingPlayer ? monsterChaseSpeed : monsterReturnSpeed;

        if (_isChasingPlayer && Vector3.Distance(_originPosition, targetPosition) > maxDistanceFromOrigin)
        {
            targetPosition = _originPosition + (targetPosition - _originPosition).normalized * maxDistanceFromOrigin;
        }

        _navMeshAgent.speed = currentSpeed;
        if (_navMeshAgent.isOnNavMesh && _navMeshAgent.enabled)
        {
            _navMeshAgent.SetDestination(targetPosition);
        }
    }

    private void ManageTentacles()
    {
        RetractUnnecessaryTentacles();
        if (_isChasingPlayer || IsMonsterMoving())
        {
            TryDeployNewTentacles(maxSimultaneousTentacles - _activeTentacles.Count);
        }
    }

    private bool IsMonsterMoving()
    {
        return _navMeshAgent.isOnNavMesh && _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance + 0.2f;
    }

    private void RetractUnnecessaryTentacles()
    {
        for (int i = _activeTentacles.Count - 1; i >= 0; i--)
        {
            var tentacle = _activeTentacles[i];
            if (ShouldRetractTentacle(tentacle))
            {
                RetractTentacle(tentacle);
            }
        }
    }

    private bool ShouldRetractTentacle(PooledTentacle tentacle)
    {
        if (!tentacle.IsActiveAndAttached) return false;

        float distance = Vector3.Distance(transform.position, tentacle.WorldAttachmentPoint);
        if (distance > tentacleReachRadius || distance < 0.05f) return true;

        if (tentacle.AttachedGameObject == null) return true;

        Vector3 direction = (tentacle.WorldAttachmentPoint - transform.position).normalized;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance + 0.1f, solidSurfaceLayerMask))
        {
            return hit.collider.gameObject != tentacle.AttachedGameObject;
        }

        return true;
    }

    private void RetractTentacle(PooledTentacle tentacle)
    {
        _activeTentacles.Remove(tentacle);
        if (tentacle.AttachedGameObject != null && _tentaclesAttachedToGameObject.ContainsKey(tentacle.AttachedGameObject))
        {
            _tentaclesAttachedToGameObject[tentacle.AttachedGameObject]--;
            if (_tentaclesAttachedToGameObject[tentacle.AttachedGameObject] <= 0)
            {
                _tentaclesAttachedToGameObject.Remove(tentacle.AttachedGameObject);
            }
        }
        tentacle.Retract(tentacleRetractDuration, Ease.InSine, null);
    }

    private void TryDeployNewTentacles(int countToDeploy)
    {
        _foundAttachmentPoints.Clear();
        int hits = Physics.OverlapSphereNonAlloc(transform.position, tentacleReachRadius, _overlapColliders, solidSurfaceLayerMask);

        for (int i = 0; i < hits; i++)
        {
            Collider hitCollider = _overlapColliders[i];
            GameObject hitGameObject = hitCollider.gameObject;

            if (hitGameObject == gameObject || hitGameObject.transform.IsChildOf(transform)) continue;

            if (_tentaclesAttachedToGameObject.TryGetValue(hitGameObject, out int attachedCount) && attachedCount >= maxTentaclesPerGameObject)
            {
                continue;
            }

            Vector3 closestPoint = hitCollider.ClosestPoint(transform.position);
            if (Vector3.Distance(transform.position, closestPoint) < 0.1f) continue;

            if (IsPointObstructed(closestPoint, hitCollider)) continue;

            if (IsPointTooCloseToExistingTentacles(closestPoint)) continue;

            _foundAttachmentPoints.Add((closestPoint, hitGameObject));
        }

        DeployTentacles(countToDeploy);
    }

    private bool IsPointObstructed(Vector3 point, Collider hitCollider)
    {
        Vector3 direction = (point - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, point);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance + 0.05f, solidSurfaceLayerMask))
        {
            return hit.collider != hitCollider;
        }

        return false;
    }

    private bool IsPointTooCloseToExistingTentacles(Vector3 point)
    {
        foreach (var tentacle in _activeTentacles)
        {
            if (Vector3.Distance(point, tentacle.WorldAttachmentPoint) < minAttachmentPointDistance)
            {
                return true;
            }
        }

        foreach (var newPoint in _foundAttachmentPoints)
        {
            if (Vector3.Distance(point, newPoint.point) < minAttachmentPointDistance)
            {
                return true;
            }
        }

        return false;
    }

    private void DeployTentacles(int countToDeploy)
    {
        int deployedCount = 0;

        foreach (var pointTuple in _foundAttachmentPoints)
        {
            if (deployedCount >= countToDeploy) break;

            if (_tentaclesAttachedToGameObject.TryGetValue(pointTuple.obj, out int attachedCount) && attachedCount >= maxTentaclesPerGameObject)
            {
                continue;
            }

            PooledTentacle availableTentacle = GetAvailableTentacleFromPool();
            if (availableTentacle != null)
            {
                _activeTentacles.Add(availableTentacle);
                availableTentacle.Deploy(transform.position, pointTuple.point, pointTuple.obj, tentacleGrowthDuration, Ease.OutSine, () =>
                {
                    if (availableTentacle.IsActiveAndAttached)
                    {
                        if (!_tentaclesAttachedToGameObject.ContainsKey(pointTuple.obj))
                        {
                            _tentaclesAttachedToGameObject[pointTuple.obj] = 0;
                        }
                        _tentaclesAttachedToGameObject[pointTuple.obj]++;
                    }
                });
                deployedCount++;
            }
        }
    }

    private PooledTentacle GetAvailableTentacleFromPool()
    {
        foreach (var tentacle in _tentaclePool)
        {
            if (!tentacle.Instance.activeSelf)
            {
                return tentacle;
            }
        }
        return null;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (_stop)
            return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Death();
        }
        else if (collision.gameObject.CompareTag("WeirdSphere"))
        {
            Capture(collision.transform);
        }
    }

    async void Death()
    {
        GameManager.Instance.IsPlayerInputEnabled = false;
        _stop = true;

        await ScreenManager.Instance.ShowDialogText("You have been caught by the Monster!<br>This is your end.");
        await UniTask.WaitUntil(() => Input.anyKeyDown);
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    async void Capture(Transform ball)
    {
        _stop = true;

        transform.DOMove(ball.position, 0.5f).SetEase(Ease.InElastic)
            .OnComplete(() => _stop = false);
        transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                transform.SetParent(ball);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.gameObject.SetActive(false);
            });

        await ScreenManager.Instance.ShowDialogText("You have captured the Monster!");
        await UniTask.WaitUntil(() => Input.anyKeyDown);
        ScreenManager.Instance.HideDialogText();

    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 centerPos = transform.position;

        DrawGizmoSphere(centerPos, tentacleReachGizmoColor, tentacleReachRadius);
        DrawGizmoSphere(centerPos, playerDetectionGizmoColor, engagementRadius);
        DrawGizmoSphere(centerPos, disengageGizmoColor, disengageRadius);
        DrawGizmoSphere(_originPosition, maxOriginDistanceGizmoColor, maxDistanceFromOrigin);

        if (Application.isPlaying)
        {
            foreach (var tentacle in _activeTentacles)
            {
                if (tentacle.Instance.activeSelf && tentacle.IsActiveAndAttached)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(tentacle.WorldAttachmentPoint, 0.1f);
                    Gizmos.DrawLine(transform.position, tentacle.WorldAttachmentPoint);
                }
            }
        }
    }

    private void DrawGizmoSphere(Vector3 position, Color color, float radius)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(position, radius);
    }

    void OnDestroy()
    {
        foreach (var tentacle in _tentaclePool)
        {
            tentacle?.CurrentTween?.Kill();
        }
    }
}