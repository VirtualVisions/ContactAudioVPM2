﻿
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Vowgan.Contact.Footsteps
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContactFootsteps))]
    public class ContactFootstepsEditor : Editor
    {
        private ContactFootsteps script;
        public VisualTreeAsset InspectorTree;
        
        private SerializedProperty propPresets;
        private SerializedProperty propGroundLayers;


        private void OnEnable()
        {
            script =  target as ContactFootsteps;
            propGroundLayers = serializedObject.FindProperty(nameof(ContactFootsteps.GroundLayers));
            propPresets = serializedObject.FindProperty(nameof(ContactFootsteps.Presets));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            InspectorTree.CloneTree(root);

            Slider volumeMultiplierField = root.Query<Slider>("VolumeMultiplierField");
            volumeMultiplierField.RegisterValueChangedCallback(evt =>
            {
                if (!Application.isPlaying) return;
                UdonSharpEditorUtility.CopyUdonToProxy(script);
                script.VolumeMultiplier = evt.newValue;
                UdonSharpEditorUtility.CopyProxyToUdon(script);
            });

            IMGUIContainer presetsContainer = root.Query<IMGUIContainer>("PresetsContainer");
            presetsContainer.onGUIHandler += PresetsContainerGUI;

            Button findAllPresetsButton = root.Query<Button>("FindAllPresetsButton");
            findAllPresetsButton.clicked += FindAllPresetsButtonClicked;
            
            return root;
        }

        private void FindAllPresetsButtonClicked()
        {
            string[] presetGUIDs = AssetDatabase.FindAssets("t:ContactFootstepPreset");
            
            List<string> sceneDependencies = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                string sceneGUID = AssetDatabase.AssetPathToGUID(SceneManager.GetSceneAt(i).path);
                string[] dependencies = AssetDatabase.GetDependencies(sceneGUID);
                sceneDependencies.AddRange(dependencies);
            }
            
            List<string> scenePresets = new List<string>();

            foreach (string guid in presetGUIDs)
            {
                if (sceneDependencies.Contains(guid))
                {
                    scenePresets.Add(guid);
                }
            }

            propPresets.arraySize = presetGUIDs.Length;
            for (int i = 0; i < presetGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(presetGUIDs[i]);
                ContactFootstepPreset preset = AssetDatabase.LoadAssetAtPath<ContactFootstepPreset>(path);
                propPresets.GetArrayElementAtIndex(i).objectReferenceValue = preset;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void PresetsContainerGUI()
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(propPresets);
                if (changed.changed) serializedObject.ApplyModifiedProperties();
            }
        }
    }
}