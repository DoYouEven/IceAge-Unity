/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace EnergyBarToolkit {

[CustomEditor(typeof(TransformRenderer3D))]
public class TransformRenderer3DInspector : EnergyBar3DInspectorBase {

    #region Private Fields

    private SerializedProperty objectTexture;
    private SerializedProperty objectAtlasTextureGUID;

    private SerializedProperty objectTint;

    private SerializedProperty objectAnchor;

    private SerializedProperty transformTranslate;
    private SerializedProperty transformRotate;
    private SerializedProperty transformScale;

    SerializedProperty translateFunctionStart;
    SerializedProperty translateFunctionEnd;
    SerializedProperty rotateFunctionStart;
    SerializedProperty rotateFunctionEnd;
    SerializedProperty scaleFunctionStart;
    SerializedProperty scaleFunctionEnd;

    private TransformRenderer3D script;

    #endregion

    #region Slots

    public override void OnEnable() {
        base.OnEnable();

        script = target as TransformRenderer3D;

        objectTexture = serializedObject.FindProperty("objectTexture");
        objectAtlasTextureGUID = serializedObject.FindProperty("objectAtlasTextureGUID");

        objectTint = serializedObject.FindProperty("objectTint");

        objectAnchor = serializedObject.FindProperty("objectAnchor");

        transformTranslate = serializedObject.FindProperty("transformTranslate");
        transformRotate = serializedObject.FindProperty("transformRotate");
        transformScale = serializedObject.FindProperty("transformScale");

        translateFunctionStart = serializedObject.FindProperty("translateFunction")
            .FindPropertyRelative("startPosition");
        translateFunctionEnd = serializedObject.FindProperty("translateFunction")
            .FindPropertyRelative("endPosition");
        rotateFunctionStart = serializedObject.FindProperty("rotateFunction")
            .FindPropertyRelative("startAngle");
        rotateFunctionEnd = serializedObject.FindProperty("rotateFunction")
            .FindPropertyRelative("endAngle");
        scaleFunctionStart = serializedObject.FindProperty("scaleFunction")
            .FindPropertyRelative("startScale");
        scaleFunctionEnd = serializedObject.FindProperty("scaleFunction")
            .FindPropertyRelative("endScale");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        Header();

        SectionTextures();
        SectionPositionAndSize();
        SectionAppearance();
        SectionTransform();
        SectionEffects();

        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Private Methods

    private void SectionTextures() {
        Section("Textures", () => {
            FieldTextureMode();

            if (script.textureMode != EnergyBar3DBase.TextureMode.TextureAtlas || script.atlas != null) {
                FieldBackgroundTextures();

                EditorGUILayout.BeginHorizontal();
                FieldSprite(objectTexture, objectAtlasTextureGUID, "Object Texture");
                EditorGUILayout.PropertyField(objectTint, new GUIContent(""), new GUILayoutOption[] { GUILayout.Width(50) });
                EditorGUILayout.EndHorizontal();

                
                MadGUI.PropertyFieldVector2(objectAnchor, "Object Anchor");
                FieldForegroundTextures();
            }

            FieldPremultipliedAlpha();
        });
    }

    private void SectionAppearance() {
        Section("Appearance", () => {
            FieldLabel();
        });
    }

    private void SectionEffects() {
        Section("Effects", () => {
            FieldSmoothEffect();
        });
    }

    private void SectionTransform() {
        Section("Object Transform", () => {
            MadGUI.PropertyFieldToggleGroup2(transformTranslate, "Translate", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyFieldVector2(translateFunctionStart, "Start Point");
                    MadGUI.PropertyFieldVector2(translateFunctionEnd, "End Point");
                });
            });

            MadGUI.PropertyFieldToggleGroup2(transformRotate, "Rotate", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(rotateFunctionStart, "Start Angle");
                    MadGUI.PropertyField(rotateFunctionEnd, "End Angle");
                });
            });

            MadGUI.PropertyFieldToggleGroup2(transformScale, "Scale", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyFieldVector2(scaleFunctionStart, "Start Scale");
                    MadGUI.PropertyFieldVector2(scaleFunctionEnd, "End Scale");
                });
            });
        });
    }

    #endregion
    
}

} // namespace