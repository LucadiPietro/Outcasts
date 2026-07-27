using System.Collections.Generic;
using RhythmCombat.Domain.Timing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Super UI")]
    [SerializeField] Image[] superFillImages;
    [SerializeField] TextMeshProUGUI[] superValueTexts;
    [SerializeField] bool autoBindMissingSuperReferences = true;
    [SerializeField] bool logMissingSuperReferences = true;

    void Awake()
    {
        if (autoBindMissingTextReferences)
        {
            BindMissingTextReferences();
        }

        if (autoBindMissingSuperReferences)
        {
            BindMissingSuperReferences();
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

    public void UpdateSuperMeters(IReadOnlyList<BattleSuperMeterState> meters)
    {
        if (meters == null)
        {
            return;
        }

        for (int i = 0; i < meters.Count; i++)
        {
            BattleSuperMeterState meter = meters[i];
            if (meter == null)
            {
                continue;
            }

            if (superFillImages != null && i < superFillImages.Length && superFillImages[i] != null)
            {
                Image fillImage = ResolveSuperFillImage(superFillImages[i]);
                if (fillImage != null)
                {
                    EnsureFilledImage(fillImage);
                    fillImage.fillAmount = Mathf.Clamp01(meter.NormalizedValue);
                }
            }

            if (superValueTexts != null && i < superValueTexts.Length && superValueTexts[i] != null)
            {
                superValueTexts[i].text = Mathf.RoundToInt(meter.CurrentValue) + "/" + Mathf.RoundToInt(meter.MaxValue);
            }
        }
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

    void BindMissingSuperReferences()
    {
        if (superFillImages == null || superFillImages.Length == 0)
        {
            superFillImages = FindSuperFillImages();

            if (logMissingSuperReferences && superFillImages.Length == 0)
            {
                Debug.LogWarning("BattleUIManager: nessun fill Super trovato nel Canvas. Collega superFillImages dall'Inspector.");
            }
        }

        if (superValueTexts == null || superValueTexts.Length == 0)
        {
            superValueTexts = FindTexts("SuperValue", "SuperText", "SuperMeterText");
        }
    }

    TextMeshProUGUI FindText(params string[] names)
    {
        TextMeshProUGUI[] texts = FindTexts(names);
        return texts.Length > 0 ? texts[0] : null;
    }

    TextMeshProUGUI[] FindTexts(params string[] names)
    {
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
        List<TextMeshProUGUI> matches = new List<TextMeshProUGUI>();
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
                    matches.Add(text);
                    break;
                }
            }
        }

        return matches.ToArray();
    }

    Image[] FindSuperFillImages()
    {
        Image[] superImages = FindImages("Super", "SuperFill", "SuperMeter");
        List<Image> fills = new List<Image>();

        for (int i = 0; i < superImages.Length; i++)
        {
            Image fillImage = ResolveSuperFillImage(superImages[i]);
            if (fillImage != null && !fills.Contains(fillImage))
            {
                fills.Add(fillImage);
            }
        }

        fills.Sort(CompareImagesByScenePosition);
        return fills.ToArray();
    }

    Image[] FindImages(params string[] names)
    {
        Image[] images = FindObjectsOfType<Image>(true);
        List<Image> matches = new List<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null)
            {
                continue;
            }

            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                if (image.name == names[nameIndex])
                {
                    matches.Add(image);
                    break;
                }
            }
        }

        return matches.ToArray();
    }

    static Image ResolveSuperFillImage(Image image)
    {
        if (image == null)
        {
            return null;
        }

        if (image.type == Image.Type.Filled)
        {
            return image;
        }

        Image[] children = image.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Image child = children[i];
            if (child != null &&
                child != image &&
                child.type == Image.Type.Filled &&
                IsFillName(child.name))
            {
                return child;
            }
        }

        for (int i = 0; i < children.Length; i++)
        {
            Image child = children[i];
            if (child != null &&
                child != image &&
                child.type == Image.Type.Filled)
            {
                return child;
            }
        }

        return image;
    }

    static void EnsureFilledImage(Image image)
    {
        if (image == null || image.type == Image.Type.Filled)
        {
            return;
        }

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Vertical;
        image.fillOrigin = 0;
    }

    static bool IsFillName(string objectName)
    {
        return !string.IsNullOrEmpty(objectName) && objectName.StartsWith("Fill");
    }

    static int CompareImagesByScenePosition(Image left, Image right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        Vector3 leftPosition = left.transform.position;
        Vector3 rightPosition = right.transform.position;
        int xComparison = leftPosition.x.CompareTo(rightPosition.x);
        return xComparison != 0 ? xComparison : leftPosition.y.CompareTo(rightPosition.y);
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
