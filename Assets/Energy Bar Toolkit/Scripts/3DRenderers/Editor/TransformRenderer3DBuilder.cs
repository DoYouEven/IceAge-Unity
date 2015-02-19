/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public class TransformRenderer3DBuilder {

    // ===========================================================
    // Static Methods
    // ===========================================================

    public static TransformRenderer3D Create() {
        var panel = MadPanel.UniqueOrNull();

        if (panel == null) {
            var panels = MadPanel.All();
            if (panels.Length == 0) {
                if (EditorUtility.DisplayDialog(
                "Init Scene?",
                "Scene not initialized for 3D bars. You cannot place new bar without proper initialization. Do it now?",
                "Yes", "No")) {
                    MadInitTool.ShowWindow();
                }
            } else {
                CreateMeshBarTool.ShowWindow(EnergyBar3DBase.BarType.Transform);
            }

            return null;
        } else {
            return Create(panel);
        }
    }
    
    public static TransformRenderer3D Create(MadPanel panel) {
        var bar = MadTransform.CreateChild<TransformRenderer3D>(panel.transform, "transform progress bar");
        TryApplyExampleTextures(bar);
        Selection.activeObject = bar.gameObject;
        
        return bar;
    }
    
    static void TryApplyExampleTextures(TransformRenderer3D bar) {
        var objectTexture = AssetDatabase.LoadAssetAtPath(
            "Assets/Energy Bar Toolkit/Example/Textures/ClockBar/object.png", typeof(Texture2D)) as Texture2D;
        var bgTexture = AssetDatabase.LoadAssetAtPath(
            "Assets/Energy Bar Toolkit/Example/Textures/ClockBar/bg.png", typeof(Texture2D)) as Texture2D;

        if (objectTexture != null && bgTexture != null) {
            bar.objectTexture = objectTexture;
            bar.texturesBackground = new EnergyBarBase.Tex[] { new EnergyBarBase.Tex() { texture = bgTexture, color = Color.white } };

            bar.transformRotate = true;
            bar.rotateFunction.endAngle = 360;

        } else {
            Debug.LogWarning("Failed to locate example textures. This is not something bad, but have you changed "
                             + "your Energy Bar Toolkit directory location?");
        }
    }
    
    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace