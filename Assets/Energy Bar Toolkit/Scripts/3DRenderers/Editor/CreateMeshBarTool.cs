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

    public class CreateMeshBarTool : EditorWindow {

        #region Fields

        public MadPanel panel;
        public EnergyBar3DBase.BarType barType;



        #endregion

        #region Unity Methods

        void OnGUI() {
            MadGUI.Info("There's more than one Panel on the scene. Please set the one that you want to create the bar on.");

            panel = EditorGUILayout.ObjectField("Panel", panel, typeof(MadPanel), true) as MadPanel;
            barType = (EnergyBar3DBase.BarType) EditorGUILayout.EnumPopup("Bar Type", barType);

            GUI.enabled = panel != null;
            if (MadGUI.Button("Create", Color.green)) {
                EnergyBarUtils.Create3DBar(barType, panel);
                Close();
            }
            GUI.enabled = true;
        }

        #endregion

        #region Static Methods

        public static void ShowWindow(EnergyBar3DBase.BarType barType) {
            var tool = EditorWindow.GetWindow<CreateMeshBarTool>(false, "Create Bar", true);
            tool.barType = barType;
        }

        #endregion
    }

}