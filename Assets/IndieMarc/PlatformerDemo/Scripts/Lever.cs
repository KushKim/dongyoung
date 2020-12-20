using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Lever script
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>
/// 

namespace IndieMarc.Platformer
{

    public enum LeverState
    {
        left, center, right, disabled
    }

    public class Lever : MonoBehaviour
    {
        public Sprite lever_center;
        public Sprite lever_left;
        public Sprite lever_right;
        public Sprite lever_disabled;

        public bool can_be_center;
        public LeverState state;
        public int door_value = 1;
        public bool no_return = false;
        public bool reset_on_dead = true;

        private SpriteRenderer render;
        private LeverState start_state;
        private LeverState prev_state;
        private float timer = 0f;

        public UnityAction OnTriggerLever;

        private static List<Lever> levers = new List<Lever>();

        private void Awake()
        {
            levers.Add(this);
            render = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            start_state = state;
            prev_state = state;
            ChangeSprite();
        }

        void Update()
        {

            timer += Time.deltaTime;

            if (state != prev_state)
            {
                ChangeSprite();
                prev_state = state;
            }
        }

        private void OnDestroy()
        {
            levers.Remove(this);
        }

        void OnTriggerEnter2D(Collider2D coll)
        {
            if (coll.gameObject.GetComponent<PlayerCharacter>())
            {
                if (state == LeverState.disabled)
                    return;
                
                Activate();
            }
        }

        public void Activate()
        {
            //Can't activate twice very fast
            if (timer < 0f)
                return;

            if (!no_return || state == start_state)
            {
                timer = -0.8f;

                //Change state
                if (state == LeverState.left)
                {
                    state = (can_be_center) ? LeverState.center : LeverState.right;
                }
                else if (state == LeverState.center)
                {
                    state = LeverState.right;
                }
                else if (state == LeverState.right)
                {
                    state = LeverState.left;
                }
                
                //Audio
                GetComponent<AudioSource>().Play();

                //Trigger
                if (OnTriggerLever != null)
                    OnTriggerLever.Invoke();
            }
        }

        private void ChangeSprite()
        {
            if (state == LeverState.left)
            {
                render.sprite = lever_left;
            }
            if (state == LeverState.center)
            {
                render.sprite = lever_center;
            }
            if (state == LeverState.right)
            {
                render.sprite = lever_right;
            }
            if (state == LeverState.disabled)
            {
                render.sprite = lever_disabled;
            }

            if (no_return && state != start_state)
            {
                render.sprite = lever_disabled;
            }
        }
        
        public void ResetOne()
        {
            if (reset_on_dead)
            {
                state = start_state;
            }
        }

        public static void ResetAll()
        {
            foreach (Lever lever in levers)
            {
                lever.ResetOne();
            }
        }
    }

}