using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Подключаем библиотеку для работы с навигацией

public class EnemyAI : MonoBehaviour
{
    [Header("EnemySettings")]
    public NavMeshAgent agent; // Для управления перемещением AI
    public StatComponent characterStats;
    public Transform player; // Позиция игрока
    public List<Transform> points; // Точки для патрулирования
    private int destPoint = 0; // Текущая цель патрулирования
    public float chaseRange = 10f; // Дистанция обнаружения игрока
    public float patrolSpeed = 3.5f; // Скорость при патрулировании
    public float chaseSpeed = 7f; // Скорость при преследовании
    public float EnemyDamage = 5.0f;

    [Header("AnimSettings")]
    public Animator animator; // Подключи свой Animator здесь
    public float attackRange = 1f; // Дистанция для начала атаки

    [Header("NavMeshSettings")]
    public bool FindMovePoints; //Переменная отчечает за то, чтобы искать точки передвижения в начале игры
    public float DelayForFindingPoints; //Задержка перед поиском точек движения

    [Header("SoundSettings")]
    public AudioSource audioSource; // Источник звука
    public AudioClip[] patrolSounds; // Звуки для патрулирования
    public AudioClip[] chaseSounds; // Звуки для преследования
    public AudioClip[] attackSounds; // Звуки для атаки
    public float soundDelay = 5f; // Задержка между звуками

    public AudioSource bgAudioSource; // Источник звука
    public AudioClip bgPatrolSounds; // Звуки для патрулирования
    public AudioClip bgChaseSounds; // Звуки для преследования

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol; // Начальное состояние AI
    void Start()
    {
        agent.autoBraking = false;
        agent.speed = patrolSpeed; // Устанавливаем начальную скорость патрулирования

        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (FindMovePoints)
            StartCoroutine(FindPointsAfterDelay(DelayForFindingPoints));
        else
            GoToNextPoint();

        StartCoroutine(PlayRandomSounds()); // Запуск корутины для звуков
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
        FindPoints(); // Найти точки после задержки
        GoToNextPoint(); // И начать патрулирование
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
            // Код, который выполняется при ударе игрока
            // Например:
            // player.GetComponent<PlayerHealth>().TakeDamage(damageAmount);
            characterStats.TakeDamage(EnemyDamage);
        }
    }


    void Update()
    {
        // Проверяем, находится ли игрок в радиусе преследования
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        // Проверяем, находится ли игрок в радиусе преследования
        if (distanceToPlayer < chaseRange)
        {
            state = State.Chase; // Переключаем состояние на преследование
            agent.speed = chaseSpeed; // Устанавливаем скорость преследования

            if (bgAudioSource.clip != bgChaseSounds)
            {
                bgAudioSource.clip = bgChaseSounds;
                bgAudioSource.loop = true;
                bgAudioSource.Play();
            }

            // Переключаем анимацию на бег
            animator.SetBool("IsRun", true);

            // Если игрок находится в пределах дистанции атаки, начинаем атаку
            if (distanceToPlayer < attackRange)
            {
                state = State.Attack; // Добавь это состояние в enum
                // Активируем анимацию атаки
                animator.SetBool("IsAttack", true);
            }
            else
            {
                // Если игрок не в пределах дистанции атаки, продолжаем преследование
                animator.SetBool("IsAttack", false);
                agent.destination = player.position;
            }
        }
        else if (state == State.Chase)
        {
            // Если игрок вышел из зоны преследования, возвращаемся к патрулированию
            state = State.Patrol;
            agent.speed = patrolSpeed; // Возвращаем скорость патрулирования
            if (bgAudioSource.clip != bgPatrolSounds)
            {
                bgAudioSource.clip = bgPatrolSounds;
                bgAudioSource.loop = true;
                bgAudioSource.Play();
            }
            animator.SetBool("IsRun", false);
            GoToNextPoint();
        }

        // Выполняем действия в зависимости от состояния
        switch (state)
        {
            case State.Patrol:
                // Если достигли точки назначения, идем к следующей
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextPoint();
                break;
            case State.Chase:
                // Преследуем игрока
                agent.destination = player.position;
                break;
            case State.Attack:
                agent.destination = gameObject.transform.position;
                break;
        }
    }


}
