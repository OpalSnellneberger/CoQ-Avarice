using System;
using System.Collections.Generic;
using Avarice.Utilities;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class Mutation_Avarice : BaseMutation
    {

        public const string COMMAND = "CommandAvarice";

        public string Sound = "Sounds/Abilities/sfx_ability_mutation_mental_generic_activate";

        public Guid AvariceActivatedAbilityID = Guid.Empty;

        public override bool CanLevel()
        {
            return false;
        }

        public override string GetDescription()
        {
            return "";
        }

        public override string GetLevelText(int Level)
        {
            return "Fill your heart with Greed, and let your desires flourish!";
        }

        public int GetCooldown(int level)
        {
            return Avarice.Utilities.Options.StealCooldown;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID)
            {
                return ID == CommandEvent.ID; // How is this shit supposed to work?
            }

            return true;
        }

        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND)
            {
                Cast(E.Target, E.TargetCell);
            }

            return base.HandleEvent(E);
        }

        public bool Cast(GameObject Target = null, Cell TargetCell = null)
        {
            if (!this.ParentObject.IsPlayer())
            {
                return false;
            }

            if (TargetCell == null)
            {
                TargetCell = Target?.CurrentCell ?? PickDirection(ForAttack: true, "Avarice");
            }

            if (TargetCell == null)
            {
                return false;
            }

            string FailureMessage = null;
            if (Target != null)
            {
                if (ProcessTarget(Target, ref FailureMessage))
                {
                    return true;
                }
            }
            else
            {
                foreach (GameObject item in TargetCell.GetObjectsWithPart("Combat"))
                {
                    if (ProcessTarget(item, ref FailureMessage))
                    {
                        return true;
                    }
                }
            }

            if (!FailureMessage.IsNullOrEmpty())
            {
                ParentObject.Fail(FailureMessage);
            }

            return false;
        }

        public bool ProcessTarget(GameObject Target, ref string FailureMessage)
        {
            if (Target.HasCopyRelationship(ParentObject))
            {
                FailureMessage = "You can't steal from " + ParentObject.itself + "!";
                return false;
            }

            ParentObject?.PlayWorldSound(Sound);
            PerformMentalAttack(MentalAvariceAttack, ParentObject, Target, null, "Avarice");
            UseEnergy(1000, "Mental Mutation Avarice");
            CooldownMyActivatedAbility(AvariceActivatedAbilityID, GetCooldown(base.Level));
            return true;
        }

        public bool MentalAvariceAttack(MentalAttackEvent E)
        {
            var roll = Avarice_Random.Next(0, 99);

            if (roll < Avarice.Utilities.Options.StealMutationChance)
            {
                StealMutation(E.Defender, E.Attacker);
            }

            roll = Avarice_Random.Next(0, 99);
            if (roll < Avarice.Utilities.Options.StealAttributeChance)
            {
                StealAttribute(E.Defender, E.Attacker);
            }

            return true;
        }

        public bool StealMutation(GameObject Sender, GameObject Receiver)
        {

            if (Sender.TryGetPart<Mutations>(out var SenderMutationPart))
            {
                if (Receiver.TryGetPart<Mutations>(out var ReceiverMutationPart))
                {
                    var SenderMutationList = SenderMutationPart.MutationList;
                    List<BaseMutation> ValidMutations = new List<BaseMutation>();
                    List<string> ValidMutationNames = new List<string>();

                    // Get valid mutations for the list
                    foreach (var Mutation in SenderMutationList)
                    {
                        if (MutationsAPI.IsNewMutationValidFor(Receiver, MutationFactory.CreateMutationEntryForMutation(Mutation)) && !ReceiverMutationPart.HasMutation(Mutation))
                        {
                            // New Mutation, and its valid!
                            ValidMutations.Add(Mutation);
                            ValidMutationNames.Add(Mutation.GetDisplayName());
                        }
                        else if (ReceiverMutationPart.HasMutation(Mutation))
                        {
                            // We have the mutation, check if it can go past level 1 to be valid
                            if (Mutation.CanLevel() && ReceiverMutationPart.GetMutation(Mutation.Name).Level < Mutation.GetMaxLevel())
                            {
                                // We have it, and it can level!
                                ValidMutations.Add(Mutation);
                                ValidMutationNames.Add(Mutation.GetDisplayName());
                            }
                        }
                    }

                    if( ValidMutations.Count == 0 )
                    {
                        XRL.Messages.MessageQueue.AddPlayerMessage("No mutations to steal!");

                        return false;
                    }

                    BaseMutation SelectedMutation = ValidMutations[0];

                    if (Avarice.Utilities.Options.StealMutationAtRandom)
                    {
                        SelectedMutation = ValidMutations[Avarice_Random.Next(0, ValidMutations.Count - 1)];
                    }
                    else if (ValidMutations.Count != 1)
                    {
                        var Selection = Popup.PickOption("Avarice", "You reach out and the following mutations are within your grasp. Choose a mutation to steal:", Options: ValidMutationNames.AsReadOnly());
                        SelectedMutation = ValidMutations[Selection];
                    }

                    if (MutationsAPI.IsNewMutationValidFor(Receiver, MutationFactory.CreateMutationEntryForMutation(SelectedMutation)) && !ReceiverMutationPart.HasMutation(SelectedMutation))
                    {
                        if (Popup.ShowYesNo("Do you want the mutation " + SelectedMutation.GetDisplayName() + "?") == DialogResult.No)
                        {
                            return false;
                        }

                        // Decrease Mutation level in target
                        if (SelectedMutation.Level == 1)
                        {
                            SenderMutationPart.RemoveMutation(SelectedMutation);
                        }
                        else
                        {
                            SenderMutationPart.LevelMutation(SelectedMutation, SelectedMutation.Level - 1);
                        }

                        // Add Mutation to self
                        ReceiverMutationPart.AddMutation(SelectedMutation);

                        return true;
                    }
                    else if (ReceiverMutationPart.HasMutation(SelectedMutation))
                    {
                        // Decrease Mutation level in target
                        if (SelectedMutation.Level == 1)
                        {
                            SenderMutationPart.RemoveMutation(SelectedMutation);
                        }
                        else
                        {
                            SenderMutationPart.LevelMutation(SelectedMutation, SelectedMutation.Level - 1);
                        }

                        // Increase Mutation level for receiver
                        ReceiverMutationPart.LevelMutation(SelectedMutation, ReceiverMutationPart.GetMutation(SelectedMutation.Name).Level + 1);

                        Popup.Show(SelectedMutation.GetDisplayName() + " is now level {{G|" + ReceiverMutationPart.GetMutation(SelectedMutation.Name).Level + "}}!");

                        return true;
                    }
                    else
                    {
                        XRL.Messages.MessageQueue.AddPlayerMessage(SelectedMutation.GetDisplayName() + " is an invalid mutation to steal. :(");
                    }
                }
            }

            return false;
        }

        public bool StealAttribute(GameObject Sender, GameObject Receiver)
        {
            int statRoll = Avarice_Random.Next(1, 6);
            int amountRoll = Avarice_Random.Next(1, 100);

            string chosenStat = "Strength";
            int amount = 1;

            if (amountRoll < 10)
            {
                amount++;
            }
            if (amountRoll < 5)
            {
                amount++;
            }
            if (amountRoll < 2)
            {
                amount++;
            }

            switch (statRoll)
            {
                case 2:
                    chosenStat = "Intelligence";
                    break;
                case 3:
                    chosenStat = "Willpower";
                    break;
                case 4:
                    chosenStat = "Agility";
                    break;
                case 5:
                    chosenStat = "Toughness";
                    break;
                case 6:
                    chosenStat = "Ego";
                    break;
            }

            Sender.Statistics[chosenStat].BaseValue -= amount;
            Receiver.Statistics[chosenStat].BaseValue += amount;

            if (Sender.Stat(chosenStat) <= 1)
            {
                Sender.Die(Receiver, null, "Avarice is a ferocious thing. Your future was stolen from you by one consumed with greed.");
            }

            Popup.Show("Your " + chosenStat + " is increased by {{G|" + amount + "}}!");

            return true;
        }

        private void AddAbility()
        {
            this.AvariceActivatedAbilityID = base.AddMyActivatedAbility("Avarice", COMMAND, "Mental Mutations", GetDescription(), IsAttack:true);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.Unmutate(GO);
            this.AddAbility();
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            base.RemoveMyActivatedAbility(ref this.AvariceActivatedAbilityID, null);
            return base.Unmutate(GO);
        }
    }
}
