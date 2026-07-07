using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace TheSanity.GlobalNPC.Bosses.EmpressRework
{
    public class EmpressRework : GlobalNPC
    {
        public override void ModifyNPC(NPC npc)
        {
            // Target Empress of Light (NPCID.HallowBoss)
            if (npc.type == NPCID.HallowBoss)
            {
                npc.lifeMax = 4200; // Increased HP (original: 3600)
                npc.damage = 55; // Increased damage (original: 50)
                npc.defense = 15; // Increased defense (original: 10)
            }
        }
    }

    public class EmpressAIRework : GlobalNPC
    {
        private float phase1AttackCounter = 0;
        private float phase2AttackCounter = 0;
        private float phase3AttackCounter = 0;
        private float telegraphDelay = 0f;
        private int currentAttackType = 0;
        private bool isPhase1Active = false;
        private bool isPhase2Active = false;
        private bool isPhase3Active = false;

        public override void AI(NPC npc)
        {
            if (npc.type != NPCID.HallowBoss)
                return;

            // Determine current phase based on health
            float healthPercent = npc.life / (float)npc.lifeMax;

            if (healthPercent > 0.66f)
            {
                isPhase1Active = true;
                isPhase2Active = false;
                isPhase3Active = false;
                ExecutePhase1AI(npc, healthPercent);
            }
            else if (healthPercent > 0.33f)
            {
                isPhase1Active = false;
                isPhase2Active = true;
                isPhase3Active = false;
                ExecutePhase2AI(npc, healthPercent);
            }
            else
            {
                isPhase1Active = false;
                isPhase2Active = false;
                isPhase3Active = true;
                ExecutePhase3AI(npc, healthPercent);
            }
        }

        private void ExecutePhase1AI(NPC npc, float healthPercent)
        {
            phase1AttackCounter++;
            
            // Attack every 90 frames (1.5 seconds at 60 FPS)
            if (phase1AttackCounter >= 90)
            {
                phase1AttackCounter = 0;
                currentAttackType = Main.rand.Next(0, 7); // 7 attack patterns
                telegraphDelay = 30f; // 0.5 second telegraph
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
                }
            }

            telegraphDelay--;
        }

        private void ExecutePhase2AI(NPC npc, float healthPercent)
        {
            phase2AttackCounter++;
            
            // Attack faster - every 75 frames
            if (phase2AttackCounter >= 75)
            {
                phase2AttackCounter = 0;
                currentAttackType = Main.rand.Next(0, 7);
                telegraphDelay = 25f; // 0.42 second telegraph
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
                }
            }

            telegraphDelay--;
        }

        private void ExecutePhase3AI(NPC npc, float healthPercent)
        {
            phase3AttackCounter++;
            
            // Attack very fast - every 60 frames
            if (phase3AttackCounter >= 60)
            {
                phase3AttackCounter = 0;
                currentAttackType = Main.rand.Next(0, 7);
                telegraphDelay = 20f; // 0.33 second telegraph
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
                }
            }

            telegraphDelay--;
        }

        private void HandleTelegraph(NPC npc, float delay)
        {
            if (delay > 0)
            {
                // Visual telegraph effect - glow
                int glowIntensity = (int)(255 * (1f - (delay / 30f)));
                if (Main.netMode != NetmodeID.Server)
                {
                    // Create telegraph particles/glow effect
                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Teleport, 0f, 0f, glowIntensity);
                        dust.velocity *= 0.5f;
                    }
                }
            }
        }

        // ===== PHASE 1 ATTACKS =====
        private void Phase1_HomingBurstAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                direction = direction.RotatedBy(MathHelper.TwoPi / 8 * i);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 20, 0f);
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
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, offset * 10f, ProjectileID.FairyQueenMagicProjectile, 22, 0f);
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
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 7f, ProjectileID.FairyQueenMagicProjectile, 18, 0f);
            }
        }

        private void Phase1_WavePattern(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 5; i++)
            {
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + direction * 20 * i, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 20, 0f);
            }
        }

        private void Phase1_SpinAttack(NPC npc)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 direction = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 16 * i);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 19, 0f);
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
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 21, 0f);
            }
        }

        // ===== PHASE 2 ATTACKS =====
        private void Phase2_ComboAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            // Multiple rapid bursts
            for (int burst = 0; burst < 2; burst++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                    direction = direction.RotatedBy(MathHelper.TwoPi / 10 * i);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 24, 0f);
                }
            }
        }

        private void Phase2_ScreenFillingProjectiles(NPC npc)
        {
            // Projectile spam covering screen
            for (int i = 0; i < 20; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 20 * i);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 10f, ProjectileID.FairyQueenMagicProjectile, 25, 0f);
            }
        }

        private void Phase2_DashAttack(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            npc.velocity = Vector2.Normalize(target.Center - npc.Center) * 15f;

            // Projectiles while dashing
            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 8 * i);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 8f, ProjectileID.FairyQueenMagicProjectile, 23, 0f);
            }
        }

        private void Phase2_CrossLaser(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            
            // Cross pattern
            for (int i = 0; i < 4; i++)
            {
                Vector2 cross = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i);
                for (int j = 0; j < 6; j++)
                {
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, cross * 10f, ProjectileID.FairyQueenMagicProjectile, 24, 0f);
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
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, spiral * (7f + i * 0.5f), ProjectileID.FairyQueenMagicProjectile, 24, 0f);
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
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction.RotatedBy(MathHelper.Pi / 12 * (i - 4 + w * 2)) * 10f, ProjectileID.FairyQueenMagicProjectile, 25, 0f);
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
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 10f, ProjectileID.FairyQueenMagicProjectile, 24, 0f);
                }
            }
        }

        // ===== PHASE 3 ATTACKS (EXTREME) =====
        private void Phase3_ExtremeCombo(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            // Multiple rapid combo bursts
            for (int combo = 0; combo < 3; combo++)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
                    direction = direction.RotatedBy(MathHelper.TwoPi / 12 * i + combo * 0.3f);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 28, 0f);
                }
            }
        }

        private void Phase3_ProjectileHell(NPC npc)
        {
            // Extreme projectile spam
            for (int i = 0; i < 30; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 30 * i);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 29, 0f);
            }
        }

        private void Phase3_MultiDash(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            // Rapid dash attacks in multiple directions
            for (int d = 0; d < 3; d++)
            {
                Vector2 dashDir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(MathHelper.TwoPi / 3 * d);
                npc.velocity = dashDir * 16f;

                for (int i = 0; i < 10; i++)
                {
                    Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 10 * i);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 9f, ProjectileID.FairyQueenMagicProjectile, 27, 0f);
                }
            }
        }

        private void Phase3_LaserGrid(NPC npc)
        {
            // Grid pattern of lasers
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Vector2 offset = new Vector2((x - 2) * 60, (y - 2) * 60);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, Vector2.Zero, ProjectileID.FairyQueenMagicProjectile, 30, 0f);
                }
            }
        }

        private void Phase3_ChaosPattern(NPC npc)
        {
            // Complete chaos - random spread
            for (int i = 0; i < 35; i++)
            {
                Vector2 direction = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 11f, ProjectileID.FairyQueenMagicProjectile, 28, 0f);
                }
            }
        }

        private void Phase3_UltimateBurst(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            // Ultimate burst - everything at once
            for (int i = 0; i < 40; i++)
            {
                Vector2 direction = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 40 * i);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 12f, ProjectileID.FairyQueenMagicProjectile, 30, 0f);
            }
        }

        private void Phase3_InfernoPhase(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (target == null) return;

            // Infernum-style inferno attack
            Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
            
            // Main beam
            for (int i = 0; i < 8; i++)
            {
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, direction * 12f, ProjectileID.FairyQueenMagicProjectile, 30, 0f);
            }
            
            // Side spreads
            for (int s = 0; s < 2; s++)
            {
                Vector2 side = direction.RotatedBy(MathHelper.Pi / 4 * (s == 0 ? 1 : -1));
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, side * 11f, ProjectileID.FairyQueenMagicProjectile, 29, 0f);
                }
            }
        }
    }
}
