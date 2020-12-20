using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Key script
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.TopDown
{

    public class Key : MonoBehaviour
    {

        public int key_index = 0;
        public int key_value = 1;

        private string unique_id;
        private CarryItem carry_item;

        void Start()
        {
            carry_item = GetComponent<CarryItem>();
            carry_item.OnTake += OnTake;
            carry_item.OnDrop += OnDrop;
        }

        private void OnTake(GameObject triggerer)
        {
            
        }

        private void OnDrop(GameObject triggerer)
        {
            
        }

        public bool TryOpenDoor(GameObject door)
        {
            if (door.GetComponent<Door>() && door.GetComponent<Door>().CanKeyUnlock(this) && !door.GetComponent<Door>().IsOpened())
            {
                door.GetComponent<Door>().UnlockWithKey(key_value);
                Destroy(gameObject);
                return true;
            }
            return false;
        }
    }

}