using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LanePressFeedbackGraphic : MaskableGraphic
{
    [SerializeField] float outerRadius = 62f;
    [SerializeField] float ringThickness = 9f;
    [SerializeField] int segments = 36;

    public void SetFeedbackColor(Color value)
    {
        color = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        int segmentCount = Mathf.Max(12, segments);
        float innerRadius =
            Mathf.Max(0f, outerRadius - ringThickness);

        Color32 vertexColor = color;

        for (int i = 0; i < segmentCount; i++)
        {
            float angle0 =
                Mathf.PI * 2f * i / segmentCount;

            float angle1 =
                Mathf.PI * 2f * (i + 1) /
                segmentCount;

            Vector2 outer0 =
                new Vector2(
                    Mathf.Cos(angle0),
                    Mathf.Sin(angle0)) *
                outerRadius;

            Vector2 outer1 =
                new Vector2(
                    Mathf.Cos(angle1),
                    Mathf.Sin(angle1)) *
                outerRadius;

            Vector2 inner0 =
                new Vector2(
                    Mathf.Cos(angle0),
                    Mathf.Sin(angle0)) *
                innerRadius;

            Vector2 inner1 =
                new Vector2(
                    Mathf.Cos(angle1),
                    Mathf.Sin(angle1)) *
                innerRadius;

            int index = vh.currentVertCount;

            vh.AddVert(inner0, vertexColor, Vector2.zero);
            vh.AddVert(outer0, vertexColor, Vector2.up);
            vh.AddVert(outer1, vertexColor, Vector2.one);
            vh.AddVert(inner1, vertexColor, Vector2.right);

            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
