namespace Outcasts.Minigames.GameGo
{
    using UnityEngine;

    public sealed class AlphaFader : MonoBehaviour
    {
        [SerializeField] SpriteRenderer m_Renderer;
        [SerializeField] AnimationCurve m_Curve;

        float m_Elapsed;
        void OnEnable()
        {
            m_Elapsed = 0;
        }
        void Update()
        {
            float normalizedTime = Mathf.Repeat(m_Elapsed, 1f);
            float alpha = m_Curve.Evaluate(normalizedTime);
            m_Renderer.color = new Color(m_Renderer.color.r, m_Renderer.color.g, m_Renderer.color.b, alpha);

            m_Elapsed += Time.deltaTime;
        }
    }
}
