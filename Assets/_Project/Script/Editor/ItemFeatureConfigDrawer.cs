// csharp
#if UNITY_EDITOR
using System;
using _Project.Script.Items.Feature;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemFeatureConfig))]
public class ItemFeatureConfigDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 접기/펼치기 UI를 위한 foldout
        var featureProp = property.FindPropertyRelative("feature");
        var paramProp   = property.FindPropertyRelative("param");

        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // feature
        height += EditorGUI.GetPropertyHeight(featureProp, true) + EditorGUIUtility.standardVerticalSpacing;

        // param
        var feature = featureProp.objectReferenceValue as ItemFeature;
        if (feature != null && ShouldDrawParam(feature))
        {
            // 자식까지 포함해서 그릴 때의 높이
            height += EditorGUI.GetPropertyHeight(paramProp, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 직렬화 상태 동기화
        var so = property.serializedObject;
        so.Update();

        EditorGUI.BeginProperty(position, label, property);

        var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // 라벨
        EditorGUI.LabelField(line, label, EditorStyles.boldLabel);
        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        var featureProp = property.FindPropertyRelative("feature");
        var paramProp   = property.FindPropertyRelative("param");

        // Feature 필드
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(line, featureProp, new GUIContent("Feature"), true);
        line.y += EditorGUI.GetPropertyHeight(featureProp, true) + EditorGUIUtility.standardVerticalSpacing;

        var feature = featureProp.objectReferenceValue as ItemFeature;

        // Feature가 변경되었거나 param이 비었거나 타입이 맞지 않으면 재생성
        if (EditorGUI.EndChangeCheck())
        {
            CreateOrFixParam(feature, paramProp);
        }
        else
        {
            // 매 프레임 타입 체크(스크립트 리컴파일/리네임 등)
            EnsureParamType(feature, paramProp);
        }

        // Param 필드
        if (feature != null && ShouldDrawParam(feature))
        {
            EditorGUI.PropertyField(line, paramProp, new GUIContent("Param"), includeChildren: true);
        }

        EditorGUI.EndProperty();

        // 변경사항 일괄 적용
        so.ApplyModifiedProperties();
    }

    private static bool ShouldDrawParam(ItemFeature feature)
    {
        // 기본 FeatureParam(추상 베이스)만 요구하면 숨김 처리
        return feature.ParamType != null && feature.ParamType != typeof(FeatureParam);
    }

    private static void CreateOrFixParam(ItemFeature feature, SerializedProperty paramProp)
    {
        if (feature == null)
        {
            SetManagedReference(paramProp, null);
            return;
        }

        // ParamType에 맞는 인스턴스 생성
        var newParam = SafeCreateParam(feature);
        SetManagedReference(paramProp, newParam);
    }

    private static void EnsureParamType(ItemFeature feature, SerializedProperty paramProp)
    {
        if (feature == null)
        {
            if (paramProp.managedReferenceValue != null)
            {
                SetManagedReference(paramProp, null);
            }
            return;
        }

        var current = paramProp.managedReferenceValue;
        var expectedType = feature.ParamType;

        if (!ShouldDrawParam(feature))
        {
            // 파라미터를 쓰지 않는 Feature면 null 유지
            if (current != null)
            {
                SetManagedReference(paramProp, null);
            }
            return;
        }

        if (current == null || current.GetType() != expectedType)
        {
            var newParam = SafeCreateParam(feature);
            SetManagedReference(paramProp, newParam);
        }
    }

    private static FeatureParam SafeCreateParam(ItemFeature feature)
    {
        try
        {
            // Feature가 기본값 생성기를 제공하면 가장 우선적으로 사용
            var def = feature.CreateDefaultParam();
            if (def != null) return def;

            // 그 외에는 ParamType의 기본 생성자 시도
            var t = feature.ParamType;
            if (t != null && !t.IsAbstract)
                return (FeatureParam)Activator.CreateInstance(t);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to create default param for feature {feature.name}: {e.Message}");
        }

        return null;
    }

    private static void SetManagedReference(SerializedProperty prop, object value)
    {
        // 중간에 Update/Apply를 호출하지 않는다. 외부 OnGUI에서 일괄 처리
        prop.managedReferenceValue = value;
    }
}
#endif