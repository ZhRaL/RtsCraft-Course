using System;
using Commands;
using EventBus;
using Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Units
{
    public abstract class ACommandable : MonoBehaviour, ISelectable
    {
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public ActionBase[] AvailableCommands { get; private set; }
        
        [SerializeField] private DecalProjector _decalProjector;
        [SerializeField] private UnitSO UnitSO;

        protected virtual void Start()
        {
            CurrentHealth = UnitSO.Health;
            MaxHealth = UnitSO.Health;
        }

        public void Select()
        {
            if (_decalProjector != null) 
                _decalProjector.gameObject.SetActive(true);
            
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void Deselect()
        {
            if (_decalProjector != null) 
                _decalProjector.gameObject.SetActive(false);
            
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }
    }
}