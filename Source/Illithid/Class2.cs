// Gizmo_PsionicShieldStatus
using RimWorld;
using System;
using UnityEngine;
using Verse;

[StaticConstructorOnStartup]
internal class Gizmo_PsionicShieldStatus : Gizmo
{
    public PsionicShieldBelt shield;

    private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

    private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);


    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
    {
        var overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
        {
            Rect rect = overRect.AtZero().ContractedBy(6f);
            Rect rect2 = rect;
            rect2.height = overRect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect2, shield.LabelCap);
            Rect rect3 = rect;
            rect3.yMin = overRect.height / 2f;
            var fillPercent = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true));
            Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect rect4 = rect3;
            var num = shield.Energy * 100f;
            var str = num.ToString("F0");
            num = shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true) * 100f;
            Widgets.Label(rect4, str + " / " + num.ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
        }, true, false, 1f);
        return new GizmoResult(GizmoState.Clear);
    }

    public override float GetWidth(float maxWidth)
    {
        return 140f;
    }
}
