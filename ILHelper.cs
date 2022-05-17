using AeroCore.Generics;
using AeroCore.Utils;
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
    public class ILHelper : IEnumerable<CodeInstruction>
    {
        #region head
        public delegate IList<CodeInstruction> Transformer(ILEnumerator enumer);
        private readonly List<(int action, object arg)> actionQueue = new();
        public bool Debug = false;
        private readonly string name;
        private readonly IMonitor monitor;
        public ILGenerator generator;
        private IEnumerable<CodeInstruction> instructions;

        public IEnumerator<CodeInstruction> GetEnumerator() => new ILEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new ILEnumerator(this);

        /// <summary>Create a new IL Helper</summary>
        /// <param name="Monitor">Your mod's monitor</param>
        /// <param name="Name">The name of this patch. (Only used for logging.)</param>
        public ILHelper(IMonitor Monitor, string Name, bool debug = false)
        {
            monitor = Monitor;
            name = Name;
            Debug = debug;
        }

        /// <summary>Sets up the helper to run the patch</summary>
        /// <param name="instructions">The original instruction set</param>
        /// <param name="Generator">The associated <see cref="ILGenerator"/>. Required if creating <see cref="Label"/>s, otherwise optional.</param>
        /// <returns>This</returns>
        public ILHelper Run(IEnumerable<CodeInstruction> instructions, ILGenerator Generator = null)
        {
            this.instructions = instructions;
            generator = Generator;
            return this;
        }

        /// <summary>Throughly compare the operands of two <see cref="CodeInstruction"/>s. Use (<see cref="int"/>, <see cref="Type"/>) tuples for locals.</summary>
        /// <param name="op1">Source operand</param>
        /// <param name="op2">Specified operand</param>
        public static bool CompareOperands(object op1, object op2)
        {
            if (op2 == null || op2.Equals(op1))
                return true;

            if (op1 is sbyte sb && Convert.ToInt32(sb).Equals(op2))
                return true;

            if (op1 is LocalBuilder oper1 && op2 is ValueTuple<int, Type> oper2)
            {
                return (oper2.Item1 < 0 || oper1.LocalIndex == oper2.Item1) && (oper2.Item2 == null || oper1.LocalType == oper2.Item2);
            }
            return false;
        }

        /// <summary>Uses <see cref="CompareOperands(object, object)"/> to compare operands, and also compares operators and checks null</summary>
        /// <param name="src">Source instruction</param>
        /// <param name="def">Specified instruction</param>
        public static bool CompareInstructions(CodeInstruction src, CodeInstruction def)
        {
            return def is null || (def.opcode.Equals(src.opcode) && CompareOperands(src.operand, def.operand));
        }

        #endregion head
        #region queue
        /// <summary>Returns the remainder of the original method unaltered.</summary>
        public ILHelper Finish()
        {
            actionQueue.Add((0, null));
            return this;
        }

        /// <summary>Skip a number of instructions</summary>
        /// <param name="count">How many to skip</param>
        public ILHelper Skip(int count)
        {
            actionQueue.Add((1, count));
            return this;
        }

        /// <summary>Move forward to a specific instruction</summary>
        /// <param name="marker">The instruction</param>
        public ILHelper SkipTo(CodeInstruction marker) => SkipTo(new[] { marker });

        /// <summary>Move forward to the first of a specific set of instructions</summary>
        /// <param name="markers">The instructions</param>
        public ILHelper SkipTo(IList<CodeInstruction> markers)
        {
            actionQueue.Add((2, markers));
            return this;
        }

        /// <summary>Remove a certain number of instructions</summary>
        /// <param name="count">How many to remove</param>
        public ILHelper Remove(int count)
        {
            actionQueue.Add((3, count));
            return this;
        }

        /// <summary>Remove all instructions from the current point until a specific instruction</summary>
        /// <param name="marker">The instruction to stop at</param>
        public ILHelper RemoveTo(CodeInstruction marker) => RemoveTo(new[] { marker });

        /// <summary>Remove all instructions from the current point until a specific set of instructions</summary>
        /// <param name="marker">The instructions to stop just at</param>
        public ILHelper RemoveTo(IList<CodeInstruction> markers)
        {
            actionQueue.Add((4, markers));
            return this;
        }

        /// <summary>Add an instruction</summary>
        /// <param name="instruction">The instruction to add</param>
        public ILHelper Add(CodeInstruction instruction) => Add(new[]{ instruction });

        /// <summary>Add a set of instructions</summary>
        /// <param name="instructions">The instructions to add</param>
        public ILHelper Add(IList<CodeInstruction> instructions)
        {
            actionQueue.Add((5, instructions));
            return this;
        }

        /// <summary>
        /// Directly manipulate the instructions at the current point.
        /// The delegate accepts the cursor in its current state, and outputs a list of instructions to add.
        /// </summary>
        /// <param name="transformer">Transformation delegate</param>
        public ILHelper Transform(Transformer transformer)
        {
            actionQueue.Add((6, transformer));
            return this;
        }
        /// <summary>Add a named <see cref="Label"/> to the current instruction. See <see cref="ILEnumerator.CreateLabel"/></summary>
        /// <param name="id">The name of the previously-created <see cref="Label"/></param>
        public ILHelper AddLabel(string id) => AddLabels(new[] { id });

        /// <summary>Add a list of named <see cref="Label"/>s to the current instruction. See <see cref="ILEnumerator.CreateLabel"/></summary>
        /// <param name="ids">The list of names of previously-created <see cref="Label"/>s</param>
        public ILHelper AddLabels(IList<string> ids)
        {
            actionQueue.Add((7, ids));
            return this;
        }
        #endregion queue

        public class ILEnumerator : IEnumerator<CodeInstruction>
        {
            private delegate bool Mode(ILEnumerator e, ref CodeInstruction result);
            private static readonly Mode[] modes = {Finish, Skip, SkipTo, Remove, RemoveTo, Add, Add, AddLabels};

            private bool disposedValue;
            public readonly BufferedEnumerator<CodeInstruction> source;
            private readonly ILHelper owner;
            public readonly ILGenerator gen;
            private readonly Dictionary<string, Label> labels = new();

            private CodeInstruction current = null;
            private bool isSetup = false;
            private Mode mode;
            private int modeIndex = 0;
            private bool hasErrored = false;

            private int marker = 0;
            private IList<CodeInstruction> anchors;
            private CodeInstruction[] matched;
            private IList<string> labelsToAdd;

            public CodeInstruction Current => current;
            object IEnumerator.Current => current;

            internal ILEnumerator(ILHelper Owner)
            {
                owner = Owner;
                gen = owner.generator;
                source = new(Owner.instructions.GetEnumerator());
            }

            public bool MoveNext()
            {
                if (!isSetup)
                {
                    if (!gotoNextMode())
                    {
                        owner.monitor.Log($"Patch '{owner.name}' contains no operations! This will result in an empty output!", LogLevel.Error);
                        return false;
                    }
                    if (!source.MoveNext())
                    {
                        owner.monitor.Log($"Patch '{owner.name}' source instructions empty! Did you forget to Run()?", LogLevel.Error);
                        return false;
                    }
                    current = source.Current;
                    isSetup = true;
                }

                bool r = !hasErrored;
                if (r)
                {
                    while (mode.Invoke(this, ref current) && !hasErrored)
                        if (!gotoNextMode())
                            return false;

                    if (hasErrored)
                        owner.monitor.Log($"Patch '{owner.name}' was not applied correctly!", LogLevel.Error);
                    r = !hasErrored;
                }
                return r;
            }
            public void Reset()
            {
                source.Reset();
                modeIndex = 0;
                hasErrored = false;
                isSetup = false;
                labels.Clear();
                current = null;
            }

            /// <summary>Create a new <see cref="Label"/>. Can be named. Requires an <see cref="ILGenerator"/> to be provided in <see cref="Run"/></summary>
            /// <param name="id">If included, the name of the <see cref="Label"/></param>
            /// <returns>The created <see cref="Label"/></returns>
            public Label CreateLabel(string id = null)
            {
                if (gen is null)
                {
                    error("ILGenerator is required to create labels, but was not provided");
                    return default;
                }

                if (labels.TryGetValue(id, out var l))
                {
                    error($"Label with ID '{id}' already exists");
                    return l;
                }

                Label label = gen.DefineLabel();
                if (id is not null)
                    labels.Add(id, label);
                return label;
            }

            /// <summary>Get a named <see cref="Label"/></summary>
            /// <param name="id">The name of the <see cref="Label"/> to get</param>
            /// <returns>The <see cref="Label"/></returns>
            public Label GetLabel(string id)
            {
                if (labels.TryGetValue(id, out var l))
                    return l;
                else
                    error($"Label with id '{id}' has not been created and does not exit");
                return default;
            }
            private bool gotoNextMode()
            {
                if (modeIndex >= owner.actionQueue.Count)
                    return false;
                var (action, arg) = owner.actionQueue[modeIndex];
                mode = modes[action];
                marker = 0;

                switch (action)
                {
                    case 1 or 3: marker = (int)arg; break;
                    case 2 or 4:
                        anchors = (IList<CodeInstruction>)arg;
                        matched = new CodeInstruction[anchors.Count];
                        break;
                    case 5: anchors = (IList<CodeInstruction>)arg; break;
                    case 6: anchors = ((Transformer)arg).Invoke(this); break;
                    case 7: labelsToAdd = (IList<string>)arg; break;
                }

                modeIndex++;
                return true;
            }
            private bool matchSequence()
            {
                int i = 0;
                bool r = true;
                bool ret = false;
                while(i < anchors.Count && r)
                {
                    matched[i] = source.Current;

                    if (owner.Debug && i > 0)
                        owner.monitor.Log(source.Current.ToString(), LogLevel.Debug);

                    i++;
                    if (!CompareInstructions(source.Current, anchors[i - 1]))
                        break;
                    ret = i >= anchors.Count;

                    if (owner.Debug && i == 1)
                        owner.monitor.Log(source.Current.ToString(), LogLevel.Debug);

                    if (i < anchors.Count)
                        r = source.MoveNext();
                }
                while(i > 0)
                {
                    i--;
                    source.Push(matched[i]);
                }
                source.MoveNext();
                return ret;
            }
            private void error(string reason)
            {
                hasErrored = true;
                owner.monitor.Log($"{reason}! @'{owner.name}':{modeIndex}", LogLevel.Error);
            }
            #region Modes
            private static bool Finish(ILEnumerator inst, ref CodeInstruction result)
            {
                result = inst.source.Current;
                return !inst.source.MoveNext();
            }
            private static bool Skip(ILEnumerator inst, ref CodeInstruction result)
            {
                result = inst.source.Current;
                if (inst.marker <= 0 || !inst.source.MoveNext())
                    return true;
                inst.marker--;
                return false;
            }
            private static bool SkipTo(ILEnumerator inst, ref CodeInstruction result)
            {
                bool v = inst.matchSequence();
                result = inst.source.Current;
                if (!v)
                {
                    if (!inst.source.MoveNext())
                    {
                        inst.error("Could not find marker instructions");
                        return true;
                    }
                }
                return v;
            }
            private static bool Remove(ILEnumerator inst, ref CodeInstruction result)
            {
                while (inst.marker > 0 && inst.source.MoveNext())
                    inst.marker--;
                result = inst.source.Current;
                return true;
            }
            private static bool RemoveTo(ILEnumerator inst, ref CodeInstruction result)
            {
                var v = true;
                while (v && !inst.matchSequence())
                    v = inst.source.MoveNext();
                if (!v)
                    inst.error("Could not find marker instructions");
                else
                    result = inst.source.Current;
                return true;
            }
            private static bool Add(ILEnumerator inst, ref CodeInstruction result)
            {
                if (inst.marker >= inst.anchors.Count)
                    return true;
                result = inst.anchors[inst.marker];
                inst.marker++;
                return false;
            }
            private static bool AddLabels(ILEnumerator inst, ref CodeInstruction result)
            {
                if(result is null)
                    inst.error("Tried to add labels to null instruction");
                else
                    foreach (string id in inst.labelsToAdd)
                        if (inst.labels.TryGetValue(id, out var label))
                            result.labels.Add(label);
                        else
                            inst.error($"Label with id '{id}' has not been created and does not exist");
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
