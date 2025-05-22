using System;
using Commands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class UIActionButton : MonoBehaviour,IUIElement<ActionBase,UnityAction>
    {
        [SerializeField] private Image icon;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void EnableFor(ActionBase action, UnityAction onClick)
        {
            SetIcon(action.Icon);
            _button.interactable = true;
            _button.onClick.AddListener(onClick);
        }

        public void Disable()
        {
            SetIcon(null);
            _button.interactable = false;
            _button.onClick.RemoveAllListeners();
        }

        private void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                this.icon.enabled = false;
            }
            else
            {
                this.icon.sprite = icon;
                this.icon.enabled = true;
            }
        }
    }
}