using Units;
using UnityEngine;

namespace Commands
{
    public struct CommandContext
    {
        public ACommandable Commandable { get; private set; }
        public RaycastHit Hit { get; private set; }
        public int UnitIndex { get; private set; }

        public CommandContext(ACommandable commandable, RaycastHit hit, int unitIndex = 0)
        {
            Commandable = commandable;
            Hit = hit;
            UnitIndex = unitIndex;
        }
    }
}