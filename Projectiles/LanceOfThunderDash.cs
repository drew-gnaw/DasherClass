﻿using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using System;
using System.Collections.Generic;

namespace DasherClass.Projectiles
{
    public class LanceOfThunderDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 100f;
        public override float ChargeTime => 60f;
        public override float DashTime => 10f;
        public override float PullBackScale => 0.995f;
        public override float MaxPullBackRate => 0.90f;
        public override int OnHitIFrames => 15;
        public override float HoldMinRadius => 67f;
        public override float HoldMaxRadius => 80f;
        public override float ChargingFrameDelay => 4f;
        public override float LungingFrameDelay => 2f;
        public override bool CycleChargingSprite => true;
        public override float LungingMinRadius => 67f;
        public override float LungingMaxRadius => 80f;
        public override int FrameDelay { get; set; } = 2;
        public override bool CycleLungingSprite => true;
        

        #region Lightning Effect Parameters

        // Lightning appearance
        public float LightningJaggedness = 90f;
        public float LightningCoreWidth = 2f;
        public int LightningDuration = 4; // in ticks
        public Color LightningColor = new Color(255, 255, 150);

        // Damage
        public float LightningDamageMultiplier = 1.5f;
        public float LightningKnockback = 8f;
        public float LightningHitRadius = 40f;
        public int LightningBonusCrit = 10;

        // Effects
        public float ScreenShakeIntensity = 12f;

        #endregion

        // Derived values (computed from core parameters)
        private float LightningGlowWidth => LightningCoreWidth * 1.75f;
        private float LightningBloomWidth => LightningCoreWidth * 6.25f;
        private Color LightningCoreColor => Color.White;
        private Color LightningGlowColor => new Color(LightningColor.R, LightningColor.G, LightningColor.B, 200);
        private Color LightningBloomColor => new Color(LightningColor.R, LightningColor.G, LightningColor.B, 80);

        // Internal state for the lightning effect
        private Vector2 lungeStartPosition;
        private bool hasTriggeredLightning = false;
        private int lightningTimer = 0;
        private List<LightningBolt> activeLightningBolts = new List<LightningBolt>();
        private HashSet<int> hitNPCs = new HashSet<int>();

        // Lightning bolt data structure
        private class LightningBolt
        {
            public List<Vector2> Points;
            public float Width;
            public float GlowWidth;
            public float BloomWidth;

