using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script automatically updates the sorting order and X rotation, so that it faces the camera and sort based on the Y value
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>
/// 
namespace IndieMarc.TopDown
{
    public class AutoOrderLayer : MonoBehaviour
    {
        [Header("Auto Order")]
        public bool auto_sort = true;
        public float offset;
        public float sort_refresh_rate = 0.1f;

        [Header("Auto Rotate")]
        public bool auto_rotate = true;
        public float rotate_offset;

        private SpriteRenderer render;
        private MeshRenderer mesh_render;
        private Renderer particle_render;
        private float timer = 0f;

        private static List<AutoOrderLayer> order_list = new List<AutoOrderLayer>();
        private List<AutoOrderLayerChild> child_list = new List<AutoOrderLayerChild>();

        private void Awake()
        {
            if (!order_list.Contains(this))
            {
                order_list.Add(this);
            }

            render = GetComponent<SpriteRenderer>();
            mesh_render = GetComponent<MeshRenderer>();
            timer = Random.Range(0f, 0.2f) + 1f;

            if (GetComponent<MeshRenderer>())
                GetComponent<MeshRenderer>().sortingLayerName = "Perspective";

            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (particle) particle_render = particle.GetComponent<Renderer>();

        }

        private void Start()
        {
            RefreshRender();

            if (gameObject.isStatic)
                enabled = false; //No need to update static
        }

        private void OnDestroy()
        {
            order_list.Remove(this);
        }

        void Update()
        {
            if (auto_sort)
            {
                timer += Time.deltaTime;
                if (timer < sort_refresh_rate)
                    return;
                timer -= sort_refresh_rate;

                RefreshSort();
            }
        }

        public int GetSortOrder()
        {
            return Mathf.RoundToInt((transform.position.y + offset) * 100f) * -1;
        }

        public float GetRotateZOffset()
        {
            return -rotate_offset * transform.lossyScale.y;
        }

        public void AddChild(AutoOrderLayerChild child)
        {
            child_list.Add(child);
        }

        public void RefreshSort()
        {
            int sorting_order = GetSortOrder();

            if (render != null && render.sortingOrder != sorting_order)
                render.sortingOrder = sorting_order;
            if (mesh_render != null && mesh_render.sortingOrder != sorting_order)
                mesh_render.sortingOrder = sorting_order;
            if (particle_render != null && particle_render.sortingOrder != sorting_order)
                particle_render.sortingOrder = sorting_order;

            //update childd NOW
            foreach (AutoOrderLayerChild child in child_list)
            {
                if (child != null)
                    child.SetOrder(sorting_order);
            }
        }

        public void RefreshAutoRotate()
        {
            Camera cam = FollowCamera.GetCamera();
            float current_angle = transform.rotation.eulerAngles.z;
            if (!cam.orthographic)
            {
                //Projection
                transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, 0f, current_angle);
                transform.position = new Vector3(transform.position.x, transform.position.y, GetRotateZOffset());
            }
            else
            {
                //Ortho
                transform.rotation = Quaternion.Euler(0f, 0f, current_angle);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }
        }

        public void RefreshRender()
        {
            //Rotation
            if (auto_rotate)
            {
                RefreshAutoRotate();
            }

            if (auto_sort)
            {
                RefreshSort();
            }

            //Refresh child
            foreach (AutoOrderLayerChild child in child_list)
            {
                child.RefreshRender();
            }
        }

        public static void RefreshRenderAll()
        {
            foreach (AutoOrderLayer item in order_list)
            {
                item.RefreshRender();
            }
        }
    }

}