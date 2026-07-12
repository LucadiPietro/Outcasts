using RhythmCombat.Domain.Timing;
using TMPro;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    [Header("Existing Battle UI")]
    public TextMeshProUGUI counter;
    public TextMeshProUGUI feedback;

    [Header("Rhythm Score UI")]
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] TextMeshProUGUI maxComboText;
    [SerializeField] TextMeshProUGUI judgmentText;
    [SerializeField] TextMeshProUGUI perfectCountText;
    [SerializeField] TextMeshProUGUI goodCountText;
    [SerializeField] TextMeshProUGUI badCountText;
    [SerializeField] TextMeshProUGUI missCountText;
    [SerializeField] bool autoBindMissingTextReferences = true;

    void Awake()
    {
        if (autoBindMissingTextReferences)
        {
            BindMissingTextReferences();
        }
    }

    public void UpdateCounter(int count)
    {
        if (counter == null)
        {
            return;
        }

        counter.text = count.ToString();
    }

    public void UpdateRhythmScore(BattleRhythmScoreState score)
    {
        if (score == null)
        {
            return;
        }

        UpdateCombo(score.Combo, score.MaxCombo);
        UpdateJudgment(score.LastJudgment, score.HasLastJudgment);
        UpdateRhythmStats(score);
    }

    public void UpdateCombo(int combo, int maxCombo)
    {
        UpdateCounter(combo);
        SetText(comboText, combo.ToString());
        SetText(maxComboText, maxCombo.ToString());
    }

    public void UpdateJudgment(JudgmentGrade judgment, bool hasJudgment)
    {
        if (judgmentText != null)
        {
            judgmentText.text = hasJudgment ? FormatJudgment(judgment) : string.Empty;
            judgmentText.color = GetJudgmentColor(judgment);
        }
    }

    public void UpdateRhythmStats(BattleRhythmScoreState score)
    {
        if (score == null)
        {
            return;
        }

        SetText(perfectCountText, score.PerfectCount.ToString());
        SetText(goodCountText, score.GoodCount.ToString());
        SetText(badCountText, score.BadCount.ToString());
        SetText(missCountText, score.MissCount.ToString());
    }

    public void ShowRhythmResult(BattleRhythmScoreState score, string message, Color color)
    {
        UpdateRhythmScore(score);
        ShowFeedback(message, color);
    }

    public void ShowFeedback(string message, Color color)
    {
        if (feedback == null)
        {
            return;
        }

        feedback.text = message;
        feedback.color = color;
    }

    void BindMissingTextReferences()
    {
        if (counter == null)
        {
            counter = FindText("Counter", "ComboCounter", "RuntimeCounter");
        }

        if (feedback == null)
        {
            feedback = FindText("Feedback", "JudgmentFeedback", "RuntimeFeedback");
        }

        if (comboText == null)
        {
            comboText = FindText("Combo", "ComboText", "ComboValue");
        }

        if (maxComboText == null)
        {
            maxComboText = FindText("MaxCombo", "MaxComboText", "MaxComboValue");
        }

        if (judgmentText == null)
        {
            judgmentText = FindText("Judgment", "JudgmentText", "Vote", "VoteText", "Voto", "VotoText");
        }

        if (perfectCountText == null)
        {
            perfectCountText = FindText("PerfectCount", "PerfectText", "PerfectValue");
        }

        if (goodCountText == null)
        {
            goodCountText = FindText("GoodCount", "GoodText", "GoodValue");
        }

        if (badCountText == null)
        {
            badCountText = FindText("BadCount", "BadText", "BadValue");
        }

        if (missCountText == null)
        {
            missCountText = FindText("MissCount", "MissText", "MissValue");
        }
    }

    TextMeshProUGUI FindText(params string[] names)
    {
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                if (text.name == names[nameIndex])
                {
                    return text;
                }
            }
        }

        return null;
    }

    static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    static string FormatJudgment(JudgmentGrade judgment)
    {
        return judgment.ToString().ToUpperInvariant();
    }

    static Color GetJudgmentColor(JudgmentGrade judgment)
    {
        switch (judgment)
        {
            case JudgmentGrade.Perfect:
                return new Color(0.2f, 1f, 0.85f);
            case JudgmentGrade.Good:
                return Color.green;
            case JudgmentGrade.Bad:
                return new Color(1f, 0.7f, 0.15f);
            default:
                return Color.red;
        }
    }
}
