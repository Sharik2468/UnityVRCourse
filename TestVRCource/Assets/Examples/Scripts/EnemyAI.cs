using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Подключаем библиотеку для работы с навигацией

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent; // Для управления перемещением AI
    public Transform player; // Позиция игрока
    public Transform[] points; // Точки для патрулирования
    private int destPoint = 0; // Текущая цель патрулирования
    public float chaseRange = 10f; // Дистанция обнаружения игрока

    public float patrolSpeed = 3.5f; // Скорость при патрулировании
    public float chaseSpeed = 7f; // Скорость при преследовании

    public Animator animator; // Подключи свой Animator здесь
    public float attackRange = 1f; // Дистанция для начала атаки

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol; // Начальное состояние AI
    void Start()
    {
        agent.autoBraking = false;
        agent.speed = patrolSpeed; // Устанавливаем начальную скорость патрулирования
        GoToNextPoint();
    }


    void GoToNextPoint()
    {
        // Возвращает, если нет точек патрулирования
        if (points.Length == 0)
            return;

        // Задаем в качестве цели следующую точку
        agent.destination = points[destPoint].position;

        // Переходим к следующей точке, возвращаемся к началу, если достигли последней
        destPoint = (destPoint + 1) % points.Length;
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
