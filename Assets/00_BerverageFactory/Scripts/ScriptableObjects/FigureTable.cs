using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    [CreateAssetMenu(
        fileName = "FigureTable",
        menuName = "FBFactory/Scriptable Objects/Figure Table")]
    public class FigureTable : ScriptableObject
    {
        [SerializeField] private List<FigurePrefabEntry> _entries = new();

        public List<FigurePrefabEntry> Entries => _entries;
    }


    [Serializable]
    public class FigurePrefabEntry
    {
        [SerializeField] private FigureType _figureType;
        [SerializeField] private GameObject _prefab;

        public FigureType FigureType => _figureType;
        public GameObject Prefab => _prefab;
    }
}
