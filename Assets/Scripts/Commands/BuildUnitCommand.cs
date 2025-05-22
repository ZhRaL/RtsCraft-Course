using Units;
using UnityEngine;

namespace Commands
{
    [CreateAssetMenu(menuName = "Buildings/Commands/Build Unit",order = 120, fileName = "Build Unit")]
    public class BuildUnitCommand : ActionBase
    {
        [field: SerializeField] public UnitSO Unit { get; set; }
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding;
        }

        public override void Handle(CommandContext context)
        {
            BaseBuilding building = context.Commandable as BaseBuilding;
            building.BuildUnit(Unit);
        }
    }
}