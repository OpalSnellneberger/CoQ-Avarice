using System;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class Mutation_Genesight : BaseMutation
    {

        public static readonly string COMMAND_NAME = "CommandGenesight";

        public bool RealityDistortionBased = false;

        public override bool CanLevel()
        {
            return false;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade))
            {
                return ID == PooledEvent<CommandEvent>.ID;
            }

            return true;
        }

        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND_NAME)
            {

                SoundManager.PreloadClipSet("Sounds/Abilities/sfx_ability_telepathy");
                Cell cell = PickDestinationCell(Avarice.Utilities.Options.GenesightRange, AllowVis.OnlyExplored, Locked: true, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Genesight");

                if (cell != null && cell.ManhattanDistanceTo(ParentObject.CurrentCell) <= Avarice.Utilities.Options.GenesightRange)
                {
                    ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_telepathy");
                    cell.PlayWorldSound("Sounds/Abilities/sfx_ability_telepathy");

                    cell.ForeachObjectWithPart("Mutations", delegate (GameObject GO)
                    {
                        if (GO.TryGetPart<Mutations>(out var MutationsPart))
                        {
                            int mutationCount = MutationsPart.MutationList.Count;

                            if ( mutationCount > 1)
                            {
                                // Show popup list of mutations held by the part
                                string result = GO.GetDisplayName() + " has the following mutations: \n";
                                foreach (BaseMutation mutation in MutationsPart.MutationList)
                                {
                                    result += "\n" + mutation.GetDisplayName() + " ({{G|" + mutation.Level + "}})";
                                }
                                Popup.Show(result);
                            }
                            else if (mutationCount == 1)
                            {
                                var Mutation = MutationsPart.MutationList[0];
                                Popup.Show(GO.GetDisplayName() + " has the mutation " + Mutation.GetDisplayName() + " ({{G|" + Mutation.Level + "}})");
                            }
                            else
                            {
                                Popup.Show(GO.GetDisplayName() + " has no mutations.");
                            }
                        }
                    });
                }
            }

            return base.HandleEvent(E);
        }

        public override string GetDescription()
        {
            return "You may discern the genetic code of any you perceive.";
        }

        public override string GetLevelText(int Level)
        {
            return "";
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilityID = AddMyActivatedAbility("Genesight", COMMAND_NAME, "Mental Mutations", null, "\u000e", null, IsRealityDistortionBased: RealityDistortionBased);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.Unmutate(GO);
        }
    }
}