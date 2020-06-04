using HarmonyLib;
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
		private static readonly FieldInfo pawnField = AccessTools.Field(typeof(PawnFootprintMaker), "pawn");
		private static readonly FieldInfo growthIntField = AccessTools.Field(typeof(Plant), "growthInt");
		private static readonly PropertyInfo growthPerTickField = AccessTools.Property(typeof(Plant), "GrowthPerTick");
		private static Dictionary<Pawn, IntVec3> pawnPrevPos = new Dictionary<Pawn, IntVec3>();

		// this static constructor runs to create a HarmonyInstance and install a patch.
		static HarmonyPatches()
		{
			Log.Message("trample patch start");
			Harmony harmony = new Harmony("rimworld.undera4.unificamagica");

			MethodInfo targetmethod = AccessTools.Method(typeof(RimWorld.PawnFootprintMaker), "FootprintMakerTick");
			HarmonyMethod hookA = new HarmonyMethod(typeof(rimtrample.HarmonyPatches).GetMethod("Hook"));
			harmony.Patch(targetmethod, null, hookA);

			MethodInfo destructor = AccessTools.Method(typeof(Pawn), "Destroy");
			HarmonyMethod hookB = new HarmonyMethod(typeof(rimtrample.HarmonyPatches).GetMethod("DeleteFromMap"));
			harmony.Patch(destructor, hookB, null);

			MethodInfo plantTick = AccessTools.Method(typeof(Plant), "TickLong");
			HarmonyMethod hookC = new HarmonyMethod(typeof(rimtrample.HarmonyPatches).GetMethod("PlantTickLong"));
			harmony.Patch(plantTick, hookC, null);

			Log.Message("trample patch end");
		}

		public static void Hook(PawnFootprintMaker __instance)
		{
			Pawn pawn = (Pawn)pawnField.GetValue(__instance);
			if (PawnMoved(__instance, pawn))
			{ // constant is just copied from PawnFootprintMaker

				Plant plant = pawn.Position.GetPlant(pawn.Map);
				if (plant != null && pawn.CurJob.def != JobDefOf.Sow && pawn.CurJob.def != JobDefOf.Harvest && pawn.CurJob.def != JobDefOf.CutPlant)
				{
					int damage = (int)Math.Floor(pawn.GetStatValue(StatDefOf.Mass, true) / 2f);
					if (damage > 0)
					{
						plant.TakeDamage(new DamageInfo(DamageDefOf.Rotting, damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
						if (plant.HitPoints <= 0)
						{
							Log.Message (pawn.ToString () + " has damaged plant completely: " + plant);
						}
						else
						{
							//Log.Message (pawn.ToString () + " has damaged plant " + plant.ToString () + " [" + plant.HitPoints + "/" + plant.MaxHitPoints + "]");
						}
					}
				}
			}
			pawnPrevPos[pawn] = pawn.Position;
		}

		public static void DeleteFromMap(Pawn __instance)
		{
			//Log.Message ("Removing from map: " + __instance.ToString ());
			pawnPrevPos.Remove(__instance);
		}

		private static bool PawnMoved(PawnFootprintMaker footMaker, Pawn pawn)
		{
			//Log.Message (pawn.ToString () + " is at " + pawn.Position.ToString ());
			if (!pawnPrevPos.ContainsKey(pawn))
			{
				pawnPrevPos[pawn] = pawn.Position;
			}

			return pawnPrevPos[pawn] != pawn.Position;
		}

		public static void PlantTickLong(Plant __instance)
		{
			Plant plant = __instance;
			if (PlantUtility.GrowthSeasonNow(plant.Position, plant.Map))
			{
				if (plant.HitPoints < plant.MaxHitPoints && plant.def != ThingDefOf.BurnedTree)
				{
					// Log.Message (plant+" before: "+plant.HitPoints.ToString() +"/"+growthIntField.GetValue (plant));

					// heal up
					plant.HitPoints += (int)Math.Ceiling(plant.HitPoints / 500f);
                    if (plant.HitPoints > plant.MaxHitPoints)
					{
                        Log.Message(plant + " fully healed up: " + plant.def);
                        plant.HitPoints = plant.MaxHitPoints;
					}

					// less growth
					float num = (float)growthIntField.GetValue(plant);
					float newNum = num - (float)growthPerTickField.GetValue(plant, null) * 100f;
					growthIntField.SetValue(plant, newNum); // took half growth constant from Plant.cs
                    // Log.Message (plant+" after: "+plant.HitPoints.ToString() +"/"+growthIntField.GetValue (plant));
				}
			}
		}
	}

}

// TODO: no damage from pawn that _returns_ from agriculture operations?
