using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Old
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PotentialFieldCalculationSystem : UpdateSystem
    {
        private Filter _pointSources;
        private Filter _lineSources;
        private Filter _fieldValues;
        
        private const float PointFalloffDepth = 0.2f;
        private const float LineFalloffDepth = 0.2f;
        
        public override void OnAwake()
        {
            _pointSources = World.Filter
                .With<TransformComponent>()
                .With<PointFieldSourceComponent>()
                .Build();
            _lineSources = World.Filter
                .With<TransformComponent>()
                .With<LineFieldSourceComponent>()
                .Build();
            _fieldValues = World.Filter
                .With<TransformComponent>()
                .With<PotentialFieldValueComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var target in _fieldValues)
            {
                ref var transform = ref target.GetComponent<TransformComponent>();
                ref var fieldValue = ref target.GetComponent<PotentialFieldValueComponent>();
                var targetPos = transform.Position();
                
                fieldValue.Value = float3.zero;
                foreach (var source in _pointSources)
                {
                    ref var cSourceTransform = ref source.GetComponent<TransformComponent>();
                    ref var cSource = ref source.GetComponent<PointFieldSourceComponent>();

                    var sourcePos = cSourceTransform.Position();
                    var dir = targetPos - sourcePos;
                    var dist = math.length(dir);
                    if (dist < math.EPSILON)
                        continue;

                    float forceMagnitude = cSource.Strength;

                    if (dist > cSource.Radius)
                    {
                        float falloff = math.exp(-(dist - cSource.Radius) / cSource.DecaySpeed);
                        forceMagnitude *= falloff;
                    }

                    fieldValue.Value += math.normalize(dir) * forceMagnitude;
                }

                foreach (var source in _lineSources)
                {
                    ref var cSourceTransform = ref source.GetComponent<TransformComponent>();
                    ref var cSource = ref source.GetComponent<LineFieldSourceComponent>();

                    var localToWorldMatrix = cSourceTransform.LocalToWorld;
                    var p0 = math.transform(localToWorldMatrix, cSource.LocalStart);
                    var p1 = math.transform(localToWorldMatrix, cSource.LocalEnd);
                    var line = p1 - p0;
                    var lineLength = math.length(line);
                    var lineDir = line / lineLength;

                    var toTarget = targetPos - p0;
                    var projLength = math.dot(toTarget, lineDir);

                    if (projLength < 0 || projLength > lineLength)
                        continue;

                    var closestPoint = p0 + lineDir * projLength;
                    var delta = targetPos - closestPoint;
                    var dist = math.length(delta);
                    if (dist < math.EPSILON)
                        continue;

                    float forceMagnitude = cSource.Strength;

                    if (dist > cSource.Radius)
                    {
                        float falloff = math.exp(-(dist - cSource.Radius) / cSource.DecaySpeed);
                        forceMagnitude *= falloff;
                    }

                    fieldValue.Value += math.normalize(delta) * forceMagnitude;
                }
            }
        }
    }
}