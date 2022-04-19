using AeroCore.Generics;
using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using AeroCore.Utils;
using System.Linq;

namespace AeroCore
{
    internal class ILHelper
    {
        public readonly string name;
        public ILGenerator Generator => currentGenerator;

        private enum ActionType { SkipTo, RemoveTo, Skip, RemoveCount, Transform, Add, Finish, Collect };
        public delegate IEnumerable<CodeInstruction> Transformer(IList<CodeInstruction> instructions);

        private readonly List<(ActionType action, object arg)> actionQueue = new();
        public IList<CodeInstruction> collected;
        private IEnumerable<CodeInstruction> instructions;
        private BufferedEnumerator<CodeInstruction> cursor;
        private int actionIndex = 0;
        private bool hasErrored = false;
        private ILGenerator currentGenerator = null;
        private IMonitor monitor;

        /// <summary>Creates a new ILHelper</summary>
        /// <param name="Name">The name displayed in the log</param>
        /// <param name="Monitor">Your mod's monitor</param>
        public ILHelper(string Name, IMonitor Monitor)
        {
            name = Name;
            collected = new List<CodeInstruction>();
            monitor = Monitor;
        }

        /// <summary>Skip to an instruction set</summary>
        /// <param name="codes">The instruction set</param>
        /// <returns>This</returns>
        public ILHelper Skip(IList<CodeInstruction> codes)
        {
            actionQueue.Add((ActionType.SkipTo, codes));
            return this;
        }

        /// <summary>Skip to an instruction</summary>
        /// <param name="code">The instruction</param>
        /// <returns>This</returns>
        public ILHelper Skip(CodeInstruction code) => Skip(new[] { code });

        /// <summary>Skip a number of instructions</summary>
        /// <param name="Count">How many instructions to skip</param>
        /// <returns>This</returns>
        public ILHelper Skip(int Count)
        {
            actionQueue.Add((ActionType.Skip, Count));
            return this;
        }

        /// <summary>Delete instructions up to an instruction set</summary>
        /// <param name="codes">The instructions to stop at</param>
        /// <returns>This</returns>
        public ILHelper RemoveTo(IList<CodeInstruction> codes)
        {
            actionQueue.Add((ActionType.RemoveTo, codes));
            return this;
        }

        /// <summary>Delete instructions up to an instruction</summary>
        /// <param name="code">The instruction to stop at</param>
        /// <returns>This</returns>
        public ILHelper RemoveTo(CodeInstruction code) => RemoveTo(new[] { code });

        /// <summary>Delete a number of instructions</summary>
        /// <param name="Count">How many to delete</param>
        /// <returns>This</returns>
        public ILHelper Remove(int Count)
        {
            actionQueue.Add((ActionType.RemoveCount, Count));
            return this;
        }

        /// <summary>Transform a set of instructions</summary>
        /// <param name="codes">The instructions to find and transform</param>
        /// <param name="transformer">The method that transforms the found instructions</param>
        /// <returns>This</returns>
        public ILHelper Transform(IList<CodeInstruction> codes, Transformer transformer)
        {
            actionQueue.Add((ActionType.Transform, (codes, transformer)));
            return this;
        }

        /// <summary>Transform an instruction</summary>
        /// <param name="code">The instruction to find and transform</param>
        /// <param name="transformer">The method that transforms the found instruction</param>
        /// <returns>This</returns>
        public ILHelper Transform(CodeInstruction code, Transformer transformer) => Transform(new[] { code }, transformer);

        /// <summary>Add instructions</summary>
        /// <param name="codes">The instructions to add</param>
        /// <returns>This</returns>
        public ILHelper Add(IList<CodeInstruction> codes)
        {
            actionQueue.Add((ActionType.Add, codes));
            return this;
        }

        /// <summary>Add an instruction</summary>
        /// <param name="code">The instruction to add</param>
        /// <returns>This</returns>
        public ILHelper Add(CodeInstruction code) => Add(new[] { code });

        /// <summary>Output the rest of the instructions as-is</summary>
        /// <returns>This</returns>
        public ILHelper Finish()
        {
            actionQueue.Add((ActionType.Finish, null));
            return this;
        }

        /// <summary>Store a set of instructions for use later</summary>
        /// <param name="codes">The instructions to find and store</param>
        /// <returns>This</returns>
        public ILHelper Collect(IList<CodeInstruction> codes)
        {
            actionQueue.Add((ActionType.Collect, codes));
            return this;
        }

        /// <summary>Store an instruction for use later</summary>
        /// <param name="code">The instruction to find and store</param>
        /// <returns>This</returns>
        public ILHelper Collect(CodeInstruction code) => Collect(new[] { code });

