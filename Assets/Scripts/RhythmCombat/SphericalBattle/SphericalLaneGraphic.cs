using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SphericalLaneGraphic : MaskableGraphic
{
    [SerializeField] Color attackColor = new Color(0.95f, 0.25f, 0.2f, 0.44f);
    [SerializeField] Color defenceColor = new Color(0.2f, 0.55f, 1f, 0.44f);
    [SerializeField] float laneThickness = 8f;
    [SerializeField] float targetRadius = 58f;
    [SerializeField] float ringThickness = 9f;
    [SerializeField] int curveSegments = 28;
    [SerializeField] int ringSegments = 40;

    const float SpawnX = 520f;
    const float AttackTargetX = 220f;
    const float DefenceTargetX = -220f;
    const float RowSpacing = 220f;
    const float Bend = 95f;

    public void Configure(Color newAttackColor, Color newDefenceColor)
    {
        attackColor = newAttackColor;
        defenceColor = newDefenceColor;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        for (int row = 0; row < 3; row++)
        {
            float y = (1 - row) * RowSpacing;
            float bend = row == 0 ? Bend : row == 2 ? -Bend : 0f;

            Vector2 attackStart = new Vector2(-SpawnX, y);
            Vector2 attackEnd = new Vector2(AttackTargetX, y);
            Vector2 attackControl = new Vector2((attackStart.x + attackEnd.x) * 0.5f - 35f, y + bend);
            AddBezier(vh, attackStart, attackControl, attackEnd, laneThickness, attackColor, curveSegments);
            AddRing(vh, attackEnd, targetRadius, ringThickness, attackColor, ringSegments);

            Vector2 defenceStart = new Vector2(SpawnX, y);
            Vector2 defenceEnd = new Vector2(DefenceTargetX, y);
            Vector2 defenceControl = new Vector2((defenceStart.x + defenceEnd.x) * 0.5f + 35f, y + bend);
            AddBezier(vh, defenceStart, defenceControl, defenceEnd, laneThickness, defenceColor, curveSegments);
            AddRing(vh, defenceEnd, targetRadius, ringThickness, defenceColor, ringSegments);
        }
    }

    static void AddBezier(
        VertexHelper vh,
        Vector2 start,
        Vector2 control,
        Vector2 end,
        float thickness,
        Color color,
        int segments)
    {
        Vector2 previous = start;
        for (int i = 1; i <= Mathf.Max(2, segments); i++)
        {
            float t = i / (float)Mathf.Max(2, segments);
            Vector2 current = Evaluate(start, control, end, t);
            AddSegment(vh, previous, current, thickness, color);
            previous = current;
        }
    }

    static Vector2 Evaluate(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        float u = 1f - t;
        return u * u * start + 2f * u * t * control + t * t * end;
    }

    static void AddSegment(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color color)
    {
        Vector2 direction = end - start;
        if (direction.sqrMagnitude < 0.0001f) return;

        direction.Normalize();
        Vector2 normal = new Vector2(-direction.y, direction.x) * thickness * 0.5f;
        int index = vh.currentVertCount;
        Color32 c = color;

        vh.AddVert(start - normal, c, Vector2.zero);
        vh.AddVert(start + normal, c, Vector2.up);
        vh.AddVert(end + normal, c, Vector2.one);
        vh.AddVert(end - normal, c, Vector2.right);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }

    static void AddRing(VertexHelper vh, Vector2 center, float radius, float thickness, Color color, int segments)
    {
        int count = Mathf.Max(8, segments);
        float innerRadius = Mathf.Max(0f, radius - thickness);
        Color32 c = color;

        for (int i = 0; i < count; i++)
        {
            float a0 = Mathf.PI * 2f * i / count;
            float a1 = Mathf.PI * 2f * (i + 1) / count;
            Vector2 outer0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 outer1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            Vector2 inner0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * innerRadius;
            Vector2 inner1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * innerRadius;
            int index = vh.currentVertCount;

            vh.AddVert(inner0, c, Vector2.zero);
            vh.AddVert(outer0, c, Vector2.up);
            vh.AddVert(outer1, c, Vector2.one);
            vh.AddVert(inner1, c, Vector2.right);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
