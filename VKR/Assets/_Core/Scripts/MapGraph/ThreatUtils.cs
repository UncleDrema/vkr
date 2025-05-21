using System;
using Unity.Mathematics;

namespace Game.MapGraph
{
    public static class ThreatUtils
    {
        /** У формулы силы угрозы есть параметры:
         a - сила угрозы
         d - длительность угрозы
         x - время существования угрозы
         общая формула:
         y = a * exp(-(1/100*((kx/d)^c)))
         где 1/100 - масштабирующий коэффициент,
         k - коэффициент, определяющий скорость убывания угрозы,
         c - коэффициент, определяющий плоскость графика убывания угрозы,

         Есть 5 типов убывания угрозы:
         Держится и падает когда осталось около 75% (DecayAfter75Percent): k = 1.5, c = 16
         Держится и падает когда осталось около 50% (DecayAfter50Percent): k = 2, c = 8
         Держится и падает когда осталось около 25% (DecayAfter25Percent): k = 4, c = 4
         Сразу начинает плавно падать (DecayImmediatelySlow): k = 22, c = 2
         Сразу начинает быстро падать (DecayImmediatelyFast): k = 512, c = 1
         с - всегда степень двойки, так что можно применить быстрое возведение в степень двойки
        */
        public static float GetThreatLevel(float threatPower, float threadDuration, float existenceTime, ThreatDecayType decayType)
        {
            float a = threatPower;
            float d = threadDuration;
            float x = existenceTime;
            
            float k = 0;
            int c = 0;
            switch (decayType)
            {
                case ThreatDecayType.DecayAfter75Percent:
                    k = 1.5f;
                    c = 16;
                    break;
                case ThreatDecayType.DecayAfter50Percent:
                    k = 2f;
                    c = 8;
                    break;
                case ThreatDecayType.DecayAfter25Percent:
                    k = 4f;
                    c = 4;
                    break;
                case ThreatDecayType.DecayImmediatelySlow:
                    k = 22f;
                    c = 2;
                    break;
                case ThreatDecayType.DecayImmediatelyFast:
                    k = 512f;
                    c = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(decayType), decayType, null);
            }

            float expArg = k * x / d;
            c /= 2;
            while (c > 0)
            {
                expArg *= expArg;
                c /= 2;
            }

            expArg *= -1f / 100f;
            float expValue = math.exp(expArg);
            float threatLevel = a * expValue;
            return threatLevel;
        }
    }
}