using Game.PotentialField.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateAgentPositionSystem : UpdateSystem
    {
        private readonly MapService _mapService;
        
        private Filter _objects;
        
        public UpdateAgentPositionSystem(MapService mapService)
        {
            _mapService = mapService;
        }
        
        public override void OnAwake()
        {
            _objects = World.Filter
                .With<MapPositionComponent>()
                .With<TransformComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _objects)
            {
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                ref var cMapPosition = ref agent.GetComponent<MapPositionComponent>();

                var pos = cTransform.Position();
                var mapPosition = _mapService.WorldToMapPosition(pos);
                cMapPosition.X = mapPosition.x;
                cMapPosition.Y = mapPosition.y;
            }
        }
    }
}