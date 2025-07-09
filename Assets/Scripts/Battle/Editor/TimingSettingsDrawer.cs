namespace Outcasts.Battle.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(TimingSettings))]
    public class TimingSettingsDrawer : PropertyDrawer
    {
        private const float SLIDER_HEIGHT = 20f;
        private const float HANDLE_RADIUS = 8f;
        private const float LABEL_HEIGHT = 16f;

        Color[] m_Colors = new Color[]
        {
            (Color.red + Color.yellow) * 0.5f,
            Color.yellow,
            Color.green,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float min = 0f;
            const float max = 0.12f;

            SerializedProperty[] properties = new SerializedProperty[]
            {
                property.FindPropertyRelative("m_" + nameof(TimingSettings.Perfect)),
                property.FindPropertyRelative("m_" + nameof(TimingSettings.Great)),
                property.FindPropertyRelative("m_" + nameof(TimingSettings.Good)),
            };
            var values = properties.Select(x => x.floatValue).ToArray();

            EditorGUI.BeginProperty(position, label, property);

            // Label at top
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            Rect sliderRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + LABEL_HEIGHT, position.width, SLIDER_HEIGHT);
            EditorGUI.DrawRect(sliderRect, Color.gray * 1.2f);

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Event evt = Event.current;
            float halfHeight = SLIDER_HEIGHT * 0.5f;

            float rectStart = sliderRect.x;
            for (int i = 0; i < 3; i++)
            {

                float value = Mathf.Clamp01(values[i]);
                float t = Mathf.InverseLerp(min, max, value);
                float handleX = Mathf.Lerp(sliderRect.x, sliderRect.xMax, t);
                Vector2 handleCenter = new Vector2(handleX, sliderRect.center.y);
                Rect handleRect = new Rect(handleCenter.x - HANDLE_RADIUS, handleCenter.y - HANDLE_RADIUS, HANDLE_RADIUS * 2, HANDLE_RADIUS * 2);

                // Draw area
                EditorGUI.DrawRect(new Rect(rectStart, handleCenter.y - halfHeight, handleCenter.x - rectStart, SLIDER_HEIGHT), m_Colors[i]);
                rectStart = handleCenter.x + halfHeight;

                // Draw knob
                EditorGUI.DrawRect(new Rect(handleCenter.x - halfHeight, handleCenter.y - halfHeight, SLIDER_HEIGHT, SLIDER_HEIGHT), Color.white);
                EditorGUI.DrawRect(handleRect, m_Colors[i]);
                GUI.Label(handleRect, GUIContent.none);
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.SlideArrow);

                // Value label above knob
                string valueLabel = value.ToString("0.000");
                Vector2 labelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(valueLabel));
                Rect valueLabelRect = new Rect(handleCenter.x - labelSize.x / 2f, sliderRect.y - LABEL_HEIGHT, labelSize.x, LABEL_HEIGHT);
                EditorGUI.LabelField(valueLabelRect, valueLabel, EditorStyles.miniLabel);

                // Feedback label below knob
                valueLabel = properties[i].displayName;
                labelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(valueLabel));
                valueLabelRect = new Rect(handleCenter.x - labelSize.x / 2f, sliderRect.y + LABEL_HEIGHT + labelSize.y * 0.5f, labelSize.x, LABEL_HEIGHT);
                EditorGUI.LabelField(valueLabelRect, properties[i].displayName, EditorStyles.miniLabel);

                // Interaction
                if (evt.type == EventType.MouseDown && handleRect.Contains(evt.mousePosition))
                {
                    GUIUtility.hotControl = controlID + i;
                    evt.Use();
                }

                if (GUIUtility.hotControl == controlID + i && evt.type == EventType.MouseDrag)
                {
                    float mouseT = Mathf.InverseLerp(sliderRect.x, sliderRect.xMax, evt.mousePosition.x);
                    float newValue = Mathf.Lerp(min, max, mouseT);

                    // Clamp between neighbors
                    float lower = (i == 0) ? min : values[i - 1];
                    float upper = (i == values.Length - 1) ? max : values[i + 1];

                    values[i] = Mathf.Clamp(newValue, lower, upper);
                    evt.Use();
                    GUI.changed = true;
                }

                if (evt.type == EventType.MouseUp && GUIUtility.hotControl == controlID + i)
                {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                }
            }

            string failLabel = "Fail";
            Vector2 failLabelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(failLabel));
            Rect failLabelRect = new Rect(sliderRect.max.x - failLabelSize.x, sliderRect.y + LABEL_HEIGHT + failLabelSize.y * 0.5f, failLabelSize.x, LABEL_HEIGHT);
            EditorGUI.DrawRect(new Rect(rectStart, sliderRect.y, sliderRect.max.x - rectStart, SLIDER_HEIGHT), Color.red);
            EditorGUI.LabelField(failLabelRect, failLabel, EditorStyles.miniLabel);

            // Apply updated values
            for (int i = 0; i < values.Length; i++) properties[i].floatValue = values[i];

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + SLIDER_HEIGHT + LABEL_HEIGHT * 2 + 8;
        }
    } 
}
