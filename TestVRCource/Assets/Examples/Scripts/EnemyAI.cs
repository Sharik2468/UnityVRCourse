using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // ���������� ���������� ��� ������ � ����������

public class EnemyAI : MonoBehaviour
{
    [Header("EnemySettings")]
    public NavMeshAgent agent; // ��� ���������� ������������ AI
    public StatComponent characterStats;
    public Transform player; // ������� ������
    public List<Transform> points; // ����� ��� ��������������
    private int destPoint = 0; // ������� ���� ��������������
    public float chaseRange = 10f; // ��������� ����������� ������
    public float patrolSpeed = 3.5f; // �������� ��� ��������������
    public float chaseSpeed = 7f; // �������� ��� �������������
    public float EnemyDamage = 5.0f;

    [Header("AnimSettings")]
    public Animator animator; // �������� ���� Animator �����
    public float attackRange = 1f; // ��������� ��� ������ �����

    [Header("NavMeshSettings")]
    public bool FindMovePoints; //���������� �������� �� ��, ����� ������ ����� ������������ � ������ ����
    public float DelayForFindingPoints; //�������� ����� ������� ����� ��������

    [Header("SoundSettings")]
    public AudioSource audioSource; // �������� �����
    public AudioClip[] patrolSounds; // ����� ��� ��������������
    public AudioClip[] chaseSounds; // ����� ��� �������������
    public AudioClip[] attackSounds; // ����� ��� �����
    public float soundDelay = 5f; // �������� ����� �������

    public AudioSource bgAudioSource; // �������� �����
    public AudioClip bgPatrolSounds; // ����� ��� ��������������
    public AudioClip bgChaseSounds; // ����� ��� �������������

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol; // ��������� ��������� AI
    void Start()
    {
        agent.autoBraking = false;
        agent.speed = patrolSpeed; // ������������� ��������� �������� ��������������

        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (FindMovePoints)
            StartCoroutine(FindPointsAfterDelay(DelayForFindingPoints));
        else
            GoToNextPoint();

        StartCoroutine(PlayRandomSounds()); // ������ �������� ��� ������
    }

    void FindPoints()
    {
        GameObject[] pointsObjects = GameObject.FindGameObjectsWithTag("MovePoint");
        points = new List<Transform>();

        foreach (GameObject pointObject in pointsObjects)
        {
            points.Add(pointObject.transform);
        }
    }

    IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(soundDelay);

            switch (state)
            {
                case State.Patrol:
                    PlayRandomSoundFrom(patrolSounds);
                    break;
                case State.Chase:
                    PlayRandomSoundFrom(chaseSounds);
                    break;
                case State.Attack:
                    PlayRandomSoundFrom(attackSounds);
                    break;
            }
        }
    }

    void PlayRandomSoundFrom(AudioClip[] clips)
    {
        if (clips.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, clips.Length);
            audioSource.clip = clips[randomIndex];
            audioSource.Play();
        }
    }



    IEnumerator FindPointsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FindPoints(); // ����� ����� ����� ��������
        GoToNextPoint(); // � ������ ��������������
    }


    void GoToNextPoint()
    {
        if (points.Count == 0)
            return;

        agent.destination = points[destPoint].position;
        destPoint = (destPoint + 1) % points.Count;
    }

    public void OnAttack()
    {
        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            // ���, ������� ����������� ��� ����� ������
            // ��������:
            // player.GetComponent<PlayerHealth>().TakeDamage(damageAmount);
            characterStats.TakeDamage(EnemyDamage);
        }
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

            if (bgAudioSource.clip != bgChaseSounds)
            {
                bgAudioSource.clip = bgChaseSounds;
                bgAudioSource.loop = true;
                bgAudioSource.Play();
            }

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
            if (bgAudioSource.clip != bgPatrolSounds)
            {
                bgAudioSource.clip = bgPatrolSounds;
                bgAudioSource.loop = true;
                bgAudioSource.Play();
            }
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
