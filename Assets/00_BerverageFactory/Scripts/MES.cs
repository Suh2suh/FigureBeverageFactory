using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FBFactory
{
    public class MES : SingletonBase<MES>
    {
        [SerializeField] private LineController _lineController;
        [FormerlySerializedAs("_orders")]
        [SerializeField] private List<MESOrderData> _mesOrderDataList;
        [SerializeField, ReadOnly] private List<OrderProductionStatus> _orderStatus;
        private Dictionary<string, OrderProductionStatus> _orderStatusById = new();
        private bool _isInit = false;


        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInit)
                return;

            _orderStatusById.Clear();

            for (int sequence = 0; sequence < _mesOrderDataList.Count; sequence++)
            {
                MESOrderData orderData = _mesOrderDataList[sequence];

                if (string.IsNullOrEmpty(orderData.OrderId))
                    orderData.OrderId = Guid.NewGuid().ToString("N");

                OrderProductionStatus orderStatus = new()
                {
                    Order = orderData,
                    Yield = 0,
                    Scrap = 0,
                };

                _orderStatus.Add(orderStatus);
                _orderStatusById.Add(orderData.OrderId, orderStatus);
            }

            _lineController.ConfigureOrders(_mesOrderDataList);

            _isInit = true;
        }


        public void OnProduced(string orderId)
        {
            _orderStatusById[orderId].Yield++;
        }

        public void OnRejected(string orderId)
        {
            _orderStatusById[orderId].Scrap++;
        }
    }


    [System.Serializable]
    class OrderProductionStatus
    {
        public MESOrderData Order;
        public int Yield;
        public int Scrap;
    }

    [System.Serializable]
    public class MESOrderData
    {
        [ReadOnly] public string OrderId;
        [ReadOnly] public int ProductionSequence;
        [field: SerializeField] public BOMScriptableObject BOM { get; private set; }
        [field: SerializeField] public int TargetQuantity { get; private set; }
    }
}
