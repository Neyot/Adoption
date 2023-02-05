using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using Verse;

namespace Neyot.Adoption
{
    [StaticConstructorOnStartup]
    public class Adoption
    {
        private static Harmony harmony;

        static Adoption() {
            harmony = new Harmony("Neyot.Adoption");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
