using AeroCore.Generics;
using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore
{
    public class NewILHelper : IEnumerable<CodeInstruction>
    {
        #region head
        private enum ActionType {Finish, Skip, SkipTo, Remove, RemoveTo, Add, Collect, Transform, AddLabel};
        private readonly List<(int action, object arg)> actionQueue = new();
        private readonly string name;
        private readonly IMonitor monitor;
        public ILGenerator generator;

        public IEnumerator<CodeInstruction> GetEnumerator() => new ILEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new ILEnumerator(this);

        public NewILHelper(IMonitor Monitor, string Name)
        {
            monitor = Monitor;
            name = Name;
        }
        /// <summary>Throughly compare the operands of two CodeInstructions. Use (int, Type) tuples for locals.</summary>
        /// <param name="op1">Source operand</param>
        /// <param name="op2">Specified operand</param>
        /// <returns>True if matching, otherwise false</returns>
        public static bool CompareOperands(object op1, object op2)
        {
            if (op1 == null || op1.Equals(op2))
                return true;

            if (op1 is sbyte sb && Convert.ToInt32(sb).Equals(op2))
                return true;

            if (op1 is LocalBuilder oper1 && op2 is ValueTuple<int, Type> oper2)
            {
                return (oper2.Item1 < 0 || oper1.LocalIndex == oper2.Item1) && (oper2.Item2 == null || oper1.LocalType == oper2.Item2);
            }
            return false;
        }

        #endregion head
        #region queue
        public NewILHelper Finish()
        {
            actionQueue.Add(((int)ActionType.Finish, null));
            return this;
        }
        #endregion queue

        private class ILEnumerator : IEnumerator<CodeInstruction>
        {
            private delegate bool Mode(ILEnumerator e, ref CodeInstruction result);
            private static readonly Mode[] modes = {Finish, Skip, SkipTo, Remove, RemoveTo, Add};

            private bool disposedValue;
            private BufferedEnumerator<CodeInstruction> source;
            private readonly NewILHelper owner;

            private CodeInstruction current;
            private Mode mode;
            private bool nextMode = true;
            private int modeIndex = 0;
            private bool hasErrored = false;

            private int marker = 0;
            private IList<CodeInstruction> anchors;
            private CodeInstruction[] matched;

            public CodeInstruction Current => current;
            object IEnumerator.Current => current;

            public ILEnumerator(NewILHelper Owner)
            {
                owner = Owner;
            }

            public bool MoveNext()
            {
                if (nextMode)
                {
                    if (modeIndex >= owner.actionQueue.Count)
                        return false;
                    nextMode = false;
                    var m = owner.actionQueue[modeIndex];
                    mode = modes[m.action];
                    marker = 0;

                    switch (m.action)
                    {
                        case 1 or 3: marker = (int)m.arg; break;
                        case 2 or 4:
                            anchors = (IList<CodeInstruction>)m.arg;
                            matched = new CodeInstruction[anchors.Count];
                            break;
                    }

                    modeIndex++;
                }

                bool r = !hasErrored;
                if (r)
                {
                    nextMode = mode.Invoke(this, ref current);
                    if (hasErrored)
                        owner.monitor.Log($"Patch '{owner.name}' was not applied correctly!", LogLevel.Error);
                }
                return r;
            }
            public void Reset()
            {
                source.Reset();
                nextMode = true;
                modeIndex = 0;
                hasErrored = false;
            }
            private bool matchSequence()
            {
                var cached = source.Current;
                while(marker < anchors.Count)
                {
                    var s = anchors[marker];
                    var code = source.Current; 
                    matched[marker] = code;
                    marker++;
                    if (!source.MoveNext() || (s is not null && (code.opcode != s.opcode || !CompareOperands(code.operand, s.operand))))
                    {
                        flushMatched();
                        source.MoveNext();
                        return false;
                    }
                }
                source.Push(cached);
                flushMatched();
                source.MoveNext();
                return true;
            }
            private void flushMatched()
            {
                while (marker > 0)
                {
                    source.Push(matched[^marker]);
                    marker--;
                }
            }
            #region Modes
            private static bool Finish(ILEnumerator inst, ref CodeInstruction result)
            {
                if (inst.source.MoveNext())
                    result = inst.source.Current;
                else
                    return true;
                return false;
            }
            private static bool Skip(ILEnumerator inst, ref CodeInstruction result)
            {
                if (inst.marker <= 0 || !inst.source.MoveNext())
                    return true;
                result = inst.source.Current;
                inst.marker--;
                return false;
            }
            private static bool SkipTo(ILEnumerator inst, ref CodeInstruction result)
            {
                if (!inst.source.MoveNext())
                {
                    inst.owner.monitor.Log($"Could not find marker instructions for '{inst.owner.name}':{inst.modeIndex}", LogLevel.Error);
                    inst.hasErrored = true;
                    return true;
                }
                bool v = inst.matchSequence();
                result = inst.source.Current;
                return v;
            }
            private static bool Remove(ILEnumerator inst, ref CodeInstruction result)
            {
                while (inst.marker > 0 && inst.source.MoveNext()) { }
                result = inst.source.Current;
                return true;
            }
            private static bool RemoveTo(ILEnumerator inst, ref CodeInstruction result)
            {
                var v = inst.source.MoveNext();
                while (v && !inst.matchSequence())
                    v = inst.source.MoveNext();
                if (!v)
                {
                    inst.owner.monitor.Log($"Could not find marker instructions for '{inst.owner.name}':{inst.modeIndex}", LogLevel.Error);
                    inst.hasErrored = true;
                }
                return true;
            }
            private static bool Add(ILEnumerator inst, ref CodeInstruction result)
            {
                foreach(var item in inst.anchors)
                    inst.source.Push(item);
                return true;
            }
            #endregion Modes
            #region dispose
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects)
                    }
                    disposedValue = true;
                }
            }
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion dispose
        }
    }
}
