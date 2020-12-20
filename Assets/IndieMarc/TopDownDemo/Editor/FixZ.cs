using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

/// <summary>
/// Run this script from Tools/Fix Z Offsets to fix z-fighting on overlapping sprites
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.TopDown
{
    public class FixZ : ScriptableWizard
    {

        [MenuItem("Tools/Fix Z Offsets")]
        static void SelectAllOfTagWizard()
        {
            ScriptableWizard.DisplayWizard<FixZ>("Fix floor and walls Z offset for lighting", "Fix Z Offsets");
        }

        void OffsetObjects(GameObject[] objs)
        {
            float z_offset = 0f;
            foreach (GameObject gameObject in objs)
            {
                float baseOffset = gameObject.GetComponent<Sprite3D>() ? gameObject.GetComponent<Sprite3D>().z_offset : 0f;
                Vector3 pos = gameObject.transform.position;
                gameObject.transform.position = new Vector3(pos.x, pos.y, z_offset + baseOffset);
                z_offset += 0.0001f;
                z_offset = z_offset % 0.01f;
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        GameObject[] ToGameObjects(MonoBehaviour[] behaviors)
        {
            List<GameObject> gobjs = new List<GameObject>();
            foreach (MonoBehaviour mono in behaviors)
            {
                gobjs.Add(mono.gameObject);
            }
            return gobjs.ToArray();
        }

        void OnWizardCreate()
        {
            OffsetObjects(ToGameObjects(FindObjectsOfType<FixOffset>()));
            OffsetObjects(ToGameObjects(FindObjectsOfType<Sprite3D>()));
        }
    }

}