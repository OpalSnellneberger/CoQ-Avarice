using System;

namespace Avarice.Utilities
{
    public static class Options
    {
        private static string GetOption(string ID, string Default = "")
        {
            return XRL.UI.Options.GetOption(ID, Default: Default);
        }

        public static int StealAttributeChance => Convert.ToInt32(GetOption("Option_BlackSabin_Avarice_Steal_Chance_Attribute"));
        public static int StealMutationChance => Convert.ToInt32(GetOption("Option_BlackSabin_Avarice_Steal_Chance_Mutation"));
        public static bool StealMutationAtRandom => GetOption("Option_BlackSabin_Avarice_Steal_Mutation_Random") == "Yes" ?  true : false;
        public static int StealCooldown => Convert.ToInt32(GetOption("Option_BlackSabin_Avarice_Steal_Cooldown"));

        public static int GenesightRange => Convert.ToInt32(GetOption("Option_BlackSabin_Avarice_Genesight_Range"));

    }
}