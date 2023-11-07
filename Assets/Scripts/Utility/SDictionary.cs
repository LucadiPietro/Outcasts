using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SDictionary<TKey, TValue> :
    Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [System.Serializable]
    public class Pair
    {
        public TKey key;
        public TValue value;
        public string errorCode = "";

        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
            errorCode = "";
        }
    }

    [SerializeField] List<Pair> _list = new List<Pair>();

    [SerializeField] bool _error;

    public SDictionary()
        : base() { }
    public SDictionary(IDictionary<TKey, TValue> dictionary)
        : base(dictionary) { }
    public SDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        : base(dictionary, comparer) { }
    public SDictionary(IEqualityComparer<TKey> comparer)
        : base(comparer) { }
    public SDictionary(int capacity)
        : base(capacity) { }
    public SDictionary(int capacity, IEqualityComparer<TKey> comparer)
        : base(capacity, comparer) { }
    protected SDictionary(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    // save the dictionary to list
    public void OnBeforeSerialize()
    {
        if (_error) return;

        _list.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            _list.Add(new Pair(pair.Key, pair.Value));
        }
    }

    // load dictionary from list
    public void OnAfterDeserialize()
    {
        _error = false;
        Clear();
        try
        {
            for (int i = 0; i < _list.Count; i++)
            {
                _list[i].errorCode = "";

                bool nullKey = (_list[i].key == null);
                bool repeatingKey = (!nullKey && ContainsKey(_list[i].key));

                if (nullKey) _list[i].errorCode = "Null Key";
                if (repeatingKey) _list[i].errorCode = "Repeating Key";

                if (nullKey || repeatingKey)
                {
                    _error = true;
                    continue;
                }

                Add(_list[i].key, _list[i].value);
            }
        }
        catch (System.Exception e)
        {
            _error = true;
            Debug.LogError(e);
        }
    }
}

[System.Serializable]
public class Container<T>
{
    public T t;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Container<>))]
public class ContainerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }
}

[CustomPropertyDrawer(typeof(SDictionary<,>))]
public class SDictionaryDrawer : PropertyDrawer
{
    Color normalColor = Color.white;
    Color errorColor = Color.Lerp(Color.white, Color.red, 0.2f);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        bool error = property.FindPropertyRelative("_error").boolValue;

        Color color = GUI.color;
        GUI.color = error ? errorColor: normalColor;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_list"), label, true);
        GUI.color = color;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_list"));
    }
}

[CustomPropertyDrawer(typeof(SDictionary<,>.Pair))]
public class PairDrawer : PropertyDrawer
{
    float size = 200f;

    Color normalColor = Color.white;
    Color errorColor = Color.Lerp(Color.white, Color.red, 0.6f);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        string errorCode = property.FindPropertyRelative("errorCode").stringValue;
        bool error = !errorCode.Equals("");

        Color color = GUI.color;

        GUI.color = error ? errorColor : normalColor;

        EditorGUIUtility.labelWidth = 50f;

        SerializedProperty key = property.FindPropertyRelative("key");
        SerializedProperty value = property.FindPropertyRelative("value");

        float keyHeight = EditorGUI.GetPropertyHeight(key);
        float valueHeight = EditorGUI.GetPropertyHeight(value);

        Rect keyRect = new Rect(position.x, position.y, size, keyHeight);
        EditorGUI.PropertyField(keyRect, key, true);
        if (error)
        {
            keyRect.y += EditorGUIUtility.singleLineHeight;
            keyRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.HelpBox(keyRect, $"Error: {errorCode}", MessageType.Error);
        }
        Rect valueRect = new Rect(
            position.x + size + 20f,
            position.y,
            position.width - size - 20f,
            valueHeight);
        EditorGUI.PropertyField(valueRect, value, true);
        
        GUI.color = color;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string errorCode = property.FindPropertyRelative("errorCode").stringValue;
        bool error = !errorCode.Equals("");

        float keyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("key"));
        float valueHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"));

        if (error) keyHeight += EditorGUIUtility.singleLineHeight;

        float height = Mathf.Max(keyHeight, valueHeight);

        return height;
    }
}
#endif