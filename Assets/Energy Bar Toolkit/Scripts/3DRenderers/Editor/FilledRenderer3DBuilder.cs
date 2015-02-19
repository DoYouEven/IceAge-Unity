/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public class FilledRenderer3DBuilder {

    // ===========================================================
    // Static Methods
    // ===========================================================

    public static FilledRenderer3D Create() {
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
                CreateMeshBarTool.ShowWindow(EnergyBar3DBase.BarType.Filled);
            }

            return null;
        } else {
            return Create(panel);
        }
    }

    public static FilledRenderer3D Create(MadPanel panel) {
        var bar = MadTransform.CreateChild<FilledRenderer3D>(panel.transform, "filled progress bar");
        TryApplyExampleTextures(bar);
        Selection.activeObject = bar.gameObject;
        
        return bar;
    }
    
    static void TryApplyExampleTextures(FilledRenderer3D bar) {
        var textureBar = AssetDatabase.LoadAssetAtPath(
            "Assets/Energy Bar Toolkit/Progress Bar Pack 1/Textures/bar1_bar.png", typeof(Texture2D)) as Texture2D;
        var textureFg = AssetDatabase.LoadAssetAtPath(
            "Assets/Energy Bar Toolkit/Progress Bar Pack 1/Textures/bar1_fg.png", typeof(Texture2D)) as Texture2D;
        
        if (textureBar != null && textureFg != null) {
            bar.textureBar = textureBar;
            var tex = new EnergyBarBase.Tex();
            tex.texture = textureFg;
            tex.color = Color.white;
            
            bar.texturesBackground = new EnergyBarBase.Tex[1];
            bar.texturesBackground[0] = tex;
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