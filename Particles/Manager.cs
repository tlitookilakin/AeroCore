using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace AeroCore.Particles
{
    public class Manager : IParticleManager
    {
        public Rectangle? ClipRegion { get; set; } = null;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = new(1f, 1f);
        public float Depth { get; set; } = 0f;

        private int[] life;
        private Vector2[] positions;
        private int millis;
        private readonly IParticleBehavior behavior;
        private readonly IParticleSkin skin;
        private readonly Emitter emitter;
        private bool isSetup = false;

        public Manager(int count, IParticleBehavior behavior, IParticleSkin skin, IParticleEmitter emitter)
        {
            this.behavior = behavior;
            life = new int[count];
            positions = new Vector2[count];
            this.skin = skin;
            this.emitter = emitter is Emitter e ? e : new(emitter);
        }

        public void Cleanup()
        {
            skin.Cleanup();
            behavior.Cleanup();
            millis = 0;
            isSetup = false;
            life.Clear();
            positions.Clear();
        }

        public void Draw(SpriteBatch batch)
        {
            if(isSetup)
                if(ClipRegion is null || Game1.viewport.ToRect().Intersects((Rectangle)ClipRegion))
                    skin.Draw(batch, positions, life, millis, Scale, Offset, Depth);
            else
                ModEntry.monitor.LogOnce($"Particle Emitter was not ticked before attempting to draw!", LogLevel.Warn);
        }

        public void Tick(int millis)
        {
            this.millis = millis;
            if (!isSetup)
                Setup();
            emitter.Tick(ref positions, ref life, millis);
            behavior.Tick(ref positions, ref life, millis);
        }
        private void Setup()
        {
            emitter.Reset();
            behavior.Startup();
            skin.Startup();
        }
    }
}
