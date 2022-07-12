using AeroCore.Particles;
using AeroCore.Utils;
using StardewModdingAPI;
using System.Collections.Generic;

namespace AeroCore.Models
{
    public class ParticleDefinition
    {
        public string Behavior { get; set; }
        public string Skin { get; set; }
        public object BehaviorSettings { get; set; }
        public object SkinSettings { get; set; }

        public Manager Create(IParticleEmitter emitter, int count)
            => Create(Behavior, Skin, BehaviorSettings, SkinSettings, emitter, count);
        public static Manager Create(string Behavior, string Skin, object BehaviorSettings, object SkinSettings, IParticleEmitter emitter, int count)
        {
            if (!API.API.knownPartBehaviors.TryGetValue(Behavior, out var bgen))
                ModEntry.monitor.Log($"Behavior type '{Behavior}' could not be found", LogLevel.Warn);
            else if (!API.API.knownPartSkins.TryGetValue(Skin, out var sgen))
                ModEntry.monitor.Log($"Skin type '{Skin}' could not be found", LogLevel.Warn);
            else
                return new Manager(
                    count,
                    BehaviorSettings is null ? bgen(count) : Reflection.MapTo(bgen(count), BehaviorSettings),
                    SkinSettings is null ? sgen(count) : Reflection.MapTo(sgen(count), SkinSettings),
                    emitter
                );
            return null;
        }
    }
}
