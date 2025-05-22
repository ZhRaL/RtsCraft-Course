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
    public class ActionsUI : MonoBehaviour, IUIElement<HashSet<ACommandable>>
    {
        [SerializeField] private UIActionButton[] _actionButtons;

        private void RefreshButton(HashSet<ACommandable> selectedUnits)
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
                    _actionButtons[i].EnableFor(actionForSlot, HandleClick(actionForSlot));
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

        public void EnableFor(HashSet<ACommandable> item)
        {
            RefreshButton(item);
        }

        public void Disable()
        {
            foreach (UIActionButton button in _actionButtons)
            {
                button.Disable();
            }
        }
    }
}