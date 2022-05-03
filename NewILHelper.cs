using AeroCore.Generics;
using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCore
{
    public class NewILHelper : IEnumerable<CodeInstruction>
    {
        #region head
        private enum ActionType {Finish};
        private readonly List<(int action, object arg)> actionQueue = new();
        private readonly string name;
        private readonly IMonitor monitor;

        public IEnumerator<CodeInstruction> GetEnumerator() => new ILEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new ILEnumerator(this);

        public NewILHelper(IMonitor Monitor, string Name)
        {
            monitor = Monitor;
            name = Name;
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
            private static readonly Mode[] modes = {Finish};

            private bool disposedValue;
            private BufferedEnumerator<CodeInstruction> source;
            private readonly NewILHelper owner;

            private CodeInstruction current;
            private Mode mode;
            private bool nextMode = true;
            private int modeIndex = 0;
            private bool hasErrored = false;

            public CodeInstruction Current => current;
            object IEnumerator.Current => current;

            public ILEnumerator(NewILHelper Owner)
            {
                this.owner = Owner;
            }

            public bool MoveNext()
            {
                if (nextMode)
                {
                    if (modeIndex >= owner.actionQueue.Count)
                        return false;
                    nextMode = false;
                    mode = modes[owner.actionQueue[modeIndex].action];
                    modeIndex++;
                }

                bool r = hasErrored;
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
            #region Modes
            private static bool Finish(ILEnumerator inst, ref CodeInstruction result)
            {
                if (inst.source.MoveNext())
                    result = inst.source.Current;
                else
                    return true;
                return false;
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
