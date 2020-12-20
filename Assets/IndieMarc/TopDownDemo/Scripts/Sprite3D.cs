using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allow using Sorting Layer and Sorting Order on 3D MeshRenderers
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.TopDown
{

    public class Sprite3D : MonoBehaviour
    {

        public string sort_layer = "Default";
        public int sort_order = 0;
        public float z_offset = 0f;
        public bool fix_offset = false;

        void Awake()
        {
            GetComponent<MeshRenderer>().sortingLayerID = SortingLayer.NameToID(sort_layer);
            GetComponent<MeshRenderer>().sortingOrder = sort_order;
            enabled = false;
        }
    }

}