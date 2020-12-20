using UnityEngine;
using System.Collections;

/// <summary>
/// Script for parallax background
/// </summary>

namespace IndieMarc.Platformer
{
    public class ParallaxBackground : MonoBehaviour
    {

        public float speed;

        private Camera main_camera;
        private Renderer render;

        void Start()
        {
            main_camera = FollowCamera.GetCamera();
            render = GetComponent<Renderer>();
        }

        void Update()
        {
            Camera cam = main_camera.GetComponent<Camera>();
            Vector2 offset = new Vector2(cam.transform.position.x * speed, 0f);
            render.material.mainTextureOffset = offset;
            transform.position = new Vector3(cam.transform.position.x, transform.position.y, transform.position.z);

        }
    }

}