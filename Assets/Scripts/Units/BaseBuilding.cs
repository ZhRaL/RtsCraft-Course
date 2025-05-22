using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class BaseBuilding : ACommandable
    {
        public Queue<UnitSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count >= MAX_QUEUE_SIZE)
            {
                Debug.LogError("Too many units in queue");
                return;
            }

            buildingQueue.Enqueue(unit);
            if (buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (buildingQueue.Count > 0)
            {
                UnitSO unitToBuild = buildingQueue.Peek();
                yield return new WaitForSeconds(unitToBuild.BuildTime);
                Instantiate(unitToBuild.Prefab, transform.position, Quaternion.identity);
                buildingQueue.Dequeue();
            }
        }
    }
}