using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreTypes : MonoBehaviour
{
    public enum MoveType
    {
        TwoAxis,
        OneAxisX,
        OneAxisY
    }
    public enum DifficultType
    {
        Easy,
        Normal,
        Hard
    }

    [Serializable]
    public struct DifficultStruct
    {
        [SerializeField] public Vector2 gridSize;
        [SerializeField] public Vector3 blockScale;
        [SerializeField] public int secondTimer;
    }

    public enum LabyrinthObjectType
    {
        Empty, //Преграда
        Start, //Расположение персонажа
        Exit, //Расположение выхода
        Bonus //Расположение бонуса
    }
    public enum BonusType
    {
        OnePoint = 0,
        TwoPoint = 1,
        ThreePoint = 2,
        Shield = 3
    }
    [Serializable]
    public struct BonusItem
    {
        public BonusType Type;
        public GameObject Object;
        public Vector3 locationOffset;
        public Vector3 scale;
    }
    [Serializable]
    public struct SpawnItem
    {
        public GameObject Object;
        public Vector3 locationOffset;
        public Vector3 scale; //Нет необходимости модифицировать, так как модифицируется в настройках сложности
    }

    [Serializable]
    public struct Cell
    {
        public Vector2Int indexCell;
        public LabyrinthObjectType cellType;
    }

    [System.Serializable]
    public class CellRow
    {
        public List<Cell> row = new List<Cell>();
    }

    public enum ExitRotationType
    {
        Top,
        Right
    }
}
