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
    public class AutoOrderLayerChild : MonoBehaviour
    {
        [Header("Auto Order Child")]
        public AutoOrderLayer parent;
        public int order_offset;

        [Header("Auto Rotate")]
        public bool auto_rotate = false;
        public float rotate_offset;

        private SpriteRenderer render;
        private MeshRenderer mesh_render;
        private Renderer particle_render;
        private Canvas canvas;
        private Vector3 start_rot;

        void Awake()
        {
            render = GetComponent<SpriteRenderer>();
            mesh_render = GetComponent<MeshRenderer>();
            canvas = GetComponent<Canvas>();
            start_rot = transform.rotation.eulerAngles;
            parent.AddChild(this);

            if (GetComponent<MeshRenderer>())
                GetComponent<MeshRenderer>().sortingLayerName = "Perspective";

            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (particle) particle_render = particle.GetComponent<Renderer>();
        }

        private void Start()
        {
            RefreshRender();
        }

        public void SetOrder(int order)
        {
            if (render != null)
                render.sortingOrder = order + order_offset;
            if (mesh_render != null)
                mesh_render.sortingOrder = order + order_offset;
            if (particle_render != null)
                particle_render.sortingOrder = order + order_offset;
            if (canvas != null)
                canvas.sortingOrder = order + order_offset;
        }

        public void RefreshRender()
        {
            //Rotation
            if (auto_rotate)
            {
                Camera cam = FollowCamera.GetCamera();

                if (!cam.orthographic)
                {
                    transform.rotation = Quaternion.Euler(start_rot + Vector3.right * cam.transform.rotation.eulerAngles.x);
                    transform.position = new Vector3(transform.position.x, transform.position.y, -rotate_offset * transform.lossyScale.y);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(start_rot);
                    transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                }
            }
        }
    }

}