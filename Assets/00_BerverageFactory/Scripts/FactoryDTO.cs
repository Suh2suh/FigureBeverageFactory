using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class FactoryDTO : SingletonBase<FactoryDTO>
    {
        [SerializeField] private FigureDTO _figureTableData;
        public FigureDTO FigureTable
        {
            get
            {
                _figureTableData.TryInit();
                return _figureTableData;
            }
        }


        [System.Serializable]
        public class FigureDTO
        {
            [SerializeField] private FigureTable _figureTable;
            private Dictionary<FigureType, GameObject> _figurePrefabs;

            public void TryInit()
            {
                if (_figurePrefabs != null)
                    return;

                _figurePrefabs = new Dictionary<FigureType, GameObject>();
                foreach (FigurePrefabEntry entry in _figureTable.Entries)
                {
                    _figurePrefabs.Add(entry.FigureType, entry.Prefab);
                }
            }


            public GameObject GetFigurePrefab(FigureType figureType)
            {
                return _figurePrefabs[figureType];
            }
        }
    }
}
