using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Top down character movement
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.TopDown
{
    public class PlayerCharacter : MonoBehaviour
    {
        public int player_id;

        [Header("Stats")]
        public float max_hp = 100f;

        [Header("Status")]
        public bool invulnerable = false;

        [Header("Movement")]
        public float move_accel = 1f;
        public float move_deccel = 1f;
        public float move_max = 1f;

        public UnityAction onDeath;
        public UnityAction onHit;

        private Rigidbody2D rigid;
        private Animator animator;
        private AutoOrderLayer auto_order;
        private ContactFilter2D contact_filter;

        private float hp;
        private bool is_dead = false;
        private Vector2 move;
        private Vector2 move_input;
        private Vector2 lookat = Vector2.zero;
        private float side = 1f;
        private bool disable_controls = false;
        private float hit_timer = 0f;

        private static Dictionary<int, PlayerCharacter> character_list = new Dictionary<int, PlayerCharacter>();

        void Awake()
        {
            character_list[player_id] = this;
            rigid = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            auto_order = GetComponent<AutoOrderLayer>();
            hp = max_hp;
        }

        void OnDestroy()
        {
            character_list.Remove(player_id);
        }

        void Start()
        {

        }

        //Handle physics
        void FixedUpdate()
        {
            //Movement velocity
            float desiredSpeedX = Mathf.Abs(move_input.x) > 0.1f ? move_input.x * move_max : 0f;
            float accelerationX = Mathf.Abs(move_input.x) > 0.1f ? move_accel : move_deccel;
            move.x = Mathf.MoveTowards(move.x, desiredSpeedX, accelerationX * Time.fixedDeltaTime);
            float desiredSpeedY = Mathf.Abs(move_input.y) > 0.1f ? move_input.y * move_max : 0f;
            float accelerationY = Mathf.Abs(move_input.y) > 0.1f ? move_accel : move_deccel;
            move.y = Mathf.MoveTowards(move.y, desiredSpeedY, accelerationY * Time.fixedDeltaTime);

            //Move
            rigid.velocity = move;
            
        }

        //Handle render and controls
        void Update()
        {
            hit_timer += Time.deltaTime;
            move_input = Vector2.zero;

            //Controls
            if (!disable_controls)
            {
                //Controls
                PlayerControls controls = PlayerControls.Get(player_id);
                move_input = controls.GetMove();
                
            }

            //Update lookat side
            if (move.magnitude > 0.1f)
                lookat = move.normalized;
            if (Mathf.Abs(lookat.x) > 0.02)
                side = Mathf.Sign(lookat.x);
            
        }

        public void HealDamage(float heal)
        {
            if (!is_dead)
            {
                hp += heal;
                hp = Mathf.Min(hp, max_hp);
            }
        }

        public void TakeDamage(float damage)
        {
            if (!is_dead && !invulnerable && hit_timer > 0f)
            {
                hp -= damage;
                hit_timer = -1f;

                if (hp <= 0f)
                {
                    Kill();
                }
                else
                {
                    if (onHit != null)
                        onHit.Invoke();
                }
            }
        }

        public void Kill()
        {
            if (!is_dead)
            {
                is_dead = true;
                rigid.velocity = Vector2.zero;
                move = Vector2.zero;
                move_input = Vector2.zero;

                if (onDeath != null)
                    onDeath.Invoke();
            }
        }
        
        public void Teleport(Vector3 pos)
        {
            transform.position = pos;
            move = Vector2.zero;
        }

        public Vector2 GetMove()
        {
            return move;
        }

        public Vector2 GetFacing()
        {
            return lookat;
        }

        public int GetSortOrder()
        {
            return auto_order.GetSortOrder();
        }

        //Get Character side
        public float GetSide()
        {
            return side; //Return 1 frame before to let anim do transitions
        }

        public int GetSideAnim()
        {
            return (side >= 0) ? 1 : 3;
        }

        public bool IsDead()
        {
            return is_dead;
        }

        public void DisableControls() { disable_controls = true; }
        public void EnableControls() { disable_controls = false; }
        
        public static PlayerCharacter GetNearest(Vector3 pos, float range = 999f, bool alive_only=true)
        {
            PlayerCharacter nearest = null;
            float min_dist = range;
            foreach (PlayerCharacter character in character_list.Values)
            {
                if (!alive_only || !character.IsDead())
                {
                    float dist = (pos - character.transform.position).magnitude;
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nearest = character;
                    }
                }
            }
            return nearest;
        }

        public static PlayerCharacter Get(int player_id)
        {
            foreach (PlayerCharacter character in character_list.Values)
            {
                if (character.player_id == player_id)
                {
                    return character;
                }
            }
            return null;
        }

        public static PlayerCharacter[] GetAll()
        {
            PlayerCharacter[] list = new PlayerCharacter[character_list.Count];
            character_list.Values.CopyTo(list, 0);
            return list;
        }
    }
}
