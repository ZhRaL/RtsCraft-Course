using EventBus;
using Units;
using Unity.VisualScripting;


namespace Events
{
    public struct UnitSpawnEvent : IEvent
    {
        public AUnit Unit { get; private set; }

        public UnitSpawnEvent(AUnit unit)
        {
            Unit = unit;
        }
    }
}