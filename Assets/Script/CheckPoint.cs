using System;
using UnityEngine;

namespace TuningTraveler
{
    [RequireComponent(typeof(Collider))]
    public class CheckPoint : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("");
        }

        private void OnTriggerEnter(Collider other)
        {
            var ctrl = other.GetComponent<PlayerController>();
            if(ctrl == null)
                return;
            
            ctrl.SetCheckPoint(this);
        }
    }

}
