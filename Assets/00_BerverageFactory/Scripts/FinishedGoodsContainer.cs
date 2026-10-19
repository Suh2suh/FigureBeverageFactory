using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FBFactory
{
    public class FinishedGoodsContainer : MonoBehaviour
    {
        [SerializeField] private Transform _point00;
        [SerializeField] private Transform _pointNXNY;
        [SerializeField] private TextMeshPro _countText;

        private readonly List<BeveragePallet> _pallets = new();


        private void Awake()
        {
            UpdateCountText();
        }


        private void OnTriggerEnter(Collider other)
        {
            BeveragePallet pallet = other.GetComponentInChildren<BeveragePallet>();

            if (pallet != null)
                AddPallet(pallet);
        }


        public void AddPallet(BeveragePallet pallet)
        {
            if (_pallets.Contains(pallet))
                return;

            _pallets.Add(pallet);
            StackPallet(pallet);
            UpdateCountText();
        }


        public void RemovePallet(int n)
        {
            _pallets.RemoveAt(n);
            UpdateCountText();
        }


        private void StackPallet(BeveragePallet pallet)
        {
            Bounds palletBounds = GetBounds(pallet);
            int countX = Mathf.FloorToInt(
                Mathf.Abs(_pointNXNY.position.x - _point00.position.x) / palletBounds.size.x) + 1;
            int countY = Mathf.FloorToInt(
                Mathf.Abs(_pointNXNY.position.z - _point00.position.z) / palletBounds.size.z) + 1;
            int index = _pallets.Count - 1;
            int indexInLevel = index % (countX * countY);
            int indexX = indexInLevel / countY;
            int indexY = indexInLevel % countY;

            float positionX = _point00.position.x +
                Mathf.Sign(_pointNXNY.position.x - _point00.position.x) * palletBounds.size.x * indexX;
            float positionZ = _point00.position.z +
                Mathf.Sign(_pointNXNY.position.z - _point00.position.z) * palletBounds.size.z * indexY;
            float positionY = GetStackBaseY(positionX, positionZ);

            Vector3 position = pallet.transform.position;
            position.x = positionX;
            position.z = positionZ;
            pallet.transform.position = position;

            palletBounds = GetBounds(pallet);
            pallet.transform.position += Vector3.up * (positionY - palletBounds.min.y);
        }


        private float GetStackBaseY(float positionX, float positionZ)
        {
            Vector3 origin = new Vector3(positionX, _point00.position.y + 100f, positionZ);

            if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                200f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
                return hit.point.y;

            return _point00.position.y;
        }


        private Bounds GetBounds(BeveragePallet pallet)
        {
            Collider[] colliders = pallet.GetComponentsInChildren<Collider>();
            Bounds bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);

            return bounds;
        }


        private void UpdateCountText()
        {
            _countText.text = _pallets.Count.ToString();
        }
    }
}
