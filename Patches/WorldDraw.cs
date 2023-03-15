using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AeroCore.Patches
{
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
	internal class WorldDraw
	{
		[HarmonyPostfix]
		internal static void EmitDraw(SpriteBatch b)
		{
			ModEntry.api?.EmitWorldDraw(b);
		}
	}
}
