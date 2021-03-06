﻿using System;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent
{
    public class EntityBehaviorHunger : EntityBehavior
    {
        ITreeAttribute hungerTree;
        EntityAgent entityAgent;

        float hungerCounter;
        //float lastFatReserves;
        int sprintCounter;

        long listenerId;
        long lastMoveMs;


        /*internal float FatReserves
        {
            get { return hungerTree.GetFloat("currentfatreserves"); }
            set { hungerTree.SetFloat("currentfatreserves", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }*/
        /* internal float MaxFatReserves
         {
             get { return hungerTree.GetFloat("maxfatreserves"); }
             set { hungerTree.SetFloat("maxfatreserves", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
         }*/


        public float SaturationLossDelayFruit
        {
            get { return hungerTree.GetFloat("saturationlossdelayfruit"); }
            set { hungerTree.SetFloat("saturationlossdelayfruit", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float SaturationLossDelayVegetable
        {
            get { return hungerTree.GetFloat("saturationlossdelayvegetable"); }
            set { hungerTree.SetFloat("saturationlossdelayvegetable", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float SaturationLossDelayProtein
        {
            get { return hungerTree.GetFloat("saturationlossdelayprotein"); }
            set { hungerTree.SetFloat("saturationlossdelayprotein", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float SaturationLossDelayGrain
        {
            get { return hungerTree.GetFloat("saturationlossdelaygrain"); }
            set { hungerTree.SetFloat("saturationlossdelaygrain", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float SaturationLossDelayDairy
        {
            get { return hungerTree.GetFloat("saturationlossdelaydairy"); }
            set { hungerTree.SetFloat("saturationlossdelaydairy", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float Saturation
        {
            get { return hungerTree.GetFloat("currentsaturation"); }
            set { hungerTree.SetFloat("currentsaturation", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float MaxSaturation
        {
            get { return hungerTree.GetFloat("maxsaturation"); }
            set { hungerTree.SetFloat("maxsaturation", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }
        
        public float FruitLevel
        {
            get { return hungerTree.GetFloat("fruitLevel"); }
            set { hungerTree.SetFloat("fruitLevel", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float VegetableLevel
        {
            get { return hungerTree.GetFloat("vegetableLevel"); }
            set { hungerTree.SetFloat("vegetableLevel", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float ProteinLevel
        {
            get { return hungerTree.GetFloat("proteinLevel"); }
            set { hungerTree.SetFloat("proteinLevel", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float GrainLevel
        {
            get { return hungerTree.GetFloat("grainLevel"); }
            set { hungerTree.SetFloat("grainLevel", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }

        public float DairyLevel
        {
            get { return hungerTree.GetFloat("dairyLevel"); }
            set { hungerTree.SetFloat("dairyLevel", value); entity.WatchedAttributes.MarkPathDirty("hunger"); }
        }



        public EntityBehaviorHunger(Entity entity) : base(entity)
        {
            entityAgent = entity as EntityAgent;
        }

        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            hungerTree = entity.WatchedAttributes.GetTreeAttribute("hunger");

            if (hungerTree == null)
            {
                entity.WatchedAttributes.SetAttribute("hunger", hungerTree = new TreeAttribute());

                Saturation = typeAttributes["currentsaturation"].AsFloat(1200);
                MaxSaturation = typeAttributes["maxsaturation"].AsFloat(1200);

                SaturationLossDelayFruit = typeAttributes["saturationlossdelay"].AsFloat(60 * 24);
                SaturationLossDelayVegetable = typeAttributes["saturationlossdelay"].AsFloat(60 * 24);
                SaturationLossDelayGrain = typeAttributes["saturationlossdelay"].AsFloat(60 * 24);
                SaturationLossDelayProtein = typeAttributes["saturationlossdelay"].AsFloat(60 * 24);
                SaturationLossDelayDairy = typeAttributes["saturationlossdelay"].AsFloat(60 * 24);

                FruitLevel = typeAttributes["currentfruitLevel"].AsFloat(0);
                VegetableLevel = typeAttributes["currentvegetableLevel"].AsFloat(0);
                GrainLevel = typeAttributes["currentgrainLevel"].AsFloat(0);
                ProteinLevel = typeAttributes["currentproteinLevel"].AsFloat(0);
                DairyLevel = typeAttributes["currentdairyLevel"].AsFloat(0);

                //FatReserves = configHungerTree["currentfatreserves"].AsFloat(1000);
                //MaxFatReserves = configHungerTree["maxfatreserves"].AsFloat(1000);
            }

            //lastFatReserves = FatReserves;

            listenerId = entity.World.RegisterGameTickListener(SlowTick, 6000);

            UpdateNutrientHealthBoost();
        }



        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            base.OnEntityDespawn(despawn);

            entity.World.UnregisterGameTickListener(listenerId);
        }

        public override void OnEntityReceiveSaturation(float saturation, EnumFoodCategory foodCat = EnumFoodCategory.Unknown, float saturationLossDelay = 10)
        {
            float maxsat = MaxSaturation;
            bool full = Saturation >= maxsat;

            Saturation = Math.Min(maxsat, Saturation + saturation);
            
            switch (foodCat)
            {
                case EnumFoodCategory.Fruit:
                    if (!full) FruitLevel = Math.Min(maxsat, FruitLevel + saturation / 2.5f);
                    SaturationLossDelayFruit = Math.Max(SaturationLossDelayFruit, saturationLossDelay);
                    break;

                case EnumFoodCategory.Vegetable:
                    if (!full) VegetableLevel = Math.Min(maxsat, VegetableLevel + saturation / 2.5f);
                    SaturationLossDelayVegetable = Math.Max(SaturationLossDelayVegetable, saturationLossDelay);
                    break;

                case EnumFoodCategory.Protein:
                    if (!full) ProteinLevel = Math.Min(maxsat, ProteinLevel + saturation / 2.5f);
                    SaturationLossDelayProtein = Math.Max(SaturationLossDelayProtein, saturationLossDelay);
                    break;

                case EnumFoodCategory.Grain:
                    if (!full) GrainLevel = Math.Min(maxsat, GrainLevel + saturation / 2.5f);
                    SaturationLossDelayGrain = Math.Max(SaturationLossDelayProtein, SaturationLossDelayGrain);
                    break;

                case EnumFoodCategory.Dairy:
                    if (!full) DairyLevel = Math.Min(maxsat, DairyLevel + saturation / 2.5f);
                    SaturationLossDelayDairy = Math.Max(SaturationLossDelayDairy, SaturationLossDelayGrain);
                    break;
            }

            UpdateNutrientHealthBoost();
            

        }

        public override void OnGameTick(float deltaTime)
        {
            if (entity is EntityPlayer)
            {
                EntityPlayer plr = (EntityPlayer)entity;
                EnumGameMode mode = entity.World.PlayerByUid(plr.PlayerUID).WorldData.CurrentGameMode;
                if (mode == EnumGameMode.Creative || mode == EnumGameMode.Spectator) return;

                if (plr.Controls.TriesToMove || plr.Controls.Jump)
                {
                    lastMoveMs = entity.World.ElapsedMilliseconds;
                }
            }


            sprintCounter += entityAgent != null && entityAgent.Controls.Sprint ? 1 : 0;

            //deltaTime *= 10;

            hungerCounter += deltaTime;


            // Once every 10s
            if (hungerCounter > 10)
            {
                bool isStandingStill = (entity.World.ElapsedMilliseconds - lastMoveMs) > 3000;
                float standStillFac = isStandingStill ? 1 / 3f : 1f;

                bool isondelay = false;

                if (SaturationLossDelayFruit > 0)
                {
                    SaturationLossDelayFruit -= 10 * standStillFac;
                    isondelay = true;
                } else
                {
                    FruitLevel = Math.Max(0, FruitLevel - Math.Max(1, 0.001f * FruitLevel * standStillFac));
                }

                if (SaturationLossDelayVegetable > 0)
                {
                    SaturationLossDelayVegetable -= 10 * standStillFac;
                    isondelay = true;
                } else
                {
                    VegetableLevel = Math.Max(0, VegetableLevel - Math.Max(1, 0.001f * VegetableLevel * standStillFac));
                }

                if (SaturationLossDelayProtein > 0)
                {
                    SaturationLossDelayProtein -= 10 * standStillFac;
                    isondelay = true;
                } else
                {
                    ProteinLevel = Math.Max(0, ProteinLevel - Math.Max(1, 0.001f * ProteinLevel) * standStillFac);
                }

                if (SaturationLossDelayGrain > 0)
                {
                    SaturationLossDelayGrain -= 10 * standStillFac;
                    isondelay = true;
                }
                {
                    GrainLevel = Math.Max(0, GrainLevel - Math.Max(1, 0.001f * GrainLevel) * standStillFac);
                }

                if (SaturationLossDelayDairy > 0)
                {
                    SaturationLossDelayDairy -= 10 * standStillFac;
                    isondelay = true;
                } else
                {
                    DairyLevel = Math.Max(0, DairyLevel - Math.Max(1, 0.001f * DairyLevel) * standStillFac);
                }

                UpdateNutrientHealthBoost();

                if (isondelay)
                {
                    hungerCounter -= 10;
                    return;
                }

                float prevSaturation = Saturation;
                float satLoss = (8 + sprintCounter / 15f) * standStillFac;

                if (prevSaturation > 0)
                {
                    Saturation = Math.Max(0, prevSaturation - satLoss);
                    sprintCounter = 0;
                }

                hungerCounter -= 10;    
            }
        }


        public void UpdateNutrientHealthBoost()
        {
            float fruitRel = FruitLevel / MaxSaturation;
            float grainRel = GrainLevel / MaxSaturation;
            float vegetableRel = VegetableLevel / MaxSaturation;
            float proteinRel = ProteinLevel / MaxSaturation;

            EntityBehaviorHealth bh = entity.GetBehavior<EntityBehaviorHealth>();
            //float baseMax = bh.MaxHealth;

            /*float MaxHealthNow =
                fruitRel * 0.25f * baseMax +
                grainRel * 0.25f * baseMax +
                vegetableRel * 0.25f * baseMax +
                proteinRel * 0.25f * baseMax
            ;*/

            // 0 nutr: 15 hp
            // 4 nutr: 25 hp

            // y = k*x + d
            // k = (y2-y1) / (x2-x1)

            // k = (25 - 15) / (4 - 0)
            // k = 10/4 = 2.5

            // 25 = 2.5 * 4 + d
            // d = 10

            float healthGain = 2.5f * (fruitRel + grainRel + vegetableRel + proteinRel);

            bh.MaxHealthModifiers["nutrientHealthMod"] = healthGain;
            bh.MarkDirty();
        }



        private void SlowTick(float dt)
        {
            if (entity is EntityPlayer)
            {
                EntityPlayer plr = (EntityPlayer)entity;
                if (entity.World.PlayerByUid(plr.PlayerUID).WorldData.CurrentGameMode == EnumGameMode.Creative) return;
            }

            //dt *= 20;

            if (Saturation <= 0)
            {
                // Let's say a fat reserve of 1000 is depleted in 3 ingame days using the default game speed of 1/60th
                // => 72 ingame hours / 60 = 1.2 irl hours = 4320 irl seconds
                // => 1 irl seconds substracts 1/4.32 fat reserves

                //float sprintLoss = sprintCounter / (15f * 6);
                //FatReserves = Math.Max(0, FatReserves - dt / 4.32f - sprintLoss / 4.32f);

                //if (FatReserves <= 0)
                {
                    entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Internal, Type = EnumDamageType.Hunger }, 0.25f);
                }



                sprintCounter = 0;
            }

            /*if (Saturation >= 0.85 * MaxSaturation)
            {
                // Fat recovery is 6 times slower
                FatReserves = Math.Min(MaxFatReserves, FatReserves + dt / (6 * 4.32f));
            }

            float max = MaxFatReserves;
            float cur = FatReserves / max;

            if (cur <= 0.8 || lastFatReserves <= 0.8)
            {
                float diff = cur - lastFatReserves;
                if (Math.Abs(diff) >= 0.1)
                {
                    HealthLocked += diff > 0 ? -1 : 1;

                    if (diff > 0 || Health > 0)
                    {
                        entity.ReceiveDamage(new DamageSource() { source = EnumDamageSource.Internal, type = (diff > 0) ? EnumDamageType.Heal : EnumDamageType.Hunger }, 1);
                    }

                    lastFatReserves = cur;
                }
            } else
            {
                lastFatReserves = cur;
            } */
        }

        public override string PropertyName()
        {
            return "hunger";
        }

        public override void OnEntityReceiveDamage(DamageSource damageSource, float damage)
        {
            if (damageSource.Type == EnumDamageType.Heal && damageSource.Source == EnumDamageSource.Respawn)
            {
                SaturationLossDelayFruit = 60;
                SaturationLossDelayVegetable = 60;
                SaturationLossDelayProtein = 60;
                SaturationLossDelayGrain = 60;
                SaturationLossDelayDairy = 60;

                Saturation = MaxSaturation / 2;
            }
        }
    }
 
}
