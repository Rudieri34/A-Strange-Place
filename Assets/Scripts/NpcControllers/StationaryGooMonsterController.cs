using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // Não se esqueça de importar o DOTween

public class StationaryGooMonsterController : MonoBehaviour
{
    [Header("Configurações dos Tentáculos")]
    public GameObject[] tentaclePrefabs; // Prefabs dos tentáculos
    public int maxSimultaneousTentacles = 8; // Máximo de tentáculos ativos
    public float tentacleReachRadius = 3f;   // Até onde os tentáculos podem alcançar
    public float tentacleGrowthDuration = 0.4f;
    public float tentacleRetractDuration = 0.25f;
    public LayerMask solidSurfaceLayerMask;    // Layers que os tentáculos consideram como sólidas
    public float minAttachmentPointDistance = 0.75f; // Distância mínima entre os pontos de fixação

    [Header("Configurações do Jogador")]
    public Transform playerTransform;
    public string playerTag = "Player";
    public float playerDetectionRadius = 7f; // Raio para o monstro reagir ao jogador

    [Header("Debug")]
    public bool showGizmos = true;
    public Color playerDetectionGizmoColor = Color.cyan;
    public Color tentacleReachGizmoColor = Color.yellow;

    // Classe interna para gerenciar cada tentáculo no pool
    private class PooledTentacle
    {
        public GameObject Instance;
        public Transform InstanceTransform;
        public Vector3 WorldAttachmentPoint;
        public Tween CurrentTween;
        public bool IsDeployed; // Indica se está estendido (não crescendo nem retraindo)

        public PooledTentacle(GameObject prefab, Transform monsterTransform)
        {
            Instance = Instantiate(prefab, monsterTransform.position, Quaternion.identity, monsterTransform);
            InstanceTransform = Instance.transform;
            Instance.SetActive(false);
            IsDeployed = false;
        }

        public void Deploy(Vector3 monsterCenter, Vector3 targetPoint, float duration, Ease easeType)
        {
            CurrentTween?.Kill(); // Cancela qualquer animação anterior
            IsDeployed = false;

            InstanceTransform.position = monsterCenter;
            InstanceTransform.LookAt(targetPoint);

            float length = Vector3.Distance(monsterCenter, targetPoint);
            if (length < 0.01f) length = 0.01f; // Evita escala zero

            InstanceTransform.localScale = new Vector3(InstanceTransform.localScale.x, InstanceTransform.localScale.y, 0.01f);
            Instance.SetActive(true);
            WorldAttachmentPoint = targetPoint;

            CurrentTween = InstanceTransform.DOScaleZ(length, duration).SetEase(easeType).OnComplete(() =>
            {
                CurrentTween = null;
                IsDeployed = true;
            });
        }

        public void Retract(float duration, Ease easeType, System.Action onRetracted)
        {
            CurrentTween?.Kill();
            IsDeployed = false;

            CurrentTween = InstanceTransform.DOScaleZ(0.01f, duration).SetEase(easeType).OnComplete(() =>
            {
                Instance.SetActive(false);
                CurrentTween = null;
                onRetracted?.Invoke();
            });
        }
    }

    private List<PooledTentacle> _tentaclePool = new List<PooledTentacle>();
    private List<PooledTentacle> _activeTentacles = new List<PooledTentacle>(); // Tentáculos que estão visíveis/ativos
    private bool _playerInRange = false;

