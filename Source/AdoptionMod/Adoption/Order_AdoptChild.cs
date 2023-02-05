using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Neyot.Adoption
{
    [StaticConstructorOnStartup]
    public class Order_AdoptChild
    {

        private static TargetingParameters targetParametersBody = null;

        static Order_AdoptChild()
        {
            Harmony harmony = new Harmony("Neyot.Adoption");
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), prefix: null,
                postfix: new HarmonyMethod(typeof(Order_AdoptChild), nameof(AdoptChildFloatMenuOption)));
        }

        public static TargetingParameters TargetParametersBody
        {
            get
            {
                if (targetParametersBody == null)
                {
                    targetParametersBody = new TargetingParameters()
                    {
                        canTargetPawns = true,
                        canTargetItems = true,
                        mapObjectTargetsMustBeAutoAttackable = false,
                        validator = (TargetInfo target) =>
                        {
                            if (!target.HasThing) return false;

                            if (target.Thing is Pawn pawn)
                            {
                                return IsPawnAdoptable(pawn);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    };
                }
                return targetParametersBody;
            }
        }

        private static void AdoptChildFloatMenuOption(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            // If the pawn in question cannot take jobs, don't bother.
            if (pawn.jobs == null) return;

            // Find valid children.
            foreach (LocalTargetInfo targetBody in GenUI.TargetsAt(clickPos, TargetParametersBody))
            {
                // Ensure target is reachable.
                if (!pawn.CanReach(targetBody, PathEndMode.ClosestTouch, Danger.Some))
                {
                    //option = new FloatMenuOption("CannotDress".Translate(targetBody.Thing.LabelCap, targetBody.Thing) + " (" + "NoPath".Translate() + ")", null);
                    continue;
                }

                // Add menu option to dress patient. User will be asked to select a target.
                FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Adopt Child", delegate () // TODO: Translate... "string".Translate()
                {
                    pawn.jobs.TryTakeOrderedJob(new Job(DefDatabase<JobDef>.GetNamed("AdoptChild", false), targetBody));
                }, MenuOptionPriority.High), pawn, targetBody);
                opts.Add(option);
            }
        }

        private static bool IsPawnAdoptable(Pawn pawn)
        {
            // If any of the Birth Mother, Mother, or Father are colonists then pawn is not adoptable.
            return pawn != null &&
                (pawn.DevelopmentalStage == DevelopmentalStage.Newborn ||
                pawn.DevelopmentalStage == DevelopmentalStage.Baby ||
                pawn.DevelopmentalStage == DevelopmentalStage.Child) &&
                pawn.IsColonist &&
                (!pawn.GetBirthParent()?.IsColonist ?? true) &&
                (!pawn.GetMother()?.IsColonist ?? true) &&
                (!pawn.GetFather()?.IsColonist ?? true);
        }
    }
}
