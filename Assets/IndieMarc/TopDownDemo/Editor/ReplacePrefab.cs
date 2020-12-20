using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Reflection;

/// <summary>
/// Cool script to replace multiple objects by another prefab: Tools/Replace Prefab
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.TopDown
{
    public class ReplacePrefab : ScriptableWizard
    {
        public GameObject NewPrefab;

        [MenuItem("Tools/Replace Prefab")]
        static void SelectAllOfTagWizard()
        {
            ScriptableWizard.DisplayWizard<ReplacePrefab>("Replace Prefabs", "Replace Prefabs");
        }

        void OnWizardCreate()
        {
            if (NewPrefab != null)
            {
                List<GameObject> newObjs = new List<GameObject>();

                foreach (Transform transform in Selection.transforms)
                {
                    GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(NewPrefab);
                    Undo.RegisterCreatedObjectUndo(newObject, "created prefab");

                    newObject.transform.position = transform.position;
                    newObject.transform.rotation = transform.rotation;
                    newObject.transform.localScale = transform.localScale;
                    newObject.transform.parent = transform.parent;
                    newObjs.Add(newObject);

                    Undo.DestroyObjectImmediate(transform.gameObject);
                }

                Selection.objects = newObjs.ToArray();
            }
        }
    }

}