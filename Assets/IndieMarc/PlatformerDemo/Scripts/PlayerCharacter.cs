using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Platformer character movement
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.Platformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
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

        [Header("Jump")]
        public bool can_jump = true;
        public bool double_jump = true;
        //public bool jump_on_enemies = true;
        public float jump_strength = 1f;
        public float jump_time_min = 1f;
        public float jump_time_max = 1f;
        public float jump_gravity = 1f;
        public float jump_fall_gravity = 1f;
        public float jump_move_percent = 0.75f;
        public LayerMask ground_layer;
        public float ground_raycast_dist = 0.1f;

        [Header("Crouch")]
        public bool can_crouch = true;
        public float crouch_coll_percent = 0.5f;

        [Header("Fall Below Level")]
        public bool reset_when_fall = true;
        public float fall_pos_y = -5f;
        public float fall_damage_percent = 0.25f;

        public UnityAction onDeath;
        public UnityAction onHit;
        public UnityAction onJump;
        public UnityAction onLand;
        public UnityAction onCrouch;

        private Rigidbody2D rigid;
        private CapsuleCollider2D capsule_coll;
        private ContactFilter2D contact_filter;
        private Vector2 coll_start_h;
        private Vector2 coll_start_off;
        private Vector3 start_scale;
        private Vector3 last_ground_pos;
        private Vector3 average_ground_pos;

        private Vector2 move;
        private Vector2 move_input;
        private bool jump_press;
        private bool jump_hold;

        private float hp;
        private bool is_dead = false;
        private bool was_grounded = false;
        private bool is_grounded = false;
        private bool is_ceiled = false;
        private bool is_crouch = false;
        private bool is_jumping = false;
        private bool is_double_jump = false;
        private bool disable_controls = false;
        private float grounded_timer = 0f;
        private float jump_timer = 0f;
        private float hit_timer = 0f;

        private static Dictionary<int, PlayerCharacter> character_list = new Dictionary<int, PlayerCharacter>();

        void Awake()
        {
            character_list[player_id] = this;
            rigid = GetComponent<Rigidbody2D>();
            capsule_coll = GetComponent<CapsuleCollider2D>();
            coll_start_h = capsule_coll.size;
            coll_start_off = capsule_coll.offset;
            start_scale = transform.localScale;
            average_ground_pos = transform.position;
            last_ground_pos = transform.position;
            hp = max_hp;

            contact_filter = new ContactFilter2D();
            contact_filter.layerMask = ground_layer;
            contact_filter.useLayerMask = true;
            contact_filter.useTriggers = false;

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
            if (is_dead)
                return;

            //Movement velocity
            float desiredSpeed = Mathf.Abs(move_input.x) > 0.1f ? move_input.x * move_max : 0f;
            float acceleration = Mathf.Abs(move_input.x) > 0.1f ? move_accel : move_deccel;
            acceleration = !is_grounded ? jump_move_percent * acceleration : acceleration;
            move.x = Mathf.MoveTowards(move.x, desiredSpeed, acceleration * Time.fixedDeltaTime);

            UpdateFacing();
            UpdateJump();
            UpdateCrouch();

            //Move
            rigid.velocity = move;
        }

        //Handle render and controls
        void Update()
        {
            if (is_dead)
                return;

            hit_timer += Time.deltaTime;
            grounded_timer += Time.deltaTime;

            //Controls
            PlayerControls controls = PlayerControls.Get(player_id);
            move_input = !disable_controls ? controls.GetMove() : Vector2.zero;
            jump_press = !disable_controls ? controls.GetJumpDown() : false;
            jump_hold = !disable_controls ? controls.GetJumpHold() : false;

            if (jump_press || move_input.y > 0.5f)
                Jump();

            //Reset when fall
            if (transform.position.y < fall_pos_y - GetSize().y)
            {
                TakeDamage(max_hp * fall_damage_percent);
                if (reset_when_fall)
                    Teleport(last_ground_pos);
            }
        }

        private void UpdateFacing()
        {
            if (Mathf.Abs(move.x) > 0.01f)
            {
                float side = (move.x < 0f) ? -1f : 1f;
                transform.localScale = new Vector3(start_scale.x * side, start_scale.y, start_scale.z);
            }
        }

        private void UpdateJump()
        {
            //Jump
            was_grounded = is_grounded;
            is_grounded = DetectGrounded(false);
            is_ceiled = DetectGrounded(true);
            jump_timer += Time.fixedDeltaTime;

            //Jump end timer
            if (is_jumping && !jump_hold && jump_timer > jump_time_min)
                is_jumping = false;
            if (is_jumping && jump_timer > jump_time_max)
                is_jumping = false;

            //Jump hit ceil
            if (is_ceiled)
            {
                is_jumping = false;
                move.y = Mathf.Min(move.y, 0f);
            }

            //Add jump velocity
            if (!is_grounded)
            {
                //Falling
                float gravity = !is_jumping ? jump_fall_gravity : jump_gravity; //Gravity increased when going down
                move.y = Mathf.MoveTowards(move.y, -move_max * 2f, gravity * Time.fixedDeltaTime);
            }
            else if (!is_jumping)
            {
                //Grounded
                move.y = 0f;
            }

            if (!is_grounded)
                grounded_timer = 0f;

            //Average grounded pos
            if (!was_grounded && is_grounded)
                average_ground_pos = transform.position;
            if (is_grounded)
                average_ground_pos = Vector3.Lerp(transform.position, average_ground_pos, 1f * Time.deltaTime);

            //Save last landed position
            if (is_grounded && grounded_timer > 1f)
                last_ground_pos = average_ground_pos;

            if (!was_grounded && is_grounded)
            {
                if (onLand != null)
                    onLand.Invoke();
            }
        }

        private void UpdateCrouch()
        {
            if (!can_crouch)
                return;

            //Crouch
            bool was_crouch = is_crouch;
            if (move_input.y < -0.1f && is_grounded)
            {
                is_crouch = true;
                move = Vector2.zero;
                capsule_coll.size = new Vector2(coll_start_h.x, coll_start_h.y * crouch_coll_percent);
                capsule_coll.offset = new Vector2(coll_start_off.x, coll_start_off.y - coll_start_h.y * (1f - crouch_coll_percent) / 2f);

                if (!was_crouch && is_crouch)
                {
                    if (onCrouch != null)
                        onCrouch.Invoke();
                }
            }
            else
            {
                is_crouch = false;
                capsule_coll.size = coll_start_h;
                capsule_coll.offset = coll_start_off;
            }
        }

        public void Jump(bool force_jump = false)
        {
            if (can_jump && (!is_crouch || force_jump))
            {
                if (is_grounded || force_jump || (!is_double_jump && double_jump))
                {
                    is_double_jump = !is_grounded;
                    move.y = jump_strength;
                    jump_timer = 0f;
                    is_jumping = true;
                    if (onJump != null)
                        onJump.Invoke();
                }
            }
        }

        private bool DetectGrounded(bool detect_ceiled)
        {
            bool grounded = false;
            Vector2[] raycastPositions = new Vector2[3];

            Vector2 raycast_start = rigid.position;
            Vector2 orientation = detect_ceiled ? Vector2.up : Vector2.down;
            float radius = GetSize().x * 0.5f * transform.localScale.y; ;

            if (capsule_coll != null)
            {
                //Adapt raycast to collider
                Vector2 raycast_offset = capsule_coll.offset + orientation * Mathf.Abs(capsule_coll.size.y * 0.5f - capsule_coll.size.x * 0.5f);
                raycast_start = rigid.position + raycast_offset * transform.localScale.y;
            }

            float ray_size = radius + ground_raycast_dist;
            raycastPositions[0] = raycast_start + Vector2.left * radius / 2f;
            raycastPositions[1] = raycast_start;
            raycastPositions[2] = raycast_start + Vector2.right * radius / 2f;

            RaycastHit2D[] hitBuffer = new RaycastHit2D[5];
            for (int i = 0; i < raycastPositions.Length; i++)
            {
                Physics2D.Raycast(raycastPositions[i], orientation, contact_filter, hitBuffer, ray_size);
                Debug.DrawRay(raycastPositions[i], orientation * ray_size);
                for (int j = 0; j < hitBuffer.Length; j++)
                {
                    if (hitBuffer[j].collider != null && hitBuffer[j].collider != capsule_coll && !hitBuffer[j].collider.isTrigger)
                    {
                        grounded = true;
                    }
                }
            }
            return grounded;
        }

        public void Teleport(Vector3 pos)
        {
            transform.position = pos;
            move = Vector2.zero;
            is_jumping = false;
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

        public void DisableControls() { disable_controls = true; }
        public void EnableControls() { disable_controls = false; }

        public Vector2 GetMove()
        {
            return move;
        }

        public Vector2 GetFacing()
        {
            return Vector2.right * Mathf.Sign(transform.localScale.x);
        }

        public bool IsJumping()
        {
            return is_jumping;
        }

        public bool IsGrounded()
        {
            return is_grounded;
        }

        public bool IsCrouching()
        {
            return is_crouch;
        }

        public float GetHP()
        {
            return hp;
        }

        public bool IsDead()
        {
            return is_dead;
        }

        public Vector2 GetSize()
        {
            if (capsule_coll != null)
                return new Vector2(Mathf.Abs(transform.localScale.x) * capsule_coll.size.x, Mathf.Abs(transform.localScale.y) * capsule_coll.size.y);
            return new Vector2(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (is_dead)
                return;
            
        }

        public static PlayerCharacter GetNearest(Vector3 pos, float range = 99999f, bool alive_only=false)
        {
            PlayerCharacter nearest = null;
            float min_dist = range;
            foreach (PlayerCharacter character in GetAll())
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
            foreach (PlayerCharacter character in GetAll())
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