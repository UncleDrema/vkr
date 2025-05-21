using Game.Movement.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.Movement
{
    public class MovementFeature : UpdateFeature
    {
        protected override void Initialize()
        {
            AddSystem(new MovementSystem());
        }
    }
}