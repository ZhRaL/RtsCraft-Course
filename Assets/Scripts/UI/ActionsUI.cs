using System;
using System.Collections.Generic;
using System.Linq;
using Commands;
using EventBus;
using Events;
using Units;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] private UIActionButton[] _actionButtons;
        private HashSet<ACommandable> selectedUnits = new(12);
        

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        private void Start()
        {
            foreach (UIActionButton button in _actionButtons)
            {
                button.Disable();
            }
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            if (args.Unit is ACommandable commandable)
            {
                selectedUnits.Remove(commandable);
                RefreshButton();
            }
        }


        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            if (args.Unit is ACommandable commandable)
            {
                selectedUnits.Add(commandable);
                RefreshButton();
            }
        }

        private void RefreshButton()
        {
            HashSet<ActionBase> availableCommands = new(9);

            foreach (ACommandable commandable in selectedUnits)
            {
                availableCommands.UnionWith(commandable.AvailableCommands);
            }

            for (int i = 0; i < _actionButtons.Length; i++)
            {
                ActionBase actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();
                if (actionForSlot != null)
                {
                    _actionButtons[i].EnableFor(actionForSlot,HandleClick(actionForSlot));
                }
                else
                {
                    _actionButtons[i].Disable();
                }
            }
        }

        private UnityAction HandleClick(ActionBase actionForSlot)
        {
            return () => Bus<ActionSelectedEvent>.Raise(new ActionSelectedEvent(actionForSlot));
        }
    }
}