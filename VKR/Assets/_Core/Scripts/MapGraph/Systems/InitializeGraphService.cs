using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitializeGraphService : Initializer
    {
        private readonly GraphService _graphService;
        
        public InitializeGraphService(GraphService graphService)
        {
            _graphService = graphService;
        }
        
        public override void OnAwake()
        {
            _graphService.Initialize(World);
        }
    }
}