using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using System;

namespace rimtrample
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		private static readonly FieldInfo pawnField = AccessTools.Field (typeof(PawnFootprintMaker), "pawn");
		private static Dictionary<Pawn, IntVec3> pawnPrevPos = new Dictionary<Pawn, IntVec3> ();

		// this static constructor runs to create a HarmonyInstance and install a patch.
		static HarmonyPatches ()
		{
			Log.Message ("trample patch start");
			HarmonyInstance harmony = HarmonyInstance.Create ("rimworld.undera4.unificamagica");

			MethodInfo targetmethod = AccessTools.Method (typeof(RimWorld.PawnFootprintMaker), "FootprintMakerTick");
			HarmonyMethod hook = new HarmonyMethod (typeof(rimtrample.HarmonyPatches).GetMethod ("Hook"));
			harmony.Patch (targetmethod, null, hook);

			MethodInfo destructor = AccessTools.Method (typeof(Pawn), "Destroy");
			HarmonyMethod hookD = new HarmonyMethod (typeof(rimtrample.HarmonyPatches).GetMethod ("DeleteFromMap"));
			harmony.Patch (destructor, hookD, null);

			Log.Message ("trample patch end");
		}

		public static void Hook (PawnFootprintMaker __instance)
		{
			Pawn pawn = (Pawn)pawnField.GetValue (__instance);
			if (PawnMoved (__instance, pawn)) { // constant is just copied from PawnFootprintMaker

				Plant plant = pawn.Position.GetPlant (pawn.Map);
				if (plant != null) {
					int damage = (int)Math.Floor (pawn.GetStatValue (StatDefOf.Mass, true) / 20);
					if (damage > 0) {
						plant.TakeDamage (new DamageInfo (DamageDefOf.Crush, damage, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
						if (plant.HitPoints <= 0) {
							Log.Message (pawn.ToString () + " has damaged plant completely: "+plant.ToString());
						} else {
							//Log.Message (pawn.ToString () + " has damaged plant " + plant.ToString () + " [" + plant.HitPoints + "/" + plant.MaxHitPoints + "]");
						}
					}
				}
			}
			pawnPrevPos [pawn] = pawn.Position;
		}

		public static void DeleteFromMap (Pawn __instance)
		{
			Log.Message ("Removing from map: " + __instance.ToString ());
			pawnPrevPos.Remove (__instance);
		}

		private static bool PawnMoved (PawnFootprintMaker footMaker, Pawn pawn)
		{
			//Log.Message (pawn.ToString () + " is at " + pawn.Position.ToString ());
			if (!pawnPrevPos.ContainsKey (pawn)) {
				pawnPrevPos [pawn] = pawn.Position;
			}

			return pawnPrevPos [pawn] != pawn.Position;
		}
	}
}

// TODO: no damage to trees?
// TODO: no damage from pawn that makes agriculture or cuts plants, or returns from it (!)