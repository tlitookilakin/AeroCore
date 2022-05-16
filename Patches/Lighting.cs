﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AeroCore.Utils;
using AeroCore.Models;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Reflection.Emit;
using Microsoft.Xna.Framework.Graphics;

namespace AeroCore.Patches
{
    [HarmonyPatch]
    internal class Lighting
    {
        internal static event Action<LightingEventArgs> LightingChanged;
        private static Vector2 offset;
        private static Vector2 v_offset;
        public static MethodBase TargetMethod() => AccessTools.TypeByName("StardewModdingAPI.Framework.SGame").MethodNamed("DrawImpl");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => patcher.Run(instructions);

        private static ILHelper patcher = new ILHelper(ModEntry.monitor, "Lighting")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Call, typeof(Game1).MethodNamed("get_lightmap")),
                new(OpCodes.Callvirt, typeof(Viewport).MethodNamed("get_Bounds"))
            })
            .Skip(2)
            .Transform(InjectEvent)
            .Remove(1)
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldfld, typeof(Options).FieldNamed("lightingQuality")),
                new(OpCodes.Ldc_I4_2),
                new(OpCodes.Div),
                new(OpCodes.Conv_R4)
            })
            .Skip(4)
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldsfld, typeof(Lighting).FieldNamed(nameof(v_offset))),
                new(OpCodes.Call, typeof(Vector2).MethodNamed("op_Addition"))
            })
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Call,typeof(Game1).MethodNamed("get_lightmap")),
                new(OpCodes.Call,typeof(Vector2).MethodNamed("get_Zero"))
            })
            .Skip(1)
            .Remove(1)
            .Add(new CodeInstruction(OpCodes.Ldsfld, typeof(Lighting).FieldNamed(nameof(offset))))
            .Finish();

        internal static IList<CodeInstruction> InjectEvent(ILHelper.ILEnumerator cursor)
        {
            int box = ((LocalBuilder)cursor.Current.operand).LocalIndex;
            return new CodeInstruction[] {
                cursor.Current,
                cursor.source.GetNext(),
                new(OpCodes.Ldloc_S, box),
                new(OpCodes.Ldloc_S, box + 1),
                new(OpCodes.Call, typeof(Lighting).MethodNamed(nameof(EmitEvent)))
            };
        }

        internal static void EmitEvent(Color ambient, float intensity)
        {
            GetOffset();
            LightingChanged.Invoke(new(intensity, ambient, v_offset));
        }
        internal static void GetOffset()
        {
            int pixsize = Game1.options.lightingQuality / 2;
            var pos = Game1.viewport.Location;
            offset = new(-(pos.X % pixsize), -(pos.Y % pixsize));
            v_offset = new(offset.X / (float)pixsize, offset.Y / (float)pixsize);
        }
    }
}