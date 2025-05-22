using System;
using System.Collections.Generic;
using EventBus;
using Events;
using Units;
using UnityEngine;

namespace UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private ActionsUI actionsUI;
        private HashSet<ACommandable> selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            if (args.Unit is ACommandable commandable)
            {
                selectedUnits.Remove(commandable);

                if (selectedUnits.Count > 0)
                {
                    actionsUI.EnableFor(selectedUnits);
                }
                else
                {
                   actionsUI.Disable(); 
                }
            }
        }

        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            if (args.Unit is ACommandable commandable)
            {
                selectedUnits.Add(commandable);
                actionsUI.EnableFor(selectedUnits);
            }
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }
    }
}