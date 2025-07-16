using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private Animator animator;
    private bool isMoving;
    private float stoppingDistance = 1f;
    public SelectableObject selectableObject;

    private void Start()
    {
        GameManager._instance.RegisterUnit(this);
        selectableObject = GetComponent<SelectableObject>();
    }

    private void Update()
    {
        if(isMoving && !navMeshAgent.pathPending)
        {
            if(navMeshAgent.remainingDistance <= stoppingDistance)
            {
                isMoving = false;
                animator.SetBool("IsRunning", false);
            }
        }
    }

    public void MoveToDestination(Vector3 newDestination)
    {
        navMeshAgent.SetDestination(newDestination);
        animator.SetBool("IsRunning", true);
        isMoving = true;
    }
}
