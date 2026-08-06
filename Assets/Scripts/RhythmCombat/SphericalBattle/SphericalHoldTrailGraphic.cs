using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SphericalHoldTrailGraphic : MaskableGraphic
{
    Vector2 start;
    Vector2 control;
    Vector2 end;
    double startHitTime;
    double endHitTime;
    float approachDuration = 2f;
    float thickness = 28f;
    Color trailColor = new Color(0.9f, 0.75f, 0.2f, 0.72f);
    Func<double> timeProvider;

    public void Configure(
        Vector2 startPoint,
        Vector2 controlPoint,
        Vector2 endPoint,
        double holdStartHitTime,
        double holdEndHitTime,
        float approachDurationSeconds,
        Func<double> chartTimeProvider,
        Color colorValue,
        float thicknessValue)
    {
        start = startPoint;
        control = controlPoint;
        end = endPoint;
        startHitTime = holdStartHitTime;
        endHitTime = holdEndHitTime;
        approachDuration = Mathf.Max(0.05f, approachDurationSeconds);
        timeProvider = chartTimeProvider;
        trailColor = colorValue;
        thickness = Mathf.Max(4f, thicknessValue);
        SetVerticesDirty();
    }

    void Update()
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (timeProvider == null || endHitTime <= startHitTime) return;

        double now = timeProvider();
        float headProgress = Mathf.Clamp01((float)(1d - (startHitTime - now) / approachDuration));
        float tailProgress = Mathf.Clamp01((float)(1d - (endHitTime - now) / approachDuration));

        if (headProgress <= tailProgress + 0.001f) return;

        const int segments = 24;
        Vector2 previous = Evaluate(tailProgress);
        for (int i = 1; i <= segments; i++)
        {
            float t = Mathf.Lerp(tailProgress, headProgress, i / (float)segments);
            Vector2 current = Evaluate(t);
            AddSegment(vh, previous, current, thickness, trailColor);
            previous = current;
        }
    }

    Vector2 Evaluate(float t)
    {
        float u = 1f - t;
        return u * u * start + 2f * u * t * control + t * t * end;
    }

    static void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
    {
        Vector2 direction = b - a;
        if (direction.sqrMagnitude < 0.0001f) return;

        direction.Normalize();
        Vector2 normal = new Vector2(-direction.y, direction.x) * width * 0.5f;
        int index = vh.currentVertCount;
        Color32 c = color;

        vh.AddVert(a - normal, c, Vector2.zero);
        vh.AddVert(a + normal, c, Vector2.up);
        vh.AddVert(b + normal, c, Vector2.one);
        vh.AddVert(b - normal, c, Vector2.right);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}
