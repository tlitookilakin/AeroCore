using AeroCore.Models;
using AeroCore.Utils;
using HarmonyLib;
using StardewValley;
using System;
using SObject = StardewValley.Object;
using System.Collections.Generic;
using System.Reflection.Emit;
using AeroCore.API;

namespace AeroCore.Patches
{
    [HarmonyPatch(typeof(Game1))]
    internal class UseItem
    {
        public static event Action<IUseItemEventArgs> OnUseItem;

        [HarmonyPatch(nameof(Game1.pressActionButton))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> UseObject(IEnumerable<CodeInstruction> codes, ILGenerator gen) => objectUsePatch.Run(codes, gen);

        [HarmonyPatch(nameof(Game1.pressUseToolButton))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> UseTool(IEnumerable<CodeInstruction> codes, ILGenerator gen) => toolUsePatch.Run(codes, gen);

        private static readonly ILHelper objectUsePatch = new ILHelper(ModEntry.monitor, "Use Item: Object")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Call, typeof(Game1).PropertyGetter(nameof(Game1.player))),
                new(OpCodes.Callvirt, typeof(Farmer).PropertyGetter(nameof(Farmer.ActiveObject))),
                new(OpCodes.Call, typeof(Game1).PropertyGetter(nameof(Game1.currentLocation))),
                new(OpCodes.Callvirt, typeof(SObject).MethodNamed(nameof(SObject.performUseAction)))
            })
            .Add(
                new CodeInstruction(OpCodes.Call,typeof(UseItem).MethodNamed(nameof(ActivateObject)))
            )
            .AddJump(OpCodes.Brtrue, "activated")
            .Skip(5)
            .AddLabel("activated")
            .Finish();

        private static readonly ILHelper toolUsePatch = new ILHelper(ModEntry.monitor, "Use Item: Tool")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Call, typeof(Game1).PropertyGetter(nameof(Game1.player))),
                new(OpCodes.Callvirt, typeof(Farmer).PropertyGetter(nameof(Farmer.CurrentTool))),
                new(OpCodes.Call, typeof(Game1).PropertyGetter(nameof(Game1.player))),
                new(OpCodes.Callvirt, typeof(Character).PropertyGetter(nameof(Character.currentLocation))),
                new(OpCodes.Call, typeof(Game1).PropertyGetter(nameof(Game1.player))),
                new(OpCodes.Ldflda, typeof(Character).FieldNamed(nameof(Character.lastClick)))
            })
            .Add(
                new CodeInstruction(OpCodes.Call, typeof(UseItem).MethodNamed(nameof(ActivateTool)))
            )
            .AddJump(OpCodes.Brtrue, "activated")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ret)
            })
            .AddLabel("activated")
            .Finish();

        private static bool ActivateObject()
        {
            var who = Game1.player;
            if(!who.canMove || who.ActiveObject.isTemporarilyInvisible)
                return false;
            var ev = new UseItemEventArgs(false, who.ActiveObject);
            OnUseItem?.Invoke(ev);
            return ev.IsHandled;
        }
        private static bool ActivateTool()
        {
            var ev = new UseItemEventArgs(true, Game1.player.CurrentTool);
            OnUseItem?.Invoke(ev);
            return ev.IsHandled;
        }
    }
}
