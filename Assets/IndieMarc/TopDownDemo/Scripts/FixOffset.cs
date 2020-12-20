using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.TopDown
{
    public class FixOffset : MonoBehaviour
    {
        public int order_min = 0;
        public int order_max = 0;

        [Header("Use 'Tools/Fix Z Offsets'")]
        [Tooltip("Use Tools/Fix Z Offsets  to offset all the assets with this script in the scene. This will avoid Z fighting for rendering and lighting.")]
        public float z_offset = 0f;

        void Awake()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().sortingOrder = Random.Range(order_min, order_max + 1);
            enabled = false; //Just for editor, don't run in-game
        }

    }

}