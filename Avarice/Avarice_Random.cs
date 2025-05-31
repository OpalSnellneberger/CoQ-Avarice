using System;
using XRL;
using XRL.Core;
using XRL.Rules;

namespace Avarice.Utilities
{
    [HasGameBasedStaticCache]
    public static class Avarice_Random
    {
        private static Random _rand;
        public static Random Rand
        {
            get
            {
                if (_rand == null)
                {
                    if (XRLCore.Core?.Game == null)
                    {
                        throw new Exception("Avarice mod attempted to retrieve Random, but Game is not created yet.");
                    }
                    else if (XRLCore.Core.Game.IntGameState.ContainsKey("Avarice:Random"))
                    {
                        int seed = XRLCore.Core.Game.GetIntGameState("Avarice:Random");
                        _rand = new Random(seed);
                    }
                    else
                    {
                        _rand = Stat.GetSeededRandomGenerator("Avarice");
                    }
                    XRLCore.Core.Game.SetIntGameState("Avarice:Random", _rand.Next());
                }
                return _rand;
            }
        }

        [GameBasedCacheInit]
        public static void ResetRandom()
        {
            _rand = null;
        }

        public static int Next(int minInclusive, int maxInclusive)
        {
            return Rand.Next(minInclusive, maxInclusive + 1);
        }
    }
}