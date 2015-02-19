/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public class EnergyBarInspectorBase : EditorBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    public const string FormatHelp =
        @"{cur} - current int value
{min} - minimal value
{max} - maximum value
{cur%} - current % value (0 - 100)
{cur2%} - current % value with precision of tens (0.0 - 100.0)

Examples:
{cur}/{max} - 27/100
{cur%} % - 27 %";

    // ===========================================================
    // Fields
    // ===========================================================
    
    protected SerializedProperty textureMode;
    
    protected SerializedProperty atlas;
    protected SerializedProperty atlasTexturesBackground;
    protected SerializedProperty atlasTexturesForeground;
    
    protected SerializedProperty texturesBackground;
    protected SerializedProperty texturesForeground;
    protected SerializedProperty premultipliedAlpha;
    
    protected SerializedProperty pivot;
    protected SerializedProperty guiDepth;
    protected SerializedProperty anchorObject;
    protected SerializedProperty anchorCamera;
    protected SerializedProperty anchorOffset;
    protected SerializedProperty anchor3d;
    protected SerializedProperty positionSizeFromTransform;
    protected SerializedProperty positionSizeFromTransformNormalized;
    protected SerializedProperty resizeMode;
    
    protected SerializedProperty labelEnabled;
    protected SerializedProperty labelSkin;
    protected SerializedProperty labelPosition;
    protected SerializedProperty labelPositionNormalized;
    protected SerializedProperty labelPivot;
    protected SerializedProperty labelFormat;
    protected SerializedProperty labelColor;
    protected SerializedProperty labelOutlineEnabled;
    protected SerializedProperty labelOutlineColor;
    
    // available only for 3d renderers
    private SerializedProperty labelFont;
    private SerializedProperty labelScale;
    
    protected SerializedProperty effectSmoothChange;
    protected SerializedProperty effectSmoothChangeSpeed;
    
    private EnergyBarBase energyBarBase;
    private EnergyBar3DBase energyBar3DBase;
    
    public bool showPositionAndSize = true;
    
    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public virtual void OnEnable() {
        energyBarBase = target as EnergyBarBase;
        energyBar3DBase = target as EnergyBar3DBase;
        
        textureMode = serializedObject.FindProperty("textureMode");
        
        atlas = serializedObject.FindProperty("atlas");
        atlasTexturesBackground = serializedObject.FindProperty("atlasTexturesBackground");
        atlasTexturesForeground = serializedObject.FindProperty("atlasTexturesForeground");
    
        texturesBackground = serializedObject.FindProperty("texturesBackground");
        texturesForeground = serializedObject.FindProperty("texturesForeground");
        premultipliedAlpha = serializedObject.FindProperty("premultipliedAlpha");
    
        pivot = serializedObject.FindProperty("pivot");
        guiDepth = serializedObject.FindProperty("guiDepth");
        anchorObject = serializedObject.FindProperty("anchorObject");
        anchorCamera = serializedObject.FindProperty("anchorCamera");
        anchorOffset = serializedObject.FindProperty("anchorOffset");
        anchor3d = serializedObject.FindProperty("anchor3d");
        positionSizeFromTransform = serializedObject.FindProperty("positionSizeFromTransform");
        positionSizeFromTransformNormalized = serializedObject.FindProperty("positionSizeFromTransformNormalized");
        resizeMode = serializedObject.FindProperty("resizeMode");
        
        labelEnabled = serializedObject.FindProperty("labelEnabled");
        labelSkin = serializedObject.FindProperty("labelSkin");
        labelPosition = serializedObject.FindProperty("labelPosition");
        labelPositionNormalized = serializedObject.FindProperty("labelPositionNormalized");
        labelPivot = serializedObject.FindProperty("labelPivot");
        labelFormat = serializedObject.FindProperty("labelFormat");
        labelColor = serializedObject.FindProperty("labelColor");
        labelOutlineEnabled = serializedObject.FindProperty("labelOutlineEnabled");
        labelOutlineColor = serializedObject.FindProperty("labelOutlineColor");
        
        labelFont = serializedObject.FindProperty("labelFont");
        labelScale = serializedObject.FindProperty("labelScale");
        
        effectSmoothChange = serializedObject.FindProperty("effectSmoothChange");
        effectSmoothChangeSpeed = serializedObject.FindProperty("effectSmoothChangeSpeed");
    }

    // ===========================================================
    // Methods
    // ===========================================================

    public override void OnInspectorGUI() {
        MadGUI.LookLikeControls(170);
        OnCustomInspector();
    }

    public virtual void OnCustomInspector() {

    }
    
    protected void CheckTextureIsGUI(Texture2D texture) {
        if (texture == null) {
            return;
        }
        
        var path = AssetDatabase.GetAssetPath(texture);
        var importer = TextureImporter.GetAtPath(path) as TextureImporter;
        
        if (importer.textureType != TextureImporterType.GUI) {
            if (MadGUI.WarningFix("It's recommended that this texture type is set to GUI.")) {
                importer.textureType = TextureImporterType.GUI;
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    
    protected void CheckTextureIsReadable(Texture2D texture) {
        if (texture == null) {
            return;
        }
        
        var path = AssetDatabase.GetAssetPath(texture);
        var importer = TextureImporter.GetAtPath(path) as TextureImporter;
        
        if ((importer.textureType != TextureImporterType.Advanced && importer.textureType != TextureImporterType.GUI)
                || !importer.isReadable || importer.npotScale != TextureImporterNPOTScale.None) {
            if (MadGUI.ErrorFix("This texture must be set to Advanced/Readable without power of 2.")) {
                importer.textureType = TextureImporterType.Advanced;
                importer.isReadable = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    
    protected void CheckTextureFilterTypePoint(Texture2D texture) {
        if (texture == null) {
            return;
        }
        
        var path = AssetDatabase.GetAssetPath(texture);
        var importer = TextureImporter.GetAtPath(path) as TextureImporter;
        
        if (importer.filterMode != FilterMode.Point) {
            if (MadGUI.WarningFix("It's recommended that this texture filter mode is set to Point.")) {
                importer.filterMode = FilterMode.Point;
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    
    protected void CheckTextureFilterTypeNotPoint(Texture2D texture) {
        if (texture == null) {
            return;
        }
        
        var path = AssetDatabase.GetAssetPath(texture);
        var importer = TextureImporter.GetAtPath(path) as TextureImporter;
        
        if (importer.filterMode == FilterMode.Point) {
            if (MadGUI.WarningFix("It's recommended that this texture filter mode is set to Bilinear or Trilinear")) {
                importer.filterMode = FilterMode.Bilinear;
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    
    // Unity 4.2 cames with new texture importer option called "Alpha is Transparency".
    // It changes how textures are rendered and completely fixes the issue with black borders
    // on transparent images. These methods are meant to manage this option.
    protected void CheckAlphaIsTransparency(List<Texture2D> textures) {
#if !UNITY_4_1
        bool found = textures.Find((texture) => {
            if (texture == null) {
                return false;
            }
            
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = TextureImporter.GetAtPath(path) as TextureImporter;
            
            return !importer.alphaIsTransparency;
        }) != null;
        
        if (found) {
            if (WarningFix("It is highly recommended to enable all textures 'Alpha is Transparency' option.")) {
                foreach (var texture in textures) {
                    if (texture == null) {
                        continue;
                    }
                    
                    var path = AssetDatabase.GetAssetPath(texture);
                    var importer = TextureImporter.GetAtPath(path) as TextureImporter;
                    
                    if (!importer.alphaIsTransparency) {
                        importer.alphaIsTransparency = true;
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                }
            }
        }
#endif
    }
    
    protected void CheckNotAlphaIsTransparency(List<Texture2D> textures) {
#if !UNITY_4_1
        bool showWarning = false;

        foreach (var texture in textures) {
            if (texture == null) {
                continue;
            }
            
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = TextureImporter.GetAtPath(path) as TextureImporter;
            
            if (importer.alphaIsTransparency) {
                showWarning = true;
            }
        }
        
        if (showWarning) {
            if (WarningFix("When premultiplied alpha is enabled all textures 'Alpha is Transparency' option must be "
                + "disabled.")) {
                foreach (var texture in textures) {
                    if (texture == null) {
                        continue;
                    }
                    
                    var path = AssetDatabase.GetAssetPath(texture);
                    var importer = TextureImporter.GetAtPath(path) as TextureImporter;
                    
                    if (importer.alphaIsTransparency) {
                        importer.alphaIsTransparency = false;
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                }
            }
        }
#endif
    }
    
    protected virtual List<Texture2D> AllTextures() {
        return new List<Texture2D>();
    }
    
    protected List<Texture2D> BackgroundTextures() {
        return TexturesOf((target as EnergyBarBase).texturesBackground);
    }
    
    protected List<Texture2D> ForegroundTextures() {
        return TexturesOf((target as EnergyBarBase).texturesForeground);
    }
    
    List<Texture2D> TexturesOf(EnergyBarBase.Tex[] textures) {
        List<Texture2D> o = new List<Texture2D>();
        foreach (var tex in textures) {
            o.Add(tex.texture);
        }
        
        return o;
    }

    protected void Section(string name, SectionBody body) {
        GUILayout.Label(name, "HeaderLabel");
        EditorGUILayout.Space();

        MadGUI.Indent(() => {
            body();
        });

        EditorGUILayout.Space();
    }
    
#region Fields
    protected void FieldTextureMode() {
        MadGUI.PropertyFieldEnumPopup(textureMode, "Texture Mode");
        if (energyBar3DBase.textureMode == EnergyBar3DBase.TextureMode.TextureAtlas) {
            MadGUI.PropertyField(atlas, "Texture Atlas", MadGUI.ObjectIsSet);
        }
    }

    protected void FieldBackgroundTextures() {
        if (textureMode == null || textureMode.enumValueIndex == (int) EnergyBar3DBase.TextureMode.Textures) {
            GUITextures(ref texturesBackground, "Background Textures");
        } else {
            energyBar3DBase.atlasTexturesBackground = 
                AtlasTextures(energyBar3DBase.atlasTexturesBackground, "Background Textures");
        }
    }
    
    protected void FieldForegroundTextures() {
        if (textureMode == null || textureMode.enumValueIndex == (int) EnergyBar3DBase.TextureMode.Textures) {
            GUITextures(ref texturesForeground, "Foreground Textures");
        } else {
            energyBar3DBase.atlasTexturesForeground =
                AtlasTextures(energyBar3DBase.atlasTexturesForeground, "Foreground Textures");
        }
    }
    
    private void GUITextures(ref SerializedProperty textures, string label) {
        ArrayList(textures, label, (property) => {
            var texture = property.FindPropertyRelative("texture");
            var color = property.FindPropertyRelative("color");
            
            GUITexture(texture, color);
            
            CheckTextureIsGUI(texture.objectReferenceValue as Texture2D);
        });
    }
    
    protected void GUITexture(SerializedProperty texture, SerializedProperty color) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(texture, new GUIContent(""), GUILayout.MaxWidth(150), GUILayout.ExpandWidth(true));
        EditorGUILayout.PropertyField(color, new GUIContent(""), GUILayout.MaxWidth(100));
        EditorGUILayout.EndHorizontal();
    }
    
//    private void AtlasTextures(ref SerializedProperty textures, string label) {
//        if (MadGUI.Foldout(label, true)) {
//            var list = new MadGUI.ArrayList<EnergyBar3DBase.AtlasTex>(textures, (property) => {
//                var spriteName = property.FindPropertyRelative("spriteGUID");
//                var color = property.FindPropertyRelative("color");
//                
//                AtlasTexture(spriteName, color);
//            });
//            
//            list.Draw();
//        }
//    }
    
    private EnergyBar3DBase.AtlasTex[] AtlasTextures(EnergyBar3DBase.AtlasTex[] textures, string label) {
        if (MadGUI.Foldout(label, true)) {
            var l = new List<EnergyBar3DBase.AtlasTex>();
            if (textures != null) {
                l.AddRange(textures);
            }
            
            var list = new MadGUI.ArrayList<EnergyBar3DBase.AtlasTex>(l, (item) => {
                var spriteGUID = item.spriteGUID;
                var color = item.color;
                
                EditorGUI.BeginChangeCheck();
                AtlasTexture(spriteGUID, ref color, (guid) => {
                    item.spriteGUID = guid;
                    EditorUtility.SetDirty(target);
                });
                if (EditorGUI.EndChangeCheck()) {
                    item.spriteGUID = spriteGUID;
                    item.color = color;
                    EditorUtility.SetDirty(target);
                }

                return item;
            });
            list.onAdd += (tex) => tex.color = Color.white;
            
            list.Draw();
            return l.ToArray();
        } else {
            return textures;
        }
    }
    
//    protected void AtlasTexture(SerializedProperty spriteName, SerializedProperty color) {
//        EditorGUILayout.BeginHorizontal();
//
//        FieldAtlasSprite(spriteName, "Sprite");
//        
//        EditorGUILayout.PropertyField(color, new GUIContent(""), GUILayout.MaxWidth(100));
//        EditorGUILayout.EndHorizontal();
//    }
//    
    protected void FieldAtlasSprite(SerializedProperty guid, string label) {
        var atlas = energyBar3DBase.atlas;
        MadAtlasUtil.AtlasField(guid, atlas, label);
    }

    protected void AtlasTexture(string spriteGUID, ref Color color, MadAtlasBrowser.Changed guidChangedCallback) {
        EditorGUILayout.BeginHorizontal();
        
        FieldAtlasSprite(spriteGUID, "Sprite", guidChangedCallback);
        color = EditorGUILayout.ColorField("", color, GUILayout.MaxWidth(100));
        
        EditorGUILayout.EndHorizontal();
    }
    
    protected void FieldAtlasSprite(string guid, string label, MadAtlasBrowser.Changed guidChangedCallback) {
        var atlas = energyBar3DBase.atlas;
        MadAtlasUtil.AtlasField(guid, atlas, label, guidChangedCallback);
    }
    
    protected void FieldRelativeToTransform() {
        MadGUI.PropertyField(positionSizeFromTransform, "Relative To Transform",
            "Affects position and size by this game object transform.");
        if (positionSizeFromTransform.boolValue) {
            MadGUI.Indent(() => {
                MadGUI.PropertyField(positionSizeFromTransformNormalized, "Normalized");
            });
        }
    }
    
    protected void FieldPremultipliedAlpha() {
        if (premultipliedAlpha == null) {
            return;
        }
        
        if (!premultipliedAlpha.boolValue) {
            CheckAlphaIsTransparency(AllTextures());
        }
    
        MadGUI.PropertyField(premultipliedAlpha, "Premultiplied Alpha",
        "Check this if your textures has its components multiplied by alpha channel.");

        if (premultipliedAlpha.boolValue) {
            CheckNotAlphaIsTransparency(AllTextures());
        }
    }
    
    protected void FieldSmoothEffect() {
        MadGUI.PropertyFieldToggleGroup2(effectSmoothChange, "Smooth Effect", () => {
            MadGUI.Indent(() => {
                MadGUI.PropertyField(effectSmoothChangeSpeed, "Smooth Speed");
            });
        });
    }
    
    protected void FieldLabel() {
        bool classic = labelFont == null; // true if classic GUI version
    
        MadGUI.PropertyFieldToggleGroup2(labelEnabled, "Draw Label", () => {
            MadGUI.Indent(() => {
                if (classic) {
                    EditorGUILayout.PropertyField(labelSkin, new GUIContent("Label Skin"));
                } else {
                    MadGUI.PropertyField(labelFont, "Label Font", MadGUI.ObjectIsSet);
                    MadGUI.PropertyField(labelScale, "Label Scale");
                }
        
                labelPosition.vector2Value = EditorGUILayout.Vector2Field("Label Position", labelPosition.vector2Value);
                var t = target as EnergyBarBase;
                if (MadGameObject.IsActive(t.gameObject)) {
                    var rect = t.DrawAreaRect;
                    PropertySpecialNormalized(labelPosition, labelPositionNormalized, new Vector2(rect.width, rect.height));
                }
                
                if (labelPivot != null) {
                    MadGUI.PropertyField(labelPivot, "Pivot Point");
                }
                
                EditorGUILayout.PropertyField(labelFormat, new GUIContent("Label Format"));
                
                if (MadGUI.Foldout("Label Format Help", false)) {
                    EditorGUILayout.HelpBox(FormatHelp, MessageType.None);
                }
                
                EditorGUILayout.PropertyField(labelColor, new GUIContent("Label Color"));
                
                if (classic) {
                    MadGUI.PropertyFieldToggleGroup2(labelOutlineEnabled, "Label Outline", () => {
                        MadGUI.Indent(() => {
                            EditorGUILayout.PropertyField(labelOutlineColor, new GUIContent("Outline Color"));
                        });
                    });
                }
            });
        });
    }
 
#endregion   

#region mad2d
    protected bool IsAnchored() {
        return GetAnchor() != null;
    }
    
    protected MadAnchor GetAnchor() {
        return MadTransform.FindParent<MadAnchor>(energyBarBase.transform, 0);
    }

    protected void CreateAnchor() {
        MadUndo.LegacyRegisterSceneUndo("Create Anchor");
    
        if (IsAnchored()) {
            EditorUtility.DisplayDialog("Anchor already exists", "Your bar object is already under the anchor", "Fine");
            return;
        }
        
        var anchor = MadTransform.CreateChild<MadAnchor>(energyBarBase.transform.parent, "Anchor");
        energyBarBase.transform.parent = anchor.transform;
        
        Selection.activeObject = energyBarBase.gameObject;
    }
#endregion

    protected delegate void SectionBody();
}

} // namespace
