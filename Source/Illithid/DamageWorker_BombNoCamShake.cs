using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Firecracker;

public class DamageWorker_BombNoCamShake : DamageWorker_AddInjury
{
    private static readonly ThingDef moteExplosionFlash = ThingDef.Named("Mote_ExplosionFlash");


    public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
    {
        if (def.explosionHeatEnergyPerCell > float.Epsilon)
        {
            GenTemperature.PushHeat(explosion.Position, explosion.Map,
                def.explosionHeatEnergyPerCell * cellsToAffect.Count);
        }

        MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, moteExplosionFlash, explosion.radius * 6f);
    }
}