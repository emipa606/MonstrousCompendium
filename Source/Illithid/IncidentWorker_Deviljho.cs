// RimWorld.IncidentWorker_Deviljho

using RimWorld;
using Verse;

public class IncidentWorker_Deviljho : IncidentWorker
{
    private const float PointsFactor = 1.4f;

    private const int AnimalsStayDurationMin = 60000;

    private const int AnimalsStayDurationMax = 135000;

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map) parms.target;
        if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, map.Tile, out var pawnKindDef))
        {
            return false;
        }

        if (!RCellFinder.TryFindRandomPawnEntryCell(out var intVec, map, CellFinder.EdgeRoadChance_Animal))
        {
            return false;
        }

        var list = ManhunterPackIncidentUtility.GenerateAnimals(pawnKindDef, map.Tile, parms.points * 0.2f);
        var rot = Rot4.FromAngleFlat((map.Center - intVec).AngleFlat);
        for (var i = 0; i < list.Count; i++)
        {
            var deviljho = PawnKindDefOf.Deviljho;
            var pawn = PawnGenerator.GeneratePawn(deviljho);
            var loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
            GenSpawn.Spawn(pawn, loc, map, rot);
            pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 135000);
        }

        Find.LetterStack.ReceiveLetter("LetterLabelDeviljho".Translate(),
            "Deviljholetter".Translate(pawnKindDef.GetLabelPlural()), LetterDefOf.ThreatBig, list[0]);
        Find.TickManager.slower.SignalForceNormalSpeedShort();
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
        return true;
    }
}