            public LightningBolt()
            {
                Points = new List<Vector2>();
            }
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.2f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 30);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.frameCounter = 0;
        }

        internal override void PerformLunge()
        {
            lungeStartPosition = Owner.Center;
            hasTriggeredLightning = false;
            lightningTimer = 0;
            activeLightningBolts.Clear();
            hitNPCs.Clear();
            base.PerformLunge();
        }

        internal override void HandleProjectileVisuals()
        {
            base.HandleProjectileVisuals();

            if (currentDashTime >= DashTime && !hasTriggeredLightning)
            {
                TriggerLightningEffect();
                hasTriggeredLightning = true;

                Owner.velocity *= 0.1f;

                // Reset DasherPlayer state that base class would normally reset
                DasherPlayer.DasherPlayer dasherPlayer = Owner.GetModPlayer<DasherPlayer.DasherPlayer>();
                dasherPlayer.isLunging = false;
                dasherPlayer.isLance = false;
                isMidlunge = false;
            }
        }

        // We need to add some checks before AI happens so that the lightning effect doesn't insta die
        public override void AI()
        {
            if (Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            if (hasTriggeredLightning && lightningTimer > 0)
            {
                lightningTimer--;
                DamageNPCsAlongLightning();
                return;
            }

            if (hasTriggeredLightning && lightningTimer <= 0)
            {
                Projectile.Kill();
                return;
            }

            base.AI();
        }

        private void TriggerLightningEffect()
        {
            Vector2 startPos = lungeStartPosition;
            Vector2 endPos = Owner.Center;

            GenerateLightningBolts(startPos, endPos);

            float pitch = Main.rand.NextFloat(-0.3f, 0.3f);
            SoundEngine.PlaySound(SoundID.Thunder with { Pitch = pitch }, Owner.Center);

            if (Main.LocalPlayer.Distance(Owner.Center) < 2000f)
            {
                Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(ScreenShakeIntensity, ScreenShakeIntensity);
            }

            SpawnImpactDust(startPos);
            SpawnImpactDust(endPos);

            foreach (var bolt in activeLightningBolts)
            {
                SpawnLightningDust(bolt);
            }

            lightningTimer = LightningDuration;
        }

        private void GenerateLightningBolts(Vector2 start, Vector2 end)
        {
            activeLightningBolts.Clear();

            float distance = Vector2.Distance(start, end);
            int segments = 12 + (int)(distance * 0.01f);

            LightningBolt mainBolt = GenerateBolt(start, end, segments, LightningJaggedness);
            mainBolt.Width = LightningCoreWidth;
            mainBolt.GlowWidth = LightningGlowWidth;
            mainBolt.BloomWidth = LightningBloomWidth;
            activeLightningBolts.Add(mainBolt);
        }

        private LightningBolt GenerateBolt(Vector2 start, Vector2 end, int segments, float jaggedness)
        {
            LightningBolt bolt = new LightningBolt();
            bolt.Points.Add(start);

            Vector2 direction = end - start;
            float length = direction.Length();
            Vector2 normalizedDir = direction / length;
            Vector2 perpendicular = new Vector2(-normalizedDir.Y, normalizedDir.X);

            float[] displacements = new float[segments + 1];
            displacements[0] = 0;
            displacements[segments] = 0;

            int step = segments;
            float scale = jaggedness;
            while (step > 1)
            {
                int halfStep = step / 2;
                for (int i = halfStep; i < segments; i += step)
                {
                    int left = i - halfStep;
                    int right = Math.Min(i + halfStep, segments);
                    float avg = (displacements[left] + displacements[right]) / 2f;
                    displacements[i] = avg + (Main.rand.NextFloat() * 2f - 1f) * scale;
                }
                step = halfStep;
                scale *= 0.5f;
            }

            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(start, end, t);
                Vector2 offset = perpendicular * displacements[i];
                bolt.Points.Add(basePos + offset);
            }
            bolt.Points.Add(end);

            return bolt;
        }

        private void DamageNPCsAlongLightning()
        {
            if (Main.myPlayer != Projectile.owner) return;

            int damage = (int)(Projectile.damage * LightningDamageMultiplier);

            foreach (var bolt in activeLightningBolts)
            {
                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    Vector2 segStart = bolt.Points[i];
                    Vector2 segEnd = bolt.Points[i + 1];
                    Vector2 segCenter = (segStart + segEnd) / 2f;

                    foreach (NPC npc in Main.npc)
                    {
                        if (!npc.active || npc.friendly || npc.dontTakeDamage || hitNPCs.Contains(npc.whoAmI))
                            continue;

                        float distToSeg = DistanceToLineSegment(npc.Center, segStart, segEnd);
                        if (distToSeg <= LightningHitRadius + (npc.width + npc.height) / 4f)
                        {
                            Vector2 hitDir = (npc.Center - segCenter).SafeNormalize(Vector2.UnitX);

                            int critChance = Owner.GetWeaponCrit(Owner.HeldItem) + LightningBonusCrit;
                            bool crit = Main.rand.Next(100) < critChance;

                            npc.SimpleStrikeNPC(damage, hitDir.X > 0 ? 1 : -1, crit, LightningKnockback, DasherDamageClass.Instance);

                            hitNPCs.Add(npc.whoAmI);

                            SpawnHitDust(npc.Center);
                        }
                    }
                }
            }
        }

        private float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.Length();
            if (lineLength == 0) return Vector2.Distance(point, lineStart);

            float t = Math.Clamp(Vector2.Dot(point - lineStart, line) / (lineLength * lineLength), 0f, 1f);
            Vector2 projection = lineStart + t * line;
            return Vector2.Distance(point, projection);
        }

        private void SpawnImpactDust(Vector2 position)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(16f, 16f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.GemDiamond, velocity.X, velocity.Y, 100, LightningColor, 0.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }

            // Bright flash at endpoint
            for (int i = 0; i < 5; i++)
            {
                Dust flash = Dust.NewDustDirect(position - new Vector2(25f), 50, 50, DustID.GemDiamond, 0, 0, 0, Color.White, 1f);
                flash.noGravity = true;
                flash.velocity *= 0.3f;
            }
        }

        private void SpawnLightningDust(LightningBolt bolt)
        {
            for (int i = 0; i < bolt.Points.Count - 1; i++)
            {
                Vector2 segStart = bolt.Points[i];
                Vector2 segEnd = bolt.Points[i + 1];

                for (int j = 0; j < 5; j++)
                {
                    float t = Main.rand.NextFloat();
                    Vector2 pos = Vector2.Lerp(segStart, segEnd, t);
                    Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);

                    Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.GemDiamond, velocity.X, velocity.Y, 100, LightningColor, 1.5f);
                    dust.noGravity = true;
                }
            }
        }

        private void SpawnHitDust(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.GemDiamond, velocity.X, velocity.Y, 100, LightningColor, 1.5f);
                dust.noGravity = true;
            }
        }

        #region Drawing

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw lightning effect behind the projectile
            if (lightningTimer > 0 && activeLightningBolts.Count > 0)
            {
                DrawLightningBolts();
                return false;
            }

            // Draw the lance projectile
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect;
            if (Owner.direction == 1)
            {
                directionEffect = SpriteEffects.FlipVertically;
            }
            else
            {
                directionEffect = SpriteEffects.None;
            }
            Main.EntitySpriteDraw(punchTexture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);

            // Draw charging glow effect ON TOP of the sprite
            if (!isMidlunge && currentChargeTime > 0)
            {
                DrawChargingGlow();
            }

            return false;
        }

        private void DrawChargingGlow()
        {
            float chargeProgress = Math.Clamp(currentChargeTime / ChargeTime, 0f, 1f);
            bool isFullyCharged = chargeProgress >= 1f;

            // Calculate base glow intensity from charge progress (ease in)
            float glowIntensity = MathF.Pow(chargeProgress, 2f);

            // Apply charged boost and fluctuation
            if (isFullyCharged)
            {
                float time = (float)Main.GameUpdateCount * 0.5f;
                float fluctuation = 1f + MathF.Sin(time) * 0.25f;
                float fluctuation2 = 1f + MathF.Sin(time * 1.7f + 1.3f) * 0.125f;
                glowIntensity *= 1.5f * fluctuation * fluctuation2;
            }

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects directionEffect = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            float glowAlpha = MathHelper.Lerp(0.1f, 0.6f, glowIntensity);
            float scaleBoost = MathHelper.Lerp(0.2f, 0.9f, chargeProgress);
            if (isFullyCharged)
            {
                float time = (float)Main.GameUpdateCount * 0.5f;
                scaleBoost *= 1f + MathF.Sin(time * 0.8f + 0.5f) * 0.05f;
            }

            // Glow colors derived from LightningColor
            Color glowInner = new Color(255, 255, 200);
            Color glowOuter = new Color(LightningColor.R, LightningColor.G, (byte)(LightningColor.B * 0.67f), 150);

            int layers = isFullyCharged ? 5 : 3;
            for (int layer = layers - 1; layer >= 0; layer--)
            {
                float layerProgress = (float)layer / Math.Max(layers - 1, 1);
                float layerScale = Projectile.scale * (1f + scaleBoost * (0.3f + layerProgress * 0.7f));
                float layerAlpha = glowAlpha * (1f - layerProgress * 0.6f);
                Color layerColor = Color.Lerp(glowInner, glowOuter, layerProgress) * layerAlpha;

                Main.EntitySpriteDraw(texture, drawPos, frame, layerColor, Projectile.rotation, origin, layerScale, directionEffect, 0);
            }

            // Draw extra bright core layer when fully charged
            if (isFullyCharged)
            {
                float time = (float)Main.GameUpdateCount * 0.5f;
                float coreFluctuation = 0.7f + MathF.Sin(time * 2f) * 0.3f;
                float coreAlpha = 0.4f * coreFluctuation * glowIntensity;
                Color coreColor = Color.White * coreAlpha;

                Main.EntitySpriteDraw(texture, drawPos, frame, coreColor, Projectile.rotation, origin, Projectile.scale * 1.05f, directionEffect, 0);
            }
        }

        private void DrawLightningBolts()
        {
            // Calculate fade based on remaining time
            float fade = (float)lightningTimer / LightningDuration;
            float fadeEased = MathF.Pow(fade, 0.5f); // Ease out for smoother fade

            // Get the pixel texture for drawing lines
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            // Draw each bolt
            for (int b = activeLightningBolts.Count - 1; b >= 0; b--)
            {
                LightningBolt bolt = activeLightningBolts[b];

                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    Vector2 start = bolt.Points[i] - Main.screenPosition;
                    Vector2 end = bolt.Points[i + 1] - Main.screenPosition;

                    Vector2 direction = end - start;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();

                    // Add some color variation per segment
                    Color coreColor = AddColorVariation(LightningCoreColor, 15f);
                    Color glowColor = AddColorVariation(LightningGlowColor, 7.5f);
                    Color bloomColor = AddColorVariation(LightningBloomColor, 4.5f);

                    // Apply fade using alpha (no width shrinking)
                    coreColor.A = (byte)Math.Clamp(coreColor.A * fadeEased, 0, 255);
                    glowColor.A = (byte)Math.Clamp(glowColor.A * fadeEased, 0, 255);
                    bloomColor.A = (byte)Math.Clamp(bloomColor.A * fadeEased * 0.7f, 0, 255);

                    // Draw outer bloom (largest, most transparent)
                    DrawLightningSegment(pixel, start, length, rotation, bolt.BloomWidth, bloomColor);

                    // Draw inner glow
                    DrawLightningSegment(pixel, start, length, rotation, bolt.GlowWidth, glowColor);

                    // Draw core (brightest)
                    DrawLightningSegment(pixel, start, length, rotation, bolt.Width, coreColor);
                }
            }
        }

        private void DrawLightningSegment(Texture2D texture, Vector2 start, float length, float rotation, float width, Color color)
        {
            Vector2 origin = new Vector2(0, texture.Height / 2f);
            Vector2 scale = new Vector2(length / texture.Width, width / texture.Height);

            Main.EntitySpriteDraw(
                texture,
                start,
                null,
                color,
                rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );
        }

        private Color AddColorVariation(Color baseColor, float variation)
        {
            int r = Math.Clamp((int)(baseColor.R + Main.rand.NextFloat(-variation, variation)), 0, 255);
            int g = Math.Clamp((int)(baseColor.G + Main.rand.NextFloat(-variation, variation)), 0, 255);
            int b = Math.Clamp((int)(baseColor.B + Main.rand.NextFloat(-variation, variation)), 0, 255);
            return new Color(r, g, b, baseColor.A);
        }

        #endregion
    }
}