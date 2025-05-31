using System;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class Mutation_QuietMind : BaseMutation
    {

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
        }

        public override string GetDescription()
        {
            return "Your mind is hushed, hiding your presence from psychic hunters.";
        }

        public override string GetLevelText(int Level)
        {

            return "Your Glimmer level is decreased by {{G|" + (int)(100f * GetGlimmerMultiplier(Level)) + "}}%.\n";
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade))
            {
                return ID == GetPsychicGlimmerEvent.ID;
            }

            return true;
        }

        public float GetGlimmerMultiplier(int Level)
        {
            return 1 - (1 / (1 + 0.3f * Level));
        }

        public override bool HandleEvent(GetPsychicGlimmerEvent E)
        {
            if (ParentObject.IsPlayer())
            {
                int num = (int)(E.Base * GetGlimmerMultiplier(this.Level));
                E.Base = num;
                E.Level = num;
            }

            return base.HandleEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            return base.Unmutate(GO);
        }

    }
}