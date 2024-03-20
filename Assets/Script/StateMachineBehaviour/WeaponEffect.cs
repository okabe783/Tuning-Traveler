using UnityEngine;

namespace TuningTraveler
{
    public class WeaponEffect : StateMachineBehaviour
    {
        public int _effectIndex;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            var ctrl = animator.GetComponent<PlayerController>();
            ctrl._weapon._effects[_effectIndex].Active();
        }
    }
}


