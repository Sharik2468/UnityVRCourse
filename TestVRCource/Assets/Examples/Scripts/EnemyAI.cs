using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // ���������� ���������� ��� ������ � ����������

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent; // ��� ���������� ������������ AI
    public Transform player; // ������� ������
    public Transform[] points; // ����� ��� ��������������
    private int destPoint = 0; // ������� ���� ��������������
    public float chaseRange = 10f; // ��������� ����������� ������

    public float patrolSpeed = 3.5f; // �������� ��� ��������������
    public float chaseSpeed = 7f; // �������� ��� �������������

    public Animator animator; // �������� ���� Animator �����
    public float attackRange = 1f; // ��������� ��� ������ �����

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol; // ��������� ��������� AI
    void Start()
    {
        agent.autoBraking = false;
        agent.speed = patrolSpeed; // ������������� ��������� �������� ��������������
        GoToNextPoint();
    }


    void GoToNextPoint()
    {
        // ����������, ���� ��� ����� ��������������
        if (points.Length == 0)
            return;

        // ������ � �������� ���� ��������� �����
        agent.destination = points[destPoint].position;

        // ��������� � ��������� �����, ������������ � ������, ���� �������� ���������
        destPoint = (destPoint + 1) % points.Length;
    }


    void Update()
    {
        // ���������, ��������� �� ����� � ������� �������������
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        // ���������, ��������� �� ����� � ������� �������������
        if (distanceToPlayer < chaseRange)
        {
            state = State.Chase; // ����������� ��������� �� �������������
            agent.speed = chaseSpeed; // ������������� �������� �������������

            // ����������� �������� �� ���
            animator.SetBool("IsRun", true);

            // ���� ����� ��������� � �������� ��������� �����, �������� �����
            if (distanceToPlayer < attackRange)
            {
                state = State.Attack; // ������ ��� ��������� � enum
                // ���������� �������� �����
                animator.SetBool("IsAttack", true);
            }
            else
            {
                // ���� ����� �� � �������� ��������� �����, ���������� �������������
                animator.SetBool("IsAttack", false);
                agent.destination = player.position;
            }
        }
        else if (state == State.Chase)
        {
            // ���� ����� ����� �� ���� �������������, ������������ � ��������������
            state = State.Patrol;
            agent.speed = patrolSpeed; // ���������� �������� ��������������
            animator.SetBool("IsRun", false);
            GoToNextPoint();
        }

        // ��������� �������� � ����������� �� ���������
        switch (state)
        {
            case State.Patrol:
                // ���� �������� ����� ����������, ���� � ���������
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextPoint();
                break;
            case State.Chase:
                // ���������� ������
                agent.destination = player.position;
                break;
            case State.Attack:
                agent.destination = gameObject.transform.position;
                break;
        }
    }


}
