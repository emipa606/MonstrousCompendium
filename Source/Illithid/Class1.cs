// PsionicShieldBelt
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

[StaticConstructorOnStartup]
public class PsionicShieldBelt : Apparel
{
    private float energy;

    private int ticksToReset = -1;

    private int lastKeepDisplayTick = -9999;

    private Vector3 impactAngleVect;

    private int lastAbsorbDamageTick = -9999;

    private const float MinDrawSize = 1.2f;

    private const float MaxDrawSize = 1.55f;

    private const float MaxDamagedJitterDist = 0.05f;

    private const int JitterDurationTicks = 8;

    private readonly int StartingTicksToReset = 3200;

    private readonly float EnergyOnReset = 0.2f;

    private readonly float EnergyLossPerDamage = 0.033f;

    private readonly int KeepDisplayingTicks = 1000;

    private readonly float ApparelScorePerEnergyMax = 0.25f;

    private static readonly Material BubbleMat = MaterialPool.MatFrom("ShieldBubble", ShaderDatabase.Transparent);

    private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);

    private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;

    public float Energy => energy;

    public ShieldState ShieldState
    {
        get
        {
            if (ticksToReset > 0)
            {
                return ShieldState.Resetting;
            }
            return ShieldState.Active;
        }
    }

    private bool ShouldDisplay
    {
        get
        {
            Pawn wearer = base.Wearer;
            if (wearer.Spawned && !wearer.Dead && !wearer.Downed)
            {
                if (wearer.InAggroMentalState)
                {
                    return true;
                }
                if (wearer.Drafted)
                {
                    return true;
                }
                if (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner)
                {
                    return true;
                }
                if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref energy, "energy", 0f, false);
        Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1, false);
        Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
    }

    public override IEnumerable<Gizmo> GetWornGizmos()
    {
        if (Find.Selector.SingleSelectedThing == base.Wearer)
        {
            yield return (Gizmo)new Gizmo_PsionicShieldStatus
            {
                shield = this
            };
        }
    }

    public override float GetSpecialApparelScoreOffset()
    {
        return EnergyMax * ApparelScorePerEnergyMax;
    }

    public override void Tick()
    {
        base.Tick();
        if (base.Wearer == null)
        {
            energy = 0f;
        }
        else if (ShieldState == ShieldState.Resetting)
        {
            ticksToReset--;
            if (ticksToReset <= 0)
            {
                Reset();
            }
        }
        else if (ShieldState == ShieldState.Active)
        {
            energy += EnergyGainPerTick;
            if (energy > EnergyMax)
            {
                energy = EnergyMax;
            }
        }
    }

    public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
    {
        if (ShieldState == ShieldState.Active && ((dinfo.Instigator != null && !dinfo.Instigator.Position.AdjacentTo8WayOrInside(base.Wearer.Position)) || dinfo.Def.isExplosive))
        {
            if (dinfo.Instigator != null)
            {
                if (dinfo.Instigator is AttachableThing attachableThing && attachableThing.parent == base.Wearer)
                {
                    return false;
                }
            }
            energy -= (float)dinfo.Amount * EnergyLossPerDamage;
            if (dinfo.Def == DamageDefOf.EMP)
            {
                energy = -1f;
            }
            if (energy < 0f)
            {
                Break();
            }
            else
            {
                AbsorbedDamage(dinfo);
            }
            return true;
        }
        return false;
    }

    public void KeepDisplaying()
    {
        lastKeepDisplayTick = Find.TickManager.TicksGame;
    }

    private void AbsorbedDamage(DamageInfo dinfo)
    {
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
        impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
        Vector3 loc = base.Wearer.TrueCenter() + (impactAngleVect.RotatedBy(180f) * 0.5f);
        var num = Mathf.Min(10f, 2f + ((float)dinfo.Amount / 10f));
        MoteMaker.MakeStaticMote(loc, base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
        var num2 = (int)num;
        for (var i = 0; i < num2; i++)
        {
            MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
        }
        lastAbsorbDamageTick = Find.TickManager.TicksGame;
        KeepDisplaying();
    }

    private void Break()
    {
        SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
        MoteMaker.MakeStaticMote(base.Wearer.TrueCenter(), base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
        for (var i = 0; i < 6; i++)
        {
            Vector3 loc = base.Wearer.TrueCenter() + (Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f));
            MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
        }
        energy = 0f;
        ticksToReset = StartingTicksToReset;
    }

    private void Reset()
    {
        if (base.Wearer.Spawned)
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            MoteMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
        }
        ticksToReset = -1;
        energy = EnergyOnReset;
    }

    public override void DrawWornExtras()
    {
        if (ShieldState == ShieldState.Active && ShouldDisplay)
        {
            var num = Mathf.Lerp(MinDrawSize, MaxDrawSize, energy);
            Vector3 vector = base.Wearer.Drawer.DrawPos;
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
            if (num2 < JitterDurationTicks)
            {
                var num3 = (float)(JitterDurationTicks - num2) / 8f * MaxDamagedJitterDist;
                vector += impactAngleVect * num3;
                num -= num3;
            }
            var angle = (float)Rand.Range(0, 360);
            var s = new Vector3(num, 1f, num);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
        }
    }

    public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb)
    {
        return true;
    }
}
