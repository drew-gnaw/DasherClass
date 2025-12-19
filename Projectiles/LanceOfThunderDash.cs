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
        public override float ChargeTime => 600f;
        public override float DashTime => 10f;
        public override float PullBackScale => 0.995f;
        public override float MaxPullBackRate => 0.90f;
        public override int OnHitIFrames => 15;
        public override float HoldMinRadius => 67f;
        public override float HoldMaxRadius => 80f;

        #region Lightning Effect Tunable Parameters

        // ═══════════════════════════════════════════════════════════════
        // MAIN LIGHTNING PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>How much the lightning can deviate perpendicular to the main path (pixels)</summary>
        public float LightningJaggednessMagnitude = 180f;

        /// <summary>Base number of segments for the main lightning bolt</summary>
        public int LightningBaseSegments = 12;

        /// <summary>Additional segments per 100 pixels of distance</summary>
        public float LightningSegmentsPerDistance = 0.08f;

        /// <summary>Thickness of the main lightning bolt core (pixels)</summary>
        public float LightningCoreWidth = 8f;

        /// <summary>Thickness of the lightning glow effect (pixels)</summary>
        public float LightningGlowWidth = 14f;

        /// <summary>Thickness of the outer bloom effect (pixels)</summary>
        public float LightningBloomWidth = 50f;

        // ═══════════════════════════════════════════════════════════════
        // BRANCHING PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Chance (0-1) for each segment to spawn a branch</summary>
        public float BranchChance = 0.35f;

        /// <summary>Maximum angle deviation for branches (radians)</summary>
        public float BranchAngleDeviation = 0.8f;

        /// <summary>Minimum length of a branch as fraction of remaining main bolt</summary>
        public float BranchLengthMin = 0.2f;

        /// <summary>Maximum length of a branch as fraction of remaining main bolt</summary>
        public float BranchLengthMax = 0.5f;

        /// <summary>Width multiplier for branches compared to main bolt</summary>
        public float BranchWidthMultiplier = 0.5f;

        /// <summary>Jaggedness multiplier for branches</summary>
        public float BranchJaggednessMultiplier = 0.7f;

        /// <summary>Number of segments for branch bolts</summary>
        public int BranchSegments = 5;

        /// <summary>Maximum recursion depth for sub-branches</summary>
        public int MaxBranchDepth = 2;

        /// <summary>Chance multiplier per depth level (compounds)</summary>
        public float BranchChanceDecay = 0.4f;

        // ═══════════════════════════════════════════════════════════════
        // COLOR PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Core lightning color (brightest center)</summary>
        public Color LightningCoreColor = new Color(255, 255, 255, 255);

        /// <summary>Inner glow color</summary>
        public Color LightningGlowColor = new Color(255, 255, 150, 200);

        /// <summary>Outer bloom color</summary>
        public Color LightningBloomColor = new Color(255, 255, 150, 80);

        /// <summary>Color variation per segment (randomized RGB shift)</summary>
        public float ColorVariation = 15f;

        // ═══════════════════════════════════════════════════════════════
        // VISUAL FX PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Duration the lightning persists (in frames, 60 = 1 second)</summary>
        public int LightningDuration = 60;

        /// <summary>Number of dust particles spawned at impact</summary>
        public int ImpactDustCount = 20;

        /// <summary>Number of dust particles per lightning segment</summary>
        public int DustPerSegment = 3;

        /// <summary>Size of dust particles (1.0 = normal)</summary>
        public float DustScale = 1.5f;

        /// <summary>Speed of dust particles spreading outward</summary>
        public float DustSpreadSpeed = 16f;

        /// <summary>Flash intensity at the endpoints (0-1)</summary>
        public float EndpointFlashIntensity = 1.0f;

        /// <summary>Radius of the endpoint flash orb</summary>
        public float EndpointFlashRadius = 50f;

        // ═══════════════════════════════════════════════════════════════
        // DUST COLOR PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Color of the main dust particles along the lightning</summary>
        public Color LightningDustColor = new Color(255, 255, 150, 255);

        /// <summary>Color of impact/explosion dust at endpoints</summary>
        public Color ImpactDustColor = new Color(255, 255, 100, 255);

        /// <summary>Color of the secondary impact dust</summary>
        public Color ImpactDustSecondaryColor = new Color(255, 200, 50, 255);

        /// <summary>Color of dust particles spawned on enemy hits</summary>
        public Color HitDustColor = new Color(255, 200, 100, 255);

        // ═══════════════════════════════════════════════════════════════
        // DAMAGE PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Damage multiplier for the lightning (relative to weapon damage)</summary>
        public float LightningDamageMultiplier = 1.5f;

        /// <summary>Knockback of the lightning strike</summary>
        public float LightningKnockback = 8f;

        /// <summary>Hit detection radius around each lightning segment</summary>
        public float LightningHitRadius = 40f;

        /// <summary>Whether lightning can crit</summary>
        public bool LightningCanCrit = true;

        /// <summary>Additional crit chance for lightning (added to weapon crit)</summary>
        public int LightningBonusCrit = 10;

        // ═══════════════════════════════════════════════════════════════
        // SOUND PARAMETERS  
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Volume of the thunder sound (0-1)</summary>
        public float ThunderVolume = 1.0f;

        /// <summary>Pitch variation range for the thunder sound</summary>
        public float ThunderPitchVariance = 0.3f;

        // ═══════════════════════════════════════════════════════════════
        // SCREEN SHAKE PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Intensity of the screen shake</summary>
        public float ScreenShakeIntensity = 12f;

        /// <summary>Duration of the screen shake in frames</summary>
        public int ScreenShakeDuration = 8;

        // ═══════════════════════════════════════════════════════════════
        // CHARGING GLOW PARAMETERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Minimum scale boost for glow layers when starting to charge (0 = no size increase)</summary>
        public float ChargingGlowScaleMin = 0.2f;

        /// <summary>Maximum scale boost for glow layers when fully charged</summary>
        public float ChargingGlowScaleMax = 0.9f;

        /// <summary>Base glow alpha when starting to charge (0-1)</summary>
        public float ChargingGlowAlphaMin = 0.1f;

        /// <summary>Maximum glow alpha when fully charged (0-1)</summary>
        public float ChargingGlowAlphaMax = 0.6f;

        /// <summary>Color of the charging glow (inner, closest to sprite)</summary>
        public Color ChargingGlowColorInner = new Color(255, 255, 200, 255);

        /// <summary>Color of the charging glow (outer, largest layer)</summary>
        public Color ChargingGlowColorOuter = new Color(255, 220, 100, 150);

        /// <summary>How much the glow fluctuates when fully charged (0-1)</summary>
        public float ChargedGlowFluctuationAmount = 0.25f;

        /// <summary>Speed of the glow fluctuation (higher = faster)</summary>
        public float ChargedGlowFluctuationSpeed = 5f;

        /// <summary>Additional glow multiplier when fully charged</summary>
        public float ChargedGlowBoostMultiplier = 1.5f;

        // ═══════════════════════════════════════════════════════════════
        // BACKGROUND LIGHTNING PARAMETERS (when fully charged)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Minimum frames between background lightning strikes</summary>
        public int BackgroundLightningIntervalMin = 15;

        /// <summary>Maximum frames between background lightning strikes</summary>
        public int BackgroundLightningIntervalMax = 45;

        /// <summary>How far from the player background lightning can spawn</summary>
        public float BackgroundLightningRadius = 400f;

        /// <summary>Minimum length of background lightning bolts</summary>
        public float BackgroundLightningLengthMin = 150f;

        /// <summary>Maximum length of background lightning bolts</summary>
        public float BackgroundLightningLengthMax = 350f;

        /// <summary>Width of background lightning bolts</summary>
        public float BackgroundLightningWidth = 4f;

        /// <summary>How long background lightning stays visible (frames)</summary>
        public int BackgroundLightningDuration = 8;

        /// <summary>Alpha multiplier for background lightning (0-1)</summary>
        public float BackgroundLightningAlpha = 0.7f;

        /// <summary>Jaggedness of background lightning bolts</summary>
        public float BackgroundLightningJaggedness = 60f;

        /// <summary>Number of segments in background lightning</summary>
        public int BackgroundLightningSegments = 8;

        #endregion

        // Internal state for the lightning effect
        private Vector2 lungeStartPosition;
        private bool hasTriggeredLightning = false;
        private int lightningTimer = 0;
        private List<LightningBolt> activeLightningBolts = new List<LightningBolt>();
        private HashSet<int> hitNPCs = new HashSet<int>();

        // Charging glow state
        private List<BackgroundLightningBolt> backgroundLightning = new List<BackgroundLightningBolt>();
        private int nextBackgroundLightningTime = 0;
        private bool wasFullyCharged = false;

        // Background lightning bolt data
        private class BackgroundLightningBolt
        {
            public List<Vector2> Points;
            public int Timer;
            public int MaxTimer;

            public BackgroundLightningBolt()
            {
                Points = new List<Vector2>();
            }
        }

        // Lightning bolt data structure
        private class LightningBolt
        {
            public List<Vector2> Points;
            public float Width;
            public float GlowWidth;
            public float BloomWidth;
            public int Depth;

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
            // Capture the starting position before the lunge
            lungeStartPosition = Owner.Center;
            hasTriggeredLightning = false;
            lightningTimer = 0;
            activeLightningBolts.Clear();
            hitNPCs.Clear();
            backgroundLightning.Clear();
            wasFullyCharged = false;
            base.PerformLunge();
        }

        internal override void HandleChargingProjectileVisuals()
        {
            base.HandleChargingProjectileVisuals();

            bool isFullyCharged = currentChargeTime >= ChargeTime;

            // Handle background lightning when fully charged
            if (isFullyCharged)
            {
                // Play thunder sound on first frame of being fully charged
                if (!wasFullyCharged)
                {
                    wasFullyCharged = true;
                    float pitch = -ThunderPitchVariance + Main.rand.NextFloat() * (ThunderPitchVariance * 2f);
                    SoundEngine.PlaySound(SoundID.Thunder with { Volume = ThunderVolume * 0.6f, Pitch = pitch + 0.2f }, Owner.Center);
                    nextBackgroundLightningTime = 0; // Spawn lightning immediately
                }

                // Update background lightning timers
                for (int i = backgroundLightning.Count - 1; i >= 0; i--)
                {
                    backgroundLightning[i].Timer--;
                    if (backgroundLightning[i].Timer <= 0)
                    {
                        backgroundLightning.RemoveAt(i);
                    }
                }

                // Spawn new background lightning
                if (nextBackgroundLightningTime <= 0)
                {
                    SpawnBackgroundLightning();
                    nextBackgroundLightningTime = Main.rand.Next(BackgroundLightningIntervalMin, BackgroundLightningIntervalMax + 1);
                }
                else
                {
                    nextBackgroundLightningTime--;
                }
            }
        }

        internal override void HandleProjectileVisuals()
        {
            base.HandleProjectileVisuals();

            // Update lightning effect
            if (lightningTimer > 0)
            {
                lightningTimer--;
                DamageNPCsAlongLightning();
            }

            if (currentDashTime >= DashTime)
            {
                Owner.velocity *= 0.5f;

                // Trigger the lightning effect when lunge ends
                if (!hasTriggeredLightning)
                {
                    TriggerLightningEffect();
                    hasTriggeredLightning = true;
                }
            }
        }

        private void TriggerLightningEffect()
        {
            Vector2 startPos = lungeStartPosition;
            Vector2 endPos = Owner.Center;

            // Generate the main lightning bolt and all branches
            GenerateLightningBolts(startPos, endPos);

            // Play thunder sound with variance
            float pitch = -ThunderPitchVariance + Main.rand.NextFloat() * (ThunderPitchVariance * 2f);
            SoundEngine.PlaySound(SoundID.Thunder with { Volume = ThunderVolume, Pitch = pitch }, Owner.Center);

            // Screen shake
            if (Main.LocalPlayer.Distance(Owner.Center) < 2000f)
            {
                Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(ScreenShakeIntensity, ScreenShakeIntensity);
            }

            // Spawn impact dust at both endpoints
            SpawnImpactDust(startPos);
            SpawnImpactDust(endPos);

            // Spawn dust along all lightning segments
            foreach (var bolt in activeLightningBolts)
            {
                SpawnLightningDust(bolt);
            }

            // Start the lightning visual timer
            lightningTimer = LightningDuration;
        }

        private void GenerateLightningBolts(Vector2 start, Vector2 end)
        {
            activeLightningBolts.Clear();

            float distance = Vector2.Distance(start, end);
            int segments = LightningBaseSegments + (int)(distance * LightningSegmentsPerDistance);

            // Generate main bolt
            LightningBolt mainBolt = GenerateBolt(start, end, segments, LightningJaggednessMagnitude, 0);
            mainBolt.Width = LightningCoreWidth;
            mainBolt.GlowWidth = LightningGlowWidth;
            mainBolt.BloomWidth = LightningBloomWidth;
            activeLightningBolts.Add(mainBolt);

            // Generate branches recursively
            GenerateBranches(mainBolt, 0);
        }

        private LightningBolt GenerateBolt(Vector2 start, Vector2 end, int segments, float jaggedness, int depth)
        {
            LightningBolt bolt = new LightningBolt();
            bolt.Depth = depth;
            bolt.Points.Add(start);

            Vector2 direction = end - start;
            float length = direction.Length();
            Vector2 normalizedDir = direction / length;
            Vector2 perpendicular = new Vector2(-normalizedDir.Y, normalizedDir.X);

            // Create displacement values using midpoint displacement algorithm
            float[] displacements = new float[segments + 1];
            displacements[0] = 0;
            displacements[segments] = 0;

            // Midpoint displacement
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

            // Build points
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

        private void GenerateBranches(LightningBolt parentBolt, int depth)
        {
            if (depth >= MaxBranchDepth) return;

            float currentBranchChance = BranchChance * MathF.Pow(BranchChanceDecay, depth);

            for (int i = 1; i < parentBolt.Points.Count - 1; i++)
            {
                if (Main.rand.NextFloat() < currentBranchChance)
                {
                    Vector2 branchStart = parentBolt.Points[i];

                    // Calculate direction from previous point
                    Vector2 direction = parentBolt.Points[i] - parentBolt.Points[i - 1];
                    if (direction == Vector2.Zero) continue;

                    float baseAngle = direction.ToRotation();
                    float branchAngle = baseAngle + (Main.rand.NextFloat() * 2f - 1f) * BranchAngleDeviation;

                    // Calculate remaining distance to end
                    float remainingDist = 0f;
                    for (int j = i; j < parentBolt.Points.Count - 1; j++)
                    {
                        remainingDist += Vector2.Distance(parentBolt.Points[j], parentBolt.Points[j + 1]);
                    }

                    float branchLength = remainingDist * MathHelper.Lerp(BranchLengthMin, BranchLengthMax, Main.rand.NextFloat());
                    Vector2 branchEnd = branchStart + branchAngle.ToRotationVector2() * branchLength;

                    // Generate branch bolt
                    float branchJaggedness = LightningJaggednessMagnitude * BranchJaggednessMultiplier * MathF.Pow(0.7f, depth);
                    LightningBolt branch = GenerateBolt(branchStart, branchEnd, BranchSegments, branchJaggedness, depth + 1);

                    float widthMult = MathF.Pow(BranchWidthMultiplier, depth + 1);
                    branch.Width = LightningCoreWidth * widthMult;
                    branch.GlowWidth = LightningGlowWidth * widthMult;
                    branch.BloomWidth = LightningBloomWidth * widthMult;

                    activeLightningBolts.Add(branch);

                    // Recursively add sub-branches
                    GenerateBranches(branch, depth + 1);
                }
            }
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

                        // Check if NPC is near this segment
                        float distToSeg = DistanceToLineSegment(npc.Center, segStart, segEnd);
                        if (distToSeg <= LightningHitRadius + (npc.width + npc.height) / 4f)
                        {
                            // Calculate hit direction
                            Vector2 hitDir = (npc.Center - segCenter).SafeNormalize(Vector2.UnitX);

                            // Determine crit
                            bool crit = false;
                            if (LightningCanCrit)
                            {
                                int critChance = Owner.GetWeaponCrit(Owner.HeldItem) + LightningBonusCrit;
                                crit = Main.rand.Next(100) < critChance;
                            }

                            // Apply damage
                            npc.SimpleStrikeNPC(damage, hitDir.X > 0 ? 1 : -1, crit, LightningKnockback, DasherDamageClass.Instance);

                            hitNPCs.Add(npc.whoAmI);

                            // Extra hit effects
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
            for (int i = 0; i < ImpactDustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(DustSpreadSpeed, DustSpreadSpeed);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.GemDiamond, velocity.X, velocity.Y, 100, ImpactDustColor, DustScale * 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;

                // Also spawn some secondary colored dust
                if (i % 3 == 0)
                {
                    Dust dust2 = Dust.NewDustDirect(position, 0, 0, DustID.GemDiamond, velocity.X * 0.8f, velocity.Y * 0.8f, 150, ImpactDustSecondaryColor, DustScale);
                    dust2.noGravity = true;
                }
            }

            // Bright flash at endpoint
            for (int i = 0; i < 5; i++)
            {
                Dust flash = Dust.NewDustDirect(position - new Vector2(EndpointFlashRadius / 2), (int)EndpointFlashRadius, (int)EndpointFlashRadius, DustID.GemDiamond, 0, 0, 0, LightningCoreColor, DustScale * 2f);
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

                for (int j = 0; j < DustPerSegment; j++)
                {
                    float t = Main.rand.NextFloat();
                    Vector2 pos = Vector2.Lerp(segStart, segEnd, t);
                    Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);

                    Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.GemDiamond, velocity.X, velocity.Y, 100, LightningDustColor, DustScale * (1f - bolt.Depth * 0.2f));
                    dust.noGravity = true;
                }
            }
        }

        private void SpawnHitDust(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.GemDiamond, velocity.X, velocity.Y, 100, HitDustColor, DustScale);
                dust.noGravity = true;
            }
        }

        #region Drawing

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw background lightning (when charging and fully charged)
            if (backgroundLightning.Count > 0)
            {
                DrawBackgroundLightning();
            }

            // Draw charging glow effect
            if (!isMidlunge && currentChargeTime > 0)
            {
                DrawChargingGlow();
            }

            // Draw lightning effect behind the projectile
            if (lightningTimer > 0 && activeLightningBolts.Count > 0)
            {
                DrawLightningBolts();
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
                // Fluctuating glow when fully charged
                float time = (float)Main.GameUpdateCount * ChargedGlowFluctuationSpeed * 0.1f;
                float fluctuation = 1f + MathF.Sin(time) * ChargedGlowFluctuationAmount;
                float fluctuation2 = 1f + MathF.Sin(time * 1.7f + 1.3f) * ChargedGlowFluctuationAmount * 0.5f;
                glowIntensity *= ChargedGlowBoostMultiplier * fluctuation * fluctuation2;
            }

            // Get the projectile texture and frame
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteEffects directionEffect = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // Calculate glow alpha
            float glowAlpha = MathHelper.Lerp(ChargingGlowAlphaMin, ChargingGlowAlphaMax, glowIntensity);

            // Calculate scale boost for the glow layers
            float scaleBoost = MathHelper.Lerp(ChargingGlowScaleMin, ChargingGlowScaleMax, chargeProgress);
            if (isFullyCharged)
            {
                float time = (float)Main.GameUpdateCount * ChargedGlowFluctuationSpeed * 0.1f;
                scaleBoost *= 1f + MathF.Sin(time * 0.8f + 0.5f) * ChargedGlowFluctuationAmount * 0.2f;
            }

            // Draw multiple glow layers using the projectile sprite itself
            int layers = isFullyCharged ? 5 : 3;
            for (int layer = layers - 1; layer >= 0; layer--)
            {
                float layerProgress = (float)layer / Math.Max(layers - 1, 1);

                // Outer layers are larger and more transparent
                float layerScale = Projectile.scale * (1f + scaleBoost * (0.3f + layerProgress * 0.7f));
                float layerAlpha = glowAlpha * (1f - layerProgress * 0.6f);

                // Interpolate color from inner to outer
                Color layerColor = Color.Lerp(ChargingGlowColorInner, ChargingGlowColorOuter, layerProgress);
                layerColor = layerColor * layerAlpha;

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    frame,
                    layerColor,
                    Projectile.rotation,
                    origin,
                    layerScale,
                    directionEffect,
                    0
                );
            }

            // Draw extra bright core layer when fully charged
            if (isFullyCharged)
            {
                float time = (float)Main.GameUpdateCount * ChargedGlowFluctuationSpeed * 0.1f;
                float coreFluctuation = 0.7f + MathF.Sin(time * 2f) * 0.3f;
                float coreAlpha = 0.4f * coreFluctuation * glowIntensity;

                Color coreColor = LightningCoreColor * coreAlpha;

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    frame,
                    coreColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * 1.05f,
                    directionEffect,
                    0
                );
            }
        }

        private void SpawnBackgroundLightning()
        {
            // Pick a random position around the player
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float distance = Main.rand.NextFloat(BackgroundLightningRadius * 0.3f, BackgroundLightningRadius);
            Vector2 startPos = Owner.Center + angle.ToRotationVector2() * distance;

            // Lightning goes in a generally downward direction with some randomness
            float lightningAngle = MathHelper.PiOver2 + (Main.rand.NextFloat() - 0.5f) * 1.2f;
            float length = Main.rand.NextFloat(BackgroundLightningLengthMin, BackgroundLightningLengthMax);
            Vector2 endPos = startPos + lightningAngle.ToRotationVector2() * length;

            // Generate the bolt
            BackgroundLightningBolt bolt = new BackgroundLightningBolt();
            bolt.Timer = BackgroundLightningDuration;
            bolt.MaxTimer = BackgroundLightningDuration;
            bolt.Points.Add(startPos);

            Vector2 direction = endPos - startPos;
            float boltLength = direction.Length();
            Vector2 normalizedDir = direction / boltLength;
            Vector2 perpendicular = new Vector2(-normalizedDir.Y, normalizedDir.X);

            // Simple jagged path
            for (int i = 1; i < BackgroundLightningSegments; i++)
            {
                float t = (float)i / BackgroundLightningSegments;
                Vector2 basePos = Vector2.Lerp(startPos, endPos, t);
                float offset = (Main.rand.NextFloat() * 2f - 1f) * BackgroundLightningJaggedness * (1f - MathF.Pow(t - 0.5f, 2f) * 2f);
                bolt.Points.Add(basePos + perpendicular * offset);
            }
            bolt.Points.Add(endPos);

            backgroundLightning.Add(bolt);

            // Play a quieter thunder sound
            if (Main.rand.NextFloat() < 0.3f)
            {
                float pitch = 0.3f + Main.rand.NextFloat() * 0.4f;
                SoundEngine.PlaySound(SoundID.Thunder with { Volume = ThunderVolume * 0.25f, Pitch = pitch }, startPos);
            }
        }

        private void DrawBackgroundLightning()
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            foreach (var bolt in backgroundLightning)
            {
                float fade = (float)bolt.Timer / bolt.MaxTimer;
                float fadeEased = MathF.Pow(fade, 0.3f);

                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    Vector2 start = bolt.Points[i] - Main.screenPosition;
                    Vector2 end = bolt.Points[i + 1] - Main.screenPosition;

                    Vector2 direction = end - start;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();

                    // Draw glow
                    Color glowColor = LightningGlowColor;
                    glowColor.A = (byte)(glowColor.A * fadeEased * BackgroundLightningAlpha * 0.5f);
                    DrawLightningSegment(pixel, start, length, rotation, BackgroundLightningWidth * 3f * fadeEased, glowColor);

                    // Draw core
                    Color coreColor = LightningCoreColor;
                    coreColor.A = (byte)(coreColor.A * fadeEased * BackgroundLightningAlpha);
                    DrawLightningSegment(pixel, start, length, rotation, BackgroundLightningWidth * fadeEased, coreColor);
                }
            }
        }

        private void DrawLightningBolts()
        {
            // Calculate fade based on remaining time
            float fade = (float)lightningTimer / LightningDuration;
            float fadeEased = MathF.Pow(fade, 0.5f); // Ease out for smoother fade

            // Get the pixel texture for drawing lines
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            // Draw each bolt in reverse order (main bolt last, so it's on top)
            for (int b = activeLightningBolts.Count - 1; b >= 0; b--)
            {
                LightningBolt bolt = activeLightningBolts[b];
                float depthFade = 1f - (bolt.Depth * 0.15f);

                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    Vector2 start = bolt.Points[i] - Main.screenPosition;
                    Vector2 end = bolt.Points[i + 1] - Main.screenPosition;

                    Vector2 direction = end - start;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();

                    // Add some color variation per segment
                    Color coreColor = AddColorVariation(LightningCoreColor, ColorVariation);
                    Color glowColor = AddColorVariation(LightningGlowColor, ColorVariation * 0.5f);
                    Color bloomColor = AddColorVariation(LightningBloomColor, ColorVariation * 0.3f);

                    // Apply fade and depth using alpha, not color multiplication
                    float coreFade = fadeEased * depthFade;
                    float glowFade = fadeEased * depthFade;
                    float bloomFade = fadeEased * depthFade * 0.7f;
                    
                    coreColor.A = (byte)Math.Clamp(coreColor.A * coreFade, 0, 255);
                    glowColor.A = (byte)Math.Clamp(glowColor.A * glowFade, 0, 255);
                    bloomColor.A = (byte)Math.Clamp(bloomColor.A * bloomFade, 0, 255);

                    // Draw outer bloom (largest, most transparent)
                    DrawLightningSegment(pixel, start, length, rotation, bolt.BloomWidth * fadeEased, bloomColor);

                    // Draw inner glow
                    DrawLightningSegment(pixel, start, length, rotation, bolt.GlowWidth * fadeEased, glowColor);

                    // Draw core (brightest)
                    DrawLightningSegment(pixel, start, length, rotation, bolt.Width, coreColor);
                }
            }

            // Draw endpoint flashes
            if (fadeEased > 0.3f)
            {
                float flashFade = (fadeEased - 0.3f) / 0.7f;
                DrawEndpointFlash(lungeStartPosition, flashFade);
                DrawEndpointFlash(Owner.Center, flashFade);
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

        private void DrawEndpointFlash(Vector2 worldPosition, float fade)
        {
            Vector2 screenPos = worldPosition - Main.screenPosition;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            float radius = EndpointFlashRadius * EndpointFlashIntensity * fade;

            // Draw multiple layers for a bloom effect
            for (int layer = 3; layer >= 0; layer--)
            {
                float layerRadius = radius * (1f + layer * 0.5f);
                float layerAlpha = fade * (0.3f - layer * 0.07f);

                Color flashColor = Color.Lerp(LightningCoreColor, LightningGlowColor, layer * 0.25f);
                flashColor.A = (byte)Math.Clamp(flashColor.A * layerAlpha, 0, 255);

                // Draw a circular flash using scaled squares (approximation)
                Vector2 scale = new Vector2(layerRadius * 2 / pixel.Width, layerRadius * 2 / pixel.Height);
                Main.EntitySpriteDraw(
                    pixel,
                    screenPos,
                    null,
                    flashColor,
                    MathHelper.PiOver4, // Rotate 45 degrees for diamond shape
                    new Vector2(pixel.Width / 2f, pixel.Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        private Color AddColorVariation(Color baseColor, float variation)
        {
            int r = Math.Clamp((int)(baseColor.R + Main.rand.NextFloat(-variation, variation)), 0, 255);
            int g = Math.Clamp((int)(baseColor.G + Main.rand.NextFloat(-variation, variation)), 0, 255);
            int b = Math.Clamp((int)(baseColor.B + Main.rand.NextFloat(-variation, variation)), 0, 255);
            return new Color(r, g, b, baseColor.A);
        }

        #endregion

        #region NPC Hit Collision Logic

        // public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ReelBack();
        #endregion
    }
}