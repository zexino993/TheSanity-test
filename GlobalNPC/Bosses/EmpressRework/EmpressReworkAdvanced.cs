using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TheSanity.GlobalNPC.Bosses.EmpressRework
{
    // Main Empress Rework Configuration
    public static class EmpressConfig
    {
        public enum DifficultyMode { Easy = 0, Normal = 1, Hard = 2, Extreme = 3 }
        public static DifficultyMode CurrentDifficulty = DifficultyMode.Normal;

        // Difficulty Settings
        public static float GetHealthMultiplier()
        {
            return CurrentDifficulty switch
            {
                DifficultyMode.Easy => 0.7f,
                DifficultyMode.Normal => 1.0f,
                DifficultyMode.Hard => 1.5f,
                DifficultyMode.Extreme => 2.0f,
                _ => 1.0f
            };
        }

        public static float GetDamageMultiplier()
        {
            return CurrentDifficulty switch
            {
                DifficultyMode.Easy => 0.6f,
                DifficultyMode.Normal => 1.0f,
                DifficultyMode.Hard => 1.4f,
                DifficultyMode.Extreme => 1.8f,
                _ => 1.0f
            };
        }

        public static float GetAttackSpeedMultiplier()
        {
            return CurrentDifficulty switch
            {
                DifficultyMode.Easy => 0.8f,
                DifficultyMode.Normal => 1.0f,
                DifficultyMode.Hard => 1.3f,
                DifficultyMode.Extreme => 1.6f,
                _ => 1.0f
            };
        }
    }

    // Boss Stats & Modification
    public class EmpressStatsModifier : GlobalNPC
    {
        public override void ModifyNPC(NPC npc)
        {
            if (npc.type == NPCID.HallowBoss)
            {
                float healthMult = EmpressConfig.GetHealthMultiplier();
                float damageMult = EmpressConfig.GetDamageMultiplier();

                npc.lifeMax = (int)(4200 * healthMult);
                npc.damage = (int)(55 * damageMult);
                npc.defense = (int)(15 + (5 * (int)EmpressConfig.CurrentDifficulty));
            }
        }
    }

    // Advanced AI System
    public class EmpressAdvancedAI : GlobalNPC
    {
        // Phase Management
        private float phase1AttackCounter = 0;
        private float phase2AttackCounter = 0;
        private float phase3AttackCounter = 0;
        private float telegraphDelay = 0f;
        private int currentAttackType = 0;
        private float enrageTimer = 0f;
        private float maxEnrageTime = 3600f; // 60 seconds at 60 FPS
        private bool isEnraged = false;

        // AI Behavior
        private Vector2 targetPosition = Vector2.Zero;
        private float movementCounter = 0f;
        private List<Vector2> dodgePositions = new List<Vector2>();
        private float dodgeCounter = 0f;
        private bool isDodging = false;
        private float healthThreshold = 0f;

        // Statistics
        private int totalAttacksLanded = 0;
        private float totalDamageDealt = 0f;
        private float battleDuration = 0f;

        public override void AI(NPC npc)
        {
            if (npc.type != NPCID.HallowBoss)
                return;

            battleDuration++;
            enrageTimer++;

            float healthPercent = npc.life / (float)npc.lifeMax;
            healthThreshold = healthPercent;

            // Enrage System
            if (enrageTimer > maxEnrageTime)
            {
                isEnraged = true;
                ApplyEnrageEffects(npc);
            }

            // Determine Phase
            if (healthPercent > 0.66f)
            {
                ExecutePhase1AI(npc, healthPercent);
            }
            else if (healthPercent > 0.33f)
            {
                ExecutePhase2AI(npc, healthPercent);
            }
            else
            {
                ExecutePhase3AI(npc, healthPercent);
            }

            // Advanced Movement AI
            UpdateMovementAI(npc);
            UpdateDodgeAI(npc);
        }

        private void ApplyEnrageEffects(NPC npc)
        {
            // Visual effects untuk enrage
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustPos = npc.Center + new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50));
                Dust.NewDustDirect(dustPos, 10, 10, DustID.RedTorch, 0f, 0f, 200);
            }

            // Speed up attack cadence
            phase1AttackCounter *= 0.8f;
            phase2AttackCounter *= 0.8f;
            phase3AttackCounter *= 0.8f;
        }

        private void UpdateMovementAI(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            movementCounter++;

            // Intelligent positioning every 30 frames
            if (movementCounter > 30)
            {
                movementCounter = 0;

                // Calculate optimal distance
                float optimalDistance = 200f + (50f * (int)EmpressConfig.CurrentDifficulty);
                float distanceToPlayer = Vector2.Distance(npc.Center, target.Center);

                if (distanceToPlayer > optimalDistance + 100)
                {
                    // Move closer
                    targetPosition = target.Center - Vector2.Normalize(target.Center - npc.Center) * optimalDistance;
                }
                else if (distanceToPlayer < optimalDistance - 100)
                {
                    // Move away
                    targetPosition = target.Center + Vector2.Normalize(target.Center - npc.Center) * optimalDistance;
                }
                else
                {
                    // Circle around player
                    float angle = (float)Main.rand.NextDouble() * MathHelper.TwoPi;
                    targetPosition = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * optimalDistance;
                }
            }

            // Smooth movement to target
            Vector2 moveDirection = Vector2.Normalize(targetPosition - npc.Center);
            float moveSpeed = 5f + (2f * (int)EmpressConfig.CurrentDifficulty);
            npc.velocity = Vector2.Lerp(npc.velocity, moveDirection * moveSpeed, 0.1f);
        }

        private void UpdateDodgeAI(NPC npc)
        {
            // Every 60 frames, check for player projectiles to dodge
            dodgeCounter++;
            if (dodgeCounter > 60)
            {
                dodgeCounter = 0;

                // Check nearby projectiles
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.hostile && Vector2.Distance(proj.Center, npc.Center) < 300)
                    {
                        isDodging = true;
                        Vector2 dodgeDir = Vector2.Normalize(npc.Center - proj.Center);
                        npc.velocity += dodgeDir * 8f;
                        break;
                    }
                }
            }

            if (isDodging)
            {
                dodgeCounter = 30; // Extend dodge time
                if (dodgeCounter <= 0)
                    isDodging = false;
            }
        }

        // ===== PHASE 1 AI =====
        private void ExecutePhase1AI(NPC npc, float healthPercent)
        {
            phase1AttackCounter++;
            float attackSpeedMult = EmpressConfig.GetAttackSpeedMultiplier();
            float cooldown = 90f / attackSpeedMult;

            if (phase1AttackCounter >= cooldown)
            {
                phase1AttackCounter = 0;
                currentAttackType = Main.rand.Next(0, 15); // 15 attacks untuk phase 1
                telegraphDelay = 30f;
            }

            HandleTelegraph(npc, telegraphDelay);

            if (telegraphDelay <= 0)
            {
                switch (currentAttackType)
                {
                    case 0: Phase1_HomingBurstAttack(npc); break;
                    case 1: Phase1_LaserBeamPattern(npc); break;
                    case 2: Phase1_ChargeAttack(npc); break;
                    case 3: Phase1_ProjectileCircle(npc); break;
                    case 4: Phase1_WavePattern(npc); break;
                    case 5: Phase1_SpinAttack(npc); break;
                    case 6: Phase1_TargetedSpray(npc); break;
                    case 7: Phase1_DiagonalWave(npc); break;
                    case 8: Phase1_RandomBurst(npc); break;
                    case 9: Phase1_SweepAttack(npc); break;
                    case 10: Phase1_DoubleLaser(npc); break;
                    case 11: Phase1_TripleHoming(npc); break;
                    case 12: Phase1_ExpandingCircle(npc); break;
                    case 13: Phase1_CrossBurst(npc); break;
                    case 14: Phase1_CurvedWave(npc); break;
                }
                totalAttacksLanded++;
            }

            telegraphDelay--;
        }

        // ===== PHASE 2 AI =====
        private void ExecutePhase2AI(NPC npc, float healthPercent)
        {
            phase2AttackCounter++;
            float attackSpeedMult = EmpressConfig.GetAttackSpeedMultiplier();
            float cooldown = 75f / attackSpeedMult;

            if (phase2AttackCounter >= cooldown)
            {
                phase2AttackCounter = 0;
                currentAttackType = Main.rand.Next(0, 18); // 18 attacks untuk phase 2
                telegraphDelay = 25f;
            }

            HandleTelegraph(npc, telegraphDelay);

            if (telegraphDelay <= 0)
            {
                switch (currentAttackType)
                {
                    case 0: Phase2_ComboAttack(npc); break;
                    case 1: Phase2_ScreenFillingProjectiles(npc); break;
                    case 2: Phase2_DashAttack(npc); break;
                    case 3: Phase2_CrossLaser(npc); break;
                    case 4: Phase2_SpiralPattern(npc); break;
                    case 5: Phase2_DoubleWave(npc); break;
                    case 6: Phase2_ChaoticBurst(npc); break;
                    case 7: Phase2_TripleDash(npc); break;
                    case 8: Phase2_VortexAttack(npc); break;
                    case 9: Phase2_PulseWave(npc); break;
                    case 10: Phase2_TrackingShots(npc); break;
                    case 11: Phase2_WallOfProjectiles(npc); break;
                    case 12: Phase2_ZigZagPattern(npc); break;
                    case 13: Phase2_StarBurst(npc); break;
                    case 14: Phase2_RippleAttack(npc); break;
                    case 15: Phase2_QuadCombo(npc); break;
                    case 16: Phase2_SweepVortex(npc); break;
                    case 17: Phase2_DenseSpiral(npc); break;
                }
                totalAttacksLanded++;
            }

            telegraphDelay--;
        }

        // ===== PHASE 3 AI (EXTREME) =====
        private void ExecutePhase3AI(NPC npc, float healthPercent)
        {
            phase3AttackCounter++;
            float attackSpeedMult = EmpressConfig.GetAttackSpeedMultiplier();
            float cooldown = 60f / attackSpeedMult;

            if (phase3AttackCounter >= cooldown)
            {
                phase3AttackCounter = 0;
                currentAttackType = Main.rand.Next(0, 20); // 20 attacks untuk phase 3
                telegraphDelay = 20f;
            }

            HandleTelegraph(npc, telegraphDelay);

            if (telegraphDelay <= 0)
            {
                switch (currentAttackType)
                {
                    case 0: Phase3_ExtremeCombo(npc); break;
                    case 1: Phase3_ProjectileHell(npc); break;
                    case 2: Phase3_MultiDash(npc); break;
                    case 3: Phase3_LaserGrid(npc); break;
                    case 4: Phase3_ChaosPattern(npc); break;
                    case 5: Phase3_UltimateBurst(npc); break;
                    case 6: Phase3_InfernoPhase(npc); break;
                    case 7: Phase3_OmnidirectionalBlast(npc); break;
                    case 8: Phase3_SplittingProjectiles(npc); break;
                    case 9: Phase3_MatrixAttack(npc); break;
                    case 10: Phase3_CylinderBarrage(npc); break;
                    case 11: Phase3_ExplosivePattern(npc); break;
                    case 12: Phase3_SupercomboAttack(npc); break;
                    case 13: Phase3_WaveSlash(npc); break;
                    case 14: Phase3_DensityBomb(npc); break;
                    case 15: Phase3_CrescentMoon(npc); break;
                    case 16: Phase3_HelixBarrage(npc); break;
                    case 17: Phase3_RingOfFire(npc); break;
                    case 18: Phase3_MeteoralStrike(npc); break;
                    case 19: Phase3_FinalPhaseBlast(npc); break;
                }
                totalAttacksLanded++;
            }

            telegraphDelay--;
        }

        private void HandleTelegraph(NPC npc, float delay)
        {
            if (delay > 0)
            {
                int glowIntensity = (int)(255 * (1f - (delay / 30f)));
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Teleport, 0f, 0f, glowIntensity);
                        dust.velocity *= 0.5f;
                    }
                }

                // Screen shake effect
                if (delay < 10f)
                {
                    Main.screenPosition += new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                }
            }
        }

        // ===== PHASE 1 ATTACKS (15 total) =====
        private void Phase1_HomingBurstAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                direction = direction.RotatedBy(MathHelper.TwoPi / 8 * i);
                SpawnProjectile(npc, npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 20);
            }
        }

        private void Phase1_LaserBeamPattern(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = direction.RotatedBy(MathHelper.Pi / 8 * (i - 1));
                SpawnProjectile(npc, npc.Center, offset * 10f, ProjectileID.FairyQueenMagicProjectile, 22);
            }
        }

        private void Phase1_ChargeAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;
            npc.velocity = Vector2.Normalize(target.Center - npc.Center) * 12f;
        }

        private void Phase1_ProjectileCircle(NPC npc)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 12 * i);
                SpawnProjectile(npc, npc.Center, direction * 7f, ProjectileID.FairyQueenMagicProjectile, 18);
            }
        }

        private void Phase1_WavePattern(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 5; i++)
            {
                SpawnProjectile(npc, npc.Center + direction * 20 * i, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 20);
            }
        }

        private void Phase1_SpinAttack(NPC npc)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 direction = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 16 * i);
                SpawnProjectile(npc, npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 19);
            }
        }

        private void Phase1_TargetedSpray(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int i = 0; i < 6; i++)
            {
                Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                direction = direction.RotatedBy((Main.rand.NextFloat() - 0.5f) * 0.5f);
                SpawnProjectile(npc, npc.Center, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 21);
            }
        }

        private void Phase1_DiagonalWave(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector2 dir = baseDir.RotatedBy(MathHelper.PiOver4 * (i - 1.5f) + (j - 1) * 0.2f);
                    SpawnProjectile(npc, npc.Center, dir * 8f, ProjectileID.FairyQueenMagicProjectile, 20);
                }
            }
        }

        private void Phase1_RandomBurst(NPC npc)
        {
            for (int i = 0; i < 10; i++)
            {
                float randomAngle = (float)Main.rand.NextDouble() * MathHelper.TwoPi;
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
                SpawnProjectile(npc, npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 20);
            }
        }

        private void Phase1_SweepAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 14; i++)
            {
                Vector2 dir = baseDir.RotatedBy((i - 7) * 0.1f);
                SpawnProjectile(npc, npc.Center, dir * 9f, ProjectileID.FairyQueenMagicProjectile, 21);
            }
        }

        private void Phase1_DoubleLaser(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

            for (int i = 0; i < 4; i++)
            {
                SpawnProjectile(npc, npc.Center + perpendicular * 30, direction * 10f, ProjectileID.FairyQueenMagicProjectile, 22);
                SpawnProjectile(npc, npc.Center - perpendicular * 30, direction * 10f, ProjectileID.FairyQueenMagicProjectile, 22);
            }
        }

        private void Phase1_TripleHoming(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int s = 0; s < 3; s++)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                    direction = direction.RotatedBy(MathHelper.TwoPi / 6 * i + s * MathHelper.Pi / 3);
                    SpawnProjectile(npc, npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 20);
                }
            }
        }

        private void Phase1_ExpandingCircle(NPC npc)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 20 * i);
                SpawnProjectile(npc, npc.Center, direction * (5f + i * 0.3f), ProjectileID.FairyQueenMagicProjectile, 19);
            }
        }

        private void Phase1_CrossBurst(NPC npc)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 direction = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i);
                for (int j = 0; j < 5; j++)
                {
                    SpawnProjectile(npc, npc.Center, direction * (8f + j * 1f), ProjectileID.FairyQueenMagicProjectile, 20);
                }
            }
        }

        private void Phase1_CurvedWave(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 7; i++)
            {
                Vector2 dir = baseDir.RotatedBy(Math.Sin(i * 0.5f) * 0.3f);
                SpawnProjectile(npc, npc.Center, dir * 9f, ProjectileID.FairyQueenMagicProjectile, 21);
            }
        }

        // ===== PHASE 2 ATTACKS (18 total) =====
        private void Phase2_ComboAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int burst = 0; burst < 2; burst++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                    direction = direction.RotatedBy(MathHelper.TwoPi / 10 * i);
                    SpawnProjectile(npc, npc.Center, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 24);
                }
            }
        }

        private void Phase2_ScreenFillingProjectiles(NPC npc)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 20 * i);
                SpawnProjectile(npc, npc.Center, direction * 10f, ProjectileID.FairyQueenMagicProjectile, 25);
            }
        }

        private void Phase2_DashAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            npc.velocity = Vector2.Normalize(target.Center - npc.Center) * 15f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 8 * i);
                SpawnProjectile(npc, npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 23);
            }
        }

        private void Phase2_CrossLaser(NPC npc)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 cross = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i);
                for (int j = 0; j < 6; j++)
                {
                    SpawnProjectile(npc, npc.Center, cross * 10f, ProjectileID.FairyQueenMagicProjectile, 24);
                }
            }
        }

        private void Phase2_SpiralPattern(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 15; i++)
            {
                Vector2 spiral = direction.RotatedBy(MathHelper.TwoPi / 15 * i);
                SpawnProjectile(npc, npc.Center, spiral * (7f + i * 0.5f), ProjectileID.FairyQueenMagicProjectile, 24);
            }
        }

        private void Phase2_DoubleWave(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            for (int w = 0; w < 2; w++)
            {
                for (int i = 0; i < 8; i++)
                {
                    SpawnProjectile(npc, npc.Center, direction.RotatedBy(MathHelper.Pi / 12 * (i - 4 + w * 2)) * 10f, ProjectileID.FairyQueenMagicProjectile, 25);
                }
            }
        }

        private void Phase2_ChaoticBurst(NPC npc)
        {
            for (int i = 0; i < 25; i++)
            {
                Vector2 direction = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                    SpawnProjectile(npc, npc.Center, direction * 10f, ProjectileID.FairyQueenMagicProjectile, 24);
                }
            }
        }

        private void Phase2_TripleDash(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int d = 0; d < 3; d++)
            {
                Vector2 dashDir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(MathHelper.TwoPi / 3 * d);
                npc.velocity = dashDir * 14f;
                for (int i = 0; i < 6; i++)
                {
                    SpawnProjectile(npc, npc.Center, dashDir.RotatedBy(MathHelper.TwoPi / 6 * i) * 9f, ProjectileID.FairyQueenMagicProjectile, 23);
                }
            }
        }

        private void Phase2_VortexAttack(NPC npc)
        {
            for (int i = 0; i < 18; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 18 * i);
                float speed = 7f + (i % 3) * 2f;
                SpawnProjectile(npc, npc.Center, direction * speed, ProjectileID.FairyQueenMagicProjectile, 24);
            }
        }

        private void Phase2_PulseWave(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int pulse = 0; pulse < 3; pulse++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 dir = baseDir.RotatedBy(MathHelper.TwoPi / 10 * i);
                    SpawnProjectile(npc, npc.Center, dir * (8f + pulse * 1.5f), ProjectileID.FairyQueenMagicProjectile, 24);
                }
            }
        }

        private void Phase2_TrackingShots(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                SpawnProjectile(npc, npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 25);
            }
        }

        private void Phase2_WallOfProjectiles(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            Vector2 perpendicular = new Vector2(-baseDir.Y, baseDir.X);

            for (int i = 0; i < 12; i++)
            {
                Vector2 pos = npc.Center + perpendicular * (i - 6) * 30;
                SpawnProjectile(npc, pos, baseDir * 10f, ProjectileID.FairyQueenMagicProjectile, 24);
            }
        }

        private void Phase2_ZigZagPattern(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 12; i++)
            {
                Vector2 dir = baseDir.RotatedBy(Math.Cos(i * 0.4f) * 0.4f);
                SpawnProjectile(npc, npc.Center, dir * 10f, ProjectileID.FairyQueenMagicProjectile, 24);
            }
        }

        private void Phase2_StarBurst(NPC npc)
        {
            for (int i = 0; i < 24; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 24 * i);
                SpawnProjectile(npc, npc.Center, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 24);
            }
        }

        private void Phase2_RippleAttack(NPC npc)
        {
            for (int ripple = 0; ripple < 2; ripple++)
            {
                for (int i = 0; i < 16; i++)
                {
                    Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 16 * i);
                    SpawnProjectile(npc, npc.Center, direction * (8f + ripple * 2f), ProjectileID.FairyQueenMagicProjectile, 24);
                }
            }
        }

        private void Phase2_QuadCombo(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int q = 0; q < 4; q++)
            {
                Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(MathHelper.PiOver2 * q);
                for (int i = 0; i < 6; i++)
                {
                    SpawnProjectile(npc, npc.Center, baseDir.RotatedBy(i * 0.15f) * 9f, ProjectileID.FairyQueenMagicProjectile, 24);
                }
            }
        }

        private void Phase2_SweepVortex(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 20; i++)
            {
                Vector2 dir = baseDir.RotatedBy((i - 10) * 0.08f);
                SpawnProjectile(npc, npc.Center, dir * 10f, ProjectileID.FairyQueenMagicProjectile, 25);
            }
        }

        private void Phase2_DenseSpiral(NPC npc)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 20 * i);
                SpawnProjectile(npc, npc.Center, direction * (6f + (i % 4) * 1.5f), ProjectileID.FairyQueenMagicProjectile, 23);
            }
        }

        // ===== PHASE 3 ATTACKS (20 total) =====
        private void Phase3_ExtremeCombo(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int combo = 0; combo < 3; combo++)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                    direction = direction.RotatedBy(MathHelper.TwoPi / 12 * i + combo * 0.3f);
                    SpawnProjectile(npc, npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 28);
                }
            }
        }

        private void Phase3_ProjectileHell(NPC npc)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 30 * i);
                SpawnProjectile(npc, npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 29);
            }
        }

        private void Phase3_MultiDash(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int d = 0; d < 3; d++)
            {
                Vector2 dashDir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(MathHelper.TwoPi / 3 * d);
                npc.velocity = dashDir * 16f;
                for (int i = 0; i < 10; i++)
                {
                    SpawnProjectile(npc, npc.Center, dashDir.RotatedBy(MathHelper.TwoPi / 10 * i) * 9f, ProjectileID.FairyQueenMagicProjectile, 27);
                }
            }
        }

        private void Phase3_LaserGrid(NPC npc)
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Vector2 offset = new Vector2((x - 2) * 60, (y - 2) * 60);
                    SpawnProjectile(npc, npc.Center + offset, Vector2.Zero, ProjectileID.FairyQueenMagicProjectile, 30);
                }
            }
        }

        private void Phase3_ChaosPattern(NPC npc)
        {
            for (int i = 0; i < 35; i++)
            {
                Vector2 direction = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                    SpawnProjectile(npc, npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 28);
                }
            }
        }

        private void Phase3_UltimateBurst(NPC npc)
        {
            for (int i = 0; i < 40; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 40 * i);
                SpawnProjectile(npc, npc.Center, direction * 12f, ProjectileID.FairyQueenMagicProjectile, 30);
            }
        }

        private void Phase3_InfernoPhase(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 8; i++)
            {
                SpawnProjectile(npc, npc.Center, direction * 12f, ProjectileID.FairyQueenMagicProjectile, 30);
            }
            for (int s = 0; s < 2; s++)
            {
                Vector2 side = direction.RotatedBy(MathHelper.Pi / 4 * (s == 0 ? 1 : -1));
                for (int i = 0; i < 6; i++)
                {
                    SpawnProjectile(npc, npc.Center, side * 11f, ProjectileID.FairyQueenMagicProjectile, 29);
                }
            }
        }

        private void Phase3_OmnidirectionalBlast(NPC npc)
        {
            for (int i = 0; i < 32; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 32 * i);
                SpawnProjectile(npc, npc.Center, direction * 12f, ProjectileID.FairyQueenMagicProjectile, 30);
            }
        }

        private void Phase3_SplittingProjectiles(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int split = 0; split < 3; split++)
            {
                Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(split * MathHelper.TwoPi / 3);
                for (int i = 0; i < 10; i++)
                {
                    SpawnProjectile(npc, npc.Center, baseDir * (10f + i * 0.5f), ProjectileID.FairyQueenMagicProjectile, 29);
                }
            }
        }

        private void Phase3_MatrixAttack(NPC npc)
        {
            for (int x = -3; x <= 3; x++)
            {
                for (int y = -3; y <= 3; y++)
                {
                    Vector2 pos = npc.Center + new Vector2(x * 50, y * 50);
                    SpawnProjectile(npc, pos, new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3)), ProjectileID.FairyQueenMagicProjectile, 28);
                }
            }
        }

        private void Phase3_CylinderBarrage(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int ring = 0; ring < 3; ring++)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 dir = baseDir.RotatedBy(MathHelper.TwoPi / 12 * i);
                    SpawnProjectile(npc, npc.Center, dir * (10f + ring * 1.5f), ProjectileID.FairyQueenMagicProjectile, 29);
                }
            }
        }

        private void Phase3_ExplosivePattern(NPC npc)
        {
            for (int i = 0; i < 28; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 28 * i);
                SpawnProjectile(npc, npc.Center, direction * 12f, ProjectileID.FairyQueenMagicProjectile, 30);
            }
        }

        private void Phase3_SupercomboAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int combo = 0; combo < 4; combo++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                    direction = direction.RotatedBy(MathHelper.TwoPi / 8 * i + combo * 0.4f);
                    SpawnProjectile(npc, npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 29);
                }
            }
        }

        private void Phase3_WaveSlash(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int wave = 0; wave < 4; wave++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 dir = baseDir.RotatedBy((i - 5) * 0.1f);
                    SpawnProjectile(npc, npc.Center, dir * (10f + wave * 0.8f), ProjectileID.FairyQueenMagicProjectile, 28);
                }
            }
        }

        private void Phase3_DensityBomb(NPC npc)
        {
            for (int i = 0; i < 50; i++)
            {
                float randomAngle = (float)Main.rand.NextDouble() * MathHelper.TwoPi;
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
                SpawnProjectile(npc, npc.Center, direction * (8f + Main.rand.NextFloat() * 4f), ProjectileID.FairyQueenMagicProjectile, 28);
            }
        }

        private void Phase3_CrescentMoon(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 baseDir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 16; i++)
            {
                Vector2 dir = baseDir.RotatedBy((i - 8) * 0.12f);
                SpawnProjectile(npc, npc.Center, dir * 11f, ProjectileID.FairyQueenMagicProjectile, 29);
            }
        }

        private void Phase3_HelixBarrage(NPC npc)
        {
            for (int helix = 0; helix < 3; helix++)
            {
                for (int i = 0; i < 14; i++)
                {
                    Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 14 * i + helix * 0.3f);
                    SpawnProjectile(npc, npc.Center, direction * (10f + (i % 3) * 1f), ProjectileID.FairyQueenMagicProjectile, 29);
                }
            }
        }

        private void Phase3_RingOfFire(NPC npc)
        {
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 20 * i);
                    SpawnProjectile(npc, npc.Center, direction * (10f + ring * 2f), ProjectileID.FairyQueenMagicProjectile, 30);
                }
            }
        }

        private void Phase3_MeteoralStrike(NPC npc)
        {
            for (int meteor = 0; meteor < 6; meteor++)
            {
                Vector2 pos = npc.Center + new Vector2(Main.rand.Next(-400, 400), Main.rand.Next(-400, 400));
                SpawnProjectile(npc, pos, new Vector2(0, 8), ProjectileID.FairyQueenMagicProjectile, 30);
            }
        }

        private void Phase3_FinalPhaseBlast(NPC npc)
        {
            for (int i = 0; i < 60; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 60 * i);
                SpawnProjectile(npc, npc.Center, direction * (11f + (i % 4) * 0.5f), ProjectileID.FairyQueenMagicProjectile, 30);
            }
        }

        // Helper method untuk spawn projectile dengan particle effects
        private void SpawnProjectile(NPC npc, Vector2 position, Vector2 velocity, int projectileType, int damage)
        {
            Projectile.NewProjectile(npc.GetSource_FromAI(), position, velocity, projectileType, damage, 0f);
            
            // Add particle effect
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustDirect(position - Vector2.One * 4, 8, 8, DustID.Teleport, velocity.X * 0.2f, velocity.Y * 0.2f, 150);
                    dust.velocity *= 0.5f;
                }
            }
        }
    }

    // Loot & Reward System
    public class EmpressLootSystem : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type != NPCID.HallowBoss)
                return;

            Player player = Main.player[npc.target];
            if (player == null) return;

            // Difficulty-based loot drops
            int dropQuantity = 1 + (int)EmpressConfig.CurrentDifficulty;
            
            for (int i = 0; i < dropQuantity; i++)
            {
                int itemToDrop = Main.rand.Next(new[] {
                    ItemID.CrystalStorm,
                    ItemID.SoulofLight,
                    ItemID.SoulofLight,
                    ItemID.HallowedGreaves
                });

                Item.NewItem(npc.GetSource_FromAI(), npc.position, npc.width, npc.height, itemToDrop, Main.rand.Next(2, 5));
            }

            // Extreme mode bonus
            if (EmpressConfig.CurrentDifficulty == EmpressConfig.DifficultyMode.Extreme)
            {
                Item.NewItem(npc.GetSource_FromAI(), npc.position, npc.width, npc.height, ItemID.LunarOre, Main.rand.Next(5, 15));
            }
        }
    }
}
