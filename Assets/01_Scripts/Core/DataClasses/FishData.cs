using UnityEditor;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Data class for information regarding the fish.
    /// </summary>
    [CreateAssetMenu(fileName = "FishName", menuName = "Data/Fish/FishData")]
    public class FishData : ScriptableObject
    {
        [field: Tooltip("The sprite representing the fish")]
        [SerializeField] private Sprite sprite;
        [field: Tooltip("The rarity of the fish")]
        [SerializeField] private FishRarity rarity;
        [field: Tooltip("The type of water the fish lives in")]
        [SerializeField] private FishType type;
        [field: Tooltip("The minimum and maximum weight of the fish in kilograms")]
        [SerializeField, MinMaxVector2] private Vector2 weightRangeKg;
        [field: Tooltip("The minimum and maximum size of the fish in centimeters")]
        [SerializeField, MinMaxVector2] private Vector2 lengthRangeCm;

        public Sprite Sprite => sprite;
        public FishRarity Rarity => rarity;
        public FishType Type => type;
        public Vector2 WeightRangeKg => weightRangeKg;
        public Vector2 LengthRangeCm => lengthRangeCm;

        private void OnValidate()
        {
            ControlInspectorVectorInput(ref weightRangeKg);
            ControlInspectorVectorInput(ref lengthRangeCm);
        }

        private void ControlInspectorVectorInput(ref Vector2 variable)
        {
            //var temp = variable;
            if (variable.x <= 0) variable.x = 0.001f;
            if (variable.x > variable.y) (variable.y, variable.x) = (variable.x, variable.y);
        }

    }
}

public class MinMaxVector2Attribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MinMaxVector2Attribute))]
public class MinMaxVector2Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw the main label ("weightRange")
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        SerializedProperty xProp = property.FindPropertyRelative("x");
        SerializedProperty yProp = property.FindPropertyRelative("y");

        float labelWidth = 30f;
        float fieldWidth = (position.width - labelWidth * 2) / 2f;

        // Min label + field
        Rect minLabelRect = new(position.x, position.y, labelWidth, position.height);
        Rect minFieldRect = new(position.x + labelWidth, position.y, fieldWidth, position.height);

        EditorGUI.LabelField(minLabelRect, "Min");
        xProp.floatValue = Mathf.Max(0, EditorGUI.FloatField(minFieldRect, xProp.floatValue));

        // Max label + field
        Rect maxLabelRect = new(minFieldRect.x + fieldWidth, position.y, labelWidth, position.height);
        Rect maxFieldRect = new(maxLabelRect.x + labelWidth, position.y, fieldWidth, position.height);

        EditorGUI.LabelField(maxLabelRect, "Max");
        yProp.floatValue = Mathf.Max(xProp.floatValue, EditorGUI.FloatField(maxFieldRect, yProp.floatValue));
    }
}
#endif

public enum FishRarity
{
    Common
    //,Uncommon
    ,Rare
    ,Epic
    //,Legendary
}

public enum FishType
{
    Freshwater,
    Saltwater
}