using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using Verse.AI;

namespace Neyot.Adoption
{
    public class JobDriver_AdoptChild : JobDriver
    {
        private readonly int adoptingDuration = 500;

        private Pawn targetAdopter = null;

        private Pawn targetAdoptee = null;

        public Pawn Adopter
        {
            get
            {
                if (targetAdopter == null)
                {
                    targetAdopter = TargetThingA as Pawn;
                }

                return targetAdopter;
            }
        }

        public Pawn Adoptee
        {
            get
            {
                if (targetAdoptee == null)
                {
                    targetAdoptee = TargetThingB as Pawn;
                }

                return targetAdoptee;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Try to reserve Adopter (parent/adult adopting the pawn).
            if (!pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed))
            {
                return false;
            }

            // Try to reserve Adoptee (pawn being adopted).
            if (!pawn.Reserve(TargetB, job, errorOnFailed: errorOnFailed))
            {
                return false;
            }

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var adoptee = TargetIndex.A;
            var adopter = TargetIndex.B;

            this.FailOnDespawnedOrNull(adopter);
            this.FailOnDespawnedOrNull(adoptee);

            // Adopter go to Adoptee
            yield return Toils_Goto.GotoThing(adoptee, PathEndMode.Touch);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.Adopter);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.Adoptee);
            yield return Toils_Interpersonal.GotoInteractablePosition(adoptee);

            // It's not possible to set the social mode in the yield return statement so,
            // first we define the Toil, and then assign the mode while we return it.
            Toil gotoTarget = Toils_Goto.GotoThing(adoptee, PathEndMode.Touch);
            gotoTarget.socialMode = RandomSocialMode.Off;

            // There are two wait toils: Wait simply stops the pawn for an amount of ticks. 
            // The fancy WaitWith we're using has optional parameters like a progress bar and a TargetIndex.
            Toil wait = Toils_General.WaitWith(adoptee, adoptingDuration, true, true);
            wait.socialMode = RandomSocialMode.Off;

            // Sometimes a Toil must be converted into a delegate. 
            // Since we can't directly call non-Toil functions or methods inside the IEnumerable,
            // we call on Toils_General.Do. There are other options for this, see the next chapter for more.
            yield return Toils_General.Do(() => 
            {
                this.Adopter.interactions.TryInteractWith(this.Adoptee, InteractionDefOf.Chitchat);
            });
        }
    }
}
