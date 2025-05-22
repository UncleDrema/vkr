using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitializeMapServiceSystem : Initializer
    {
        private MapService _mapService;
        
        public InitializeMapServiceSystem(MapService mapService)
        {
            _mapService = mapService;
        }
        
        public override void OnAwake()
        {
            _mapService.Initialize(World);
        }
    }
}