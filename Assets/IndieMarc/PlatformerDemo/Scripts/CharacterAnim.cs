using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.Platformer
{
    [RequireComponent(typeof(PlayerCharacter))]
    [RequireComponent(typeof(Animator))]
    public class CharacterAnim : MonoBehaviour
    {
        private PlayerCharacter character;
        private CharacterHoldItem character_item;
        private Animator animator;

        void Awake()
        {
            character = GetComponent<PlayerCharacter>();
            character_item = GetComponent<CharacterHoldItem>();
            animator = GetComponent<Animator>();

            character.onJump += OnJump;
            character.onCrouch += OnCrouch;
        }

        void Update()
        {

            //Anims
            animator.SetBool("Jumping", character.IsJumping());
            animator.SetBool("InAir", !character.IsGrounded());
            animator.SetBool("Crouching", character.IsCrouching());
            animator.SetFloat("Speed", character.GetMove().magnitude);
            if (character_item != null)
                animator.SetBool("Hold", character_item.GetHeldItem() != null);

        }

        void OnCrouch()
        {
            animator.SetTrigger("Crouch");
        }

        void OnJump()
        {
            animator.SetTrigger("Jump");
        }
    }

}