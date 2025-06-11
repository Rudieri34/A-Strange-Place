using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_MovementController : MonoBehaviour
{
    public enum NPC_Behaviours
    {
        Idle,
        Patrol,
        Follow,
        Roam
    }

    [Header("Basic Components:")]
    [SerializeField] private NPC_Behaviours _targetBehaviour = NPC_Behaviours.Idle;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private NPC_DialogController _dialogController;
    [SerializeField] private Animator _animator;
    [SerializeField] bool _alwaysChat = false;
    [SerializeField] bool _chatOnDialogue = false;
    [SerializeField] float _minWaitTime = 0;
    [SerializeField] float _maxWaitTime = 3;



    bool _waiting = false;
    private NPC_Behaviours _currentBehaviour = NPC_Behaviours.Idle;

    [Header("Patrol Behaviour:")]
    [SerializeField] private Transform[] _patrolPositions;
    int _currentPatrolIndex = -1;

    [Header("Follow Behaviour:")]
    [SerializeField] private Transform _followTarget;

    [Header("Roam Around:")]
    [SerializeField] private float _roamRadius = 10f;
    private Vector3 _roamDestination;


    private void Awake()
    {
        SetFollowTarget(GameObject.FindWithTag("Player").transform);
    }


    private void SetupBehaviours()
    {
        switch (_currentBehaviour)
        {
            case NPC_Behaviours.Idle:
                IdleBehaviour();
                break;
            case NPC_Behaviours.Patrol:
                PatrolBehaviour();
                break;
            case NPC_Behaviours.Roam:
                RoamBehaviour();
                break;
            case NPC_Behaviours.Follow:
                FollowBehaviour();
                break;

        }

    }


    void IdleBehaviour()
    {
        _navMeshAgent.SetDestination(transform.position);
    }
    async void PatrolBehaviour()
    {
        if (_waiting)
            return;

        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            if (_currentPatrolIndex + 1 >= _patrolPositions.Length)
                _currentPatrolIndex = -1;

            _currentPatrolIndex++;

            _waiting = true;
            _navMeshAgent.SetDestination(transform.position);
            await UniTask.WaitForSeconds(Random.Range(_minWaitTime, _maxWaitTime));
            _navMeshAgent.SetDestination(_patrolPositions[_currentPatrolIndex].position);
            await UniTask.WaitForEndOfFrame(this);

            _waiting = false;

        }
    }
    async void RoamBehaviour()
    {
        if (_waiting)
            return;

        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * _roamRadius;
            randomDirection += transform.position;
            NavMeshHit hit;

            _waiting = true;
            _navMeshAgent.SetDestination(transform.position);
            await UniTask.WaitForSeconds(Random.Range(_minWaitTime, _maxWaitTime));
            if (NavMesh.SamplePosition(randomDirection, out hit, _roamRadius, 1))
            {
                _roamDestination = hit.position;
                _navMeshAgent.SetDestination(_roamDestination);
            }
            await UniTask.WaitForEndOfFrame(this);
            _waiting = false;


        }
    }

    public void SetFollowTarget(Transform newTarget)
    {
        _followTarget = newTarget;
    }

    async void FollowBehaviour()
    {
        if (_waiting)
            return;

        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            _navMeshAgent.SetDestination(_followTarget.position);
    }


    public void SetNewNPCMovementBehaviour(NPC_Behaviours newBehaviour)
    {
        _targetBehaviour = newBehaviour;
    }

    // Update is called once per frame
    void Update()
    {
        if (_dialogController.IsShowingDialog)
            _currentBehaviour = NPC_Behaviours.Idle;
        else if (_currentBehaviour != _targetBehaviour)
        {
            _currentBehaviour = _targetBehaviour;
        }

        _animator.SetBool("Walking", IsMoving());

        _animator.SetBool("Chating", !IsMoving() 
            && (_alwaysChat || 
            (_chatOnDialogue && _dialogController.IsShowingDialog)));

        SetupBehaviours();
    }

    bool IsMoving()
    {
        return _navMeshAgent.velocity.magnitude > 0f;
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _roamRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _navMeshAgent.stoppingDistance);

    }
}