    void Start()
    {
        // Encontra o jogador se não estiver atribuído
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) playerTransform = playerObj.transform;
            else Debug.LogError($"{GetType().Name}: Jogador não encontrado! Verifique a tag ou atribua manualmente.");
        }

        // Validação de prefabs
        if (tentaclePrefabs == null || tentaclePrefabs.Length == 0)
        {
            Debug.LogError($"{GetType().Name}: Nenhum prefab de tentáculo atribuído!");
            enabled = false; // Desabilita o script se não houver prefabs
            return;
        }

        // Inicializa o pool de tentáculos
        for (int i = 0; i < maxSimultaneousTentacles; i++)
        {
            GameObject prefab = tentaclePrefabs[i % tentaclePrefabs.Length]; // Cicla pelos prefabs se houver mais de um
            _tentaclePool.Add(new PooledTentacle(prefab, transform));
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool currentPlayerInRange = distanceToPlayer < playerDetectionRadius;

        if (currentPlayerInRange && !_playerInRange) // Jogador acabou de entrar no alcance
        {
            _playerInRange = true;
            ExtendTentaclesToRandomPoints();
        }
        else if (!currentPlayerInRange && _playerInRange) // Jogador acabou de sair do alcance
        {
            _playerInRange = false;
            RetractAllActiveTentacles();
        }
        else if (currentPlayerInRange && _activeTentacles.Count < maxSimultaneousTentacles)
        {
            // Se o jogador continua no alcance e há espaço, tenta lançar mais tentáculos (opcional, pode preencher aos poucos)
            // Para um comportamento de "todos de uma vez", a lógica acima já cobre.
            // Esta linha poderia ser usada para um preenchimento mais gradual se nem todos foram lançados.
            // ExtendTentaclesToRandomPoints(maxSimultaneousTentacles - _activeTentacles.Count);
        }
    }

    void ExtendTentaclesToRandomPoints(int? specificCount = null)
    {
        int countToDeploy = specificCount ?? maxSimultaneousTentacles - _activeTentacles.Count;
        if (countToDeploy <= 0) return;

        int deployedThisCall = 0;
        int attempts = 0; // Para evitar loops infinitos se não encontrar pontos válidos
        List<Vector3> pointsChosenThisCall = new List<Vector3>(); // Para garantir distância entre os novos pontos

        while (deployedThisCall < countToDeploy && _activeTentacles.Count < maxSimultaneousTentacles && attempts < countToDeploy * 5)
        {
            attempts++;
            Vector3 randomDirection = Random.onUnitSphere; // Direção 3D aleatória
            // Opcional: Se quiser que os tentáculos sejam mais laterais:
            // randomDirection.y = Random.Range(-0.3f, 0.3f);
            // randomDirection.Normalize();

            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, randomDirection, out hitInfo, tentacleReachRadius, solidSurfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 potentialPoint = hitInfo.point;

                // Verifica distância mínima de outros tentáculos ativos
                bool tooCloseToExisting = false;
                foreach (var activeTentacle in _activeTentacles)
                {
                    if (Vector3.Distance(potentialPoint, activeTentacle.WorldAttachmentPoint) < minAttachmentPointDistance)
                    {
                        tooCloseToExisting = true;
                        break;
                    }
                }
                if (tooCloseToExisting) continue;

                // Verifica distância mínima de outros tentáculos escolhidos nesta mesma chamada
                foreach (var chosenPoint in pointsChosenThisCall)
                {
                    if (Vector3.Distance(potentialPoint, chosenPoint) < minAttachmentPointDistance)
                    {
                        tooCloseToExisting = true; // Reutilizando a flag
                        break;
                    }
                }
                if (tooCloseToExisting) continue;


                // Pega um tentáculo do pool
                PooledTentacle tentacleToDeploy = GetAvailableTentacleFromPool();
                if (tentacleToDeploy != null)
                {
                    _activeTentacles.Add(tentacleToDeploy);
                    pointsChosenThisCall.Add(potentialPoint);
                    tentacleToDeploy.Deploy(transform.position, potentialPoint, tentacleGrowthDuration, Ease.OutSine);
                    deployedThisCall++;
                }
                else break; // Não há mais tentáculos disponíveis no pool
            }
        }
    }

    PooledTentacle GetAvailableTentacleFromPool()
    {
        foreach (var pooledTentacle in _tentaclePool)
        {
            if (!pooledTentacle.Instance.activeSelf) // Se o GameObject está inativo, está disponível
            {
                // Garante que não está logicamente na lista de ativos (caso de alguma remoção assíncrona)
                if (!_activeTentacles.Contains(pooledTentacle))
                    return pooledTentacle;
            }
        }
        // Se todos os GameObjects do pool já estão ativos (mas talvez alguns não estejam na _activeTentacles list ainda)
        // Uma checagem mais robusta seria:
        foreach (var pooledTentacle in _tentaclePool)
        {
            bool isInActiveList = false;
            foreach (var activeT in _activeTentacles)
            {
                if (activeT == pooledTentacle)
                {
                    isInActiveList = true;
                    break;
                }
            }
            if (!isInActiveList) return pooledTentacle; // Encontrou um no pool que não está na lista de ativos
        }

        return null; // Todos os objetos do pool estão sendo gerenciados pela _activeTentacles
    }

    void RetractAllActiveTentacles()
    {
        // Cria uma cópia da lista para iterar, pois vamos modificar _activeTentacles
        List<PooledTentacle> tentaclesToRetract = new List<PooledTentacle>(_activeTentacles);
        _activeTentacles.Clear(); // Limpa a lista de ativos imediatamente

        foreach (var tentacle in tentaclesToRetract)
        {
            tentacle.Retract(tentacleRetractDuration, Ease.InSine, () =>
            {
                // Ação de callback após retração, se necessário.
                // O tentáculo já está inativo e pronto para ser reutilizado no pool.
            });
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = playerDetectionGizmoColor;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        Gizmos.color = tentacleReachGizmoColor;
        Gizmos.DrawWireSphere(transform.position, tentacleReachRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (var tentacle in _activeTentacles)
            {
                if (tentacle.Instance.activeSelf)
                {
                    Gizmos.DrawLine(transform.position, tentacle.WorldAttachmentPoint);
                    Gizmos.DrawSphere(tentacle.WorldAttachmentPoint, 0.1f);
                }
            }
        }
    }

    void OnDestroy()
    {
        // Garante que os tweens sejam parados quando o objeto é destruído
        foreach (var tentacle in _tentaclePool)
        {
            tentacle.CurrentTween?.Kill();
        }
    }
}