        /// <summary>Resets the helper to a default state. Clears all defined actions. Do not use while patching, ever.</summary>
        public void Reset()
        {
            actionQueue.Clear();
            collected.Clear();
        }

        /// <summary>Applies the patch to a given instruction set</summary>
        /// <param name="Instructions">The original instruction set</param>
        /// <param name="generator">the ILGenerator for this patch, if needed.</param>
        /// <returns>The applied patch.</returns>
        public IEnumerable<CodeInstruction> Run(IEnumerable<CodeInstruction> Instructions, ILGenerator generator = null)
        {
            ModEntry.monitor.Log("Now applying patch '" + name + "'...", LogLevel.Trace);
            currentGenerator = generator;
            instructions = Instructions;
            cursor = new(instructions.GetEnumerator());
            actionIndex = 0;
            hasErrored = false;
            foreach (var item in actionQueue)
            {
                int count = 0;
                switch (item.action)
                {
                    case ActionType.Skip:
                        count = (int)item.arg;
                        for (int c = 0; c < count && cursor.MoveNext(); c++)
                            yield return cursor.Current;
                        break;
                    case ActionType.SkipTo:
                        foreach (var code in matchSequence((IList<CodeInstruction>)item.arg))
                            yield return code;
                        break;
                    case ActionType.Add:
                        foreach (var code in (IList<CodeInstruction>)item.arg)
                            yield return code;
                        break;
                    case ActionType.RemoveCount:
                        count = (int)item.arg;
                        for (int c = 0; c < count && cursor.MoveNext(); c++){};
                        break;
                    case ActionType.RemoveTo:
                        foreach (var code in matchSequence((IList<CodeInstruction>)item.arg)){}
                        break;
                    case ActionType.Collect:
                        var markers = (IList<CodeInstruction>)item.arg;
                        foreach (var code in matchSequence(markers))
                            yield return code;
                        collected = cursor.GetBuffer();
                        break;
                    case ActionType.Finish:
                        while (cursor.MoveNext())
                            yield return cursor.Current;
                        break;
                    case ActionType.Transform:
                        (var tmarkers, var transformer) = (ValueTuple<IList<CodeInstruction>, Transformer>)item.arg;
                        foreach (var code in matchSequence(tmarkers))
                            yield return code;
                        foreach (var code in transformer(cursor.Take(tmarkers.Count).ToArray()))
                            yield return code;
                        break;
                }

                if (hasErrored)
                    break;
                actionIndex++;
            }
            currentGenerator = null;
            if (hasErrored)
                monitor.Log("Failed to correctly apply patch '" + name + "'! May cause problems!", LogLevel.Error);
            else
                monitor.Log("Successfully applied patch '" + name + "'.", LogLevel.Trace);
            hasErrored = false;
        }
        private IEnumerable<CodeInstruction> matchSequence(IList<CodeInstruction> markers)
        {
            int marker = 0;
            var matched = new CodeInstruction[markers.Count];
            while (cursor.MoveNext())
            {
                var s = markers[marker];
                var code = cursor.Current;
                if(s is null || (code.opcode == s.opcode && CompareOperands(code.operand, s.operand)))
                {
                    matched[marker] = code;
                    marker ++;
                }
                else if(marker > 0)
                {
                    for (int i = 0; i < marker; i++)
                    {
                        yield return matched[i];
                        matched[i] = null;
                    }
                    marker = 0;
                    yield return code;
                }
                else
                {
                    yield return code;
                }
                if (marker >= markers.Count)
                {
                    monitor.Log($"Found markers for '{name}':{actionIndex}", LogLevel.Trace);
                    foreach(var item in matched)
                        if(item is not null)
                            cursor.Push(item);
                    yield break;
                }
            }
            hasErrored = true;
            monitor.Log($"Failed to apply patch component '{name}':{actionIndex}; Marker instructions not found!", LogLevel.Error);
        }

        /// <summary>Throughly compare the operands of two CodeInstructions. Use (int, Type) tuples for locals.</summary>
        /// <param name="op1">Specified operand</param>
        /// <param name="op2">Source operand</param>
        /// <returns>True if matching, otherwise false</returns>
        public static bool CompareOperands(object op1, object op2)
        {
            if (op1 == null || op1.Equals(op2))
                return true;

            if (op2 is sbyte sb && Convert.ToInt32(sb).Equals(op1))
                return true;

            if (op1 is LocalBuilder oper1 && op2 is ValueTuple<int, Type> oper2)
            {
                return (oper2.Item1 < 0 || oper1.LocalIndex == oper2.Item1) && (oper2.Item2 == null || oper1.LocalType == oper2.Item2);
            }
            return false;
        }
    }
}
