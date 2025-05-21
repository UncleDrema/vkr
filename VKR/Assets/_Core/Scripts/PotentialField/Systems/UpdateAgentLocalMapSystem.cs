using System;
using Game.PotentialField.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateAgentLocalMapSystem : UpdateSystem
    {
        private Filter _agents;
        private Filter _dynamicObstacles;
        
        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentLocalFieldComponent>()
                .With<PotentialFieldComponent>()
                .With<MapPositionComponent>()
                .With<TransformComponent>()
                .Build();
            
            _dynamicObstacles = World.Filter
                .With<DynamicObstacleComponent>()
                .With<MapPositionComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _agents)
            {
                ref var cLocalField = ref agent.GetComponent<AgentLocalFieldComponent>();
                ref var cField = ref agent.GetComponent<PotentialFieldComponent>();
                ref var cAgentPosition = ref agent.GetComponent<MapPositionComponent>();
                
                var r = cLocalField.Radius;
                var s = cLocalField.Size;
                var w = cField.Width;
                var h = cField.Height;
                var x = cAgentPosition.X;
                var y = cAgentPosition.Y;
                var gx = cField.GoalX;
                var gy = cField.GoalY;

                if (gx == x && gy == y)
                {
                    agent.AddComponent<GoalReachedEvent>();
                    agent.AddComponent<ClearFieldGoalRequest>();
                    continue;
                }
                
                // 1) Инициализируем карту из глобальной карты
                for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    var i = (y + dy) * w + (x + dx);
                    if (i < 0 || i >= w * h)
                    {
                        cLocalField.Potentials[(dy + r) * s + (dx + r)] = 0;
                        cLocalField.Fixed[(dy + r) * s + (dx + r)] = false;
                    }
                    else
                    {
                        cLocalField.Potentials[(dy + r) * s + (dx + r)] = cField.Potentials[i];
                        cLocalField.Fixed[(dy + r) * s + (dx + r)] = cField.Fixed[i];
                    }
                }
                
                // 2) Отмечаем границу окна (B-зона) как препятствие
                for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++) 
                    if (dx == -r || dx == r || dy == -r || dy == r)
                    {
                        cLocalField.Potentials[(dy + r) * s + (dx + r)] = 0;
                        cLocalField.Fixed[(dy + r) * s + (dx + r)] = true;
                    }
                
                // Отметим динамические препятствия
                foreach (var dynamicObstacle in _dynamicObstacles)
                {
                    if (dynamicObstacle == agent) continue;
                    
                    ref var cDynamicObstacle = ref dynamicObstacle.GetComponent<DynamicObstacleComponent>();
                    ref var cObstaclePosition = ref dynamicObstacle.GetComponent<MapPositionComponent>();
                    var sx = cDynamicObstacle.SizeX;
                    var sy = cDynamicObstacle.SizeY;
                    var ox = cObstaclePosition.X;
                    var oy = cObstaclePosition.Y;

                    sx = math.max(0, sx / 2 - 1);
                    sy = math.max(0, sy / 2 - 1);
                    
                    for (int dy = -sy; dy <= sy; dy++)
                    for (int dx = -sx; dx <= sx; dx++)
                    {
                        var ofx = (ox + dx) - x;
                        var ofy = (oy + dy) - y;
                        if (ofx < -r || ofx > r || ofy < -r || ofy > r)
                            continue;
                        var i = (ofy + r) * s + (ofx + r);
                        if (i < 0 || i >= s * s)
                            continue;
                        cLocalField.Potentials[i] = 0;
                        cLocalField.Fixed[i] = true;
                    }
                }
                
                // Если цель находится в пределах окна, то отметим её как цель и не будем вычислять дальше
                if (x - r <= gx && gx <= x + r && y - r <= gy && gy <= y + r)
                {
                    cLocalField.Potentials[(gy - y + r) * s + (gx - x + r)] = 1;
                    cLocalField.Fixed[(gy - y + r) * s + (gx - x + r)] = true;
                    continue;
                }
                
                // 3) Отмечаем F-зону, как проходимую
                // F-зона это граница с радиусом r-1
                int fR = r - 1;
                for (int dy = -fR; dy <= fR; dy++)
                for (int dx = -fR; dx <= fR; dx++)
                {
                    if (dx == -fR || dx == fR || dy == -fR || dy == fR)
                    {
                        cLocalField.Fixed[(dy + r) * s + (dx + r)] = false;
                    }
                }
                
                // 4) Отметим промежуточную цель и соседние с ней клетки
                // Берем глобальный градиент и определяем клетку на границе окна по нему
                double pl = x > 0 ? cField.Potentials[x - 1 + y * w] : double.MaxValue;
                double pr = x < w - 1 ? cField.Potentials[x + 1 + y * w] : double.MaxValue;
                double pt = y > 0 ? cField.Potentials[x + (y - 1) * w] : double.MaxValue;
                double pb = y < h - 1 ? cField.Potentials[x + (y + 1) * w] : double.MaxValue;
                
                double2 gradient = math.normalize(new double2(pr - pl, pb - pt));

                int goalX = 0;
                int goalY = 0;
                
                // Определим цель на границе окна
                // Обработаем 4 случая на углах
                if (math.abs(gradient.x - gradient.y) < 1e-4f)
                {
                    if (gradient.x > 0)
                    {
                        if (gradient.y > 0)
                        {
                            goalX = s - 1;
                            goalY = s - 1;
                        }
                        else
                        {
                            goalX = s - 1;
                            goalY = 0;
                        }
                    }
                    else
                    {
                        if (gradient.y > 0)
                        {
                            goalX = 0;
                            goalY = s - 1;
                        }
                        else
                        {
                            goalX = 0;
                            goalY = 0;
                        }
                    }
                }
                // Иначе ищем пересечения
                else
                {
                    // Пересечение с верхней границей если градиент направлен вверх и сильнее чем вправо или влево
                    if (gradient.y > 0 && math.abs(gradient.x) < math.abs(gradient.y))
                    {
                        goalX = s / 2;
                        goalY = s - 1;
                    }
                    // Пересечение с нижней границей если градиент направлен вниз и сильнее чем вправо или влево
                    else if (gradient.y < 0 && math.abs(gradient.x) < math.abs(gradient.y))
                    {
                        goalX = s / 2;
                        goalY = 0;
                    }
                    // Пересечение с левой границей если градиент направлен влево и сильнее чем вверх или вниз
                    else if (gradient.x < 0 && math.abs(gradient.y) < math.abs(gradient.x))
                    {
                        goalX = 0;
                        goalY = s / 2;
                    }
                    // Пересечение с правой границей если градиент направлен вправо и сильнее чем вверх или вниз
                    else
                    {
                        goalX = s - 1;
                        goalY = s / 2;
                    }
                }
                
                cLocalField.Potentials[goalY * s + goalX] = 1;
                cLocalField.Fixed[goalY * s + goalX] = true;
                
                // 6) Вызовем метод Гаусса-Зейделя для расчета значений потенциала в клетках
                var potentialsSpan = cLocalField.Potentials.AsSpan();
                var fixedSpan = cLocalField.Fixed.AsSpan();
                
                Utils.GaussSeidel(
                    ref potentialsSpan,
                    ref fixedSpan,
                    s,
                    s,
                    1e-4f,
                    double2.zero,
                    cLocalField.Epsilon,
                    100
                    );
            }
        }
    }
}