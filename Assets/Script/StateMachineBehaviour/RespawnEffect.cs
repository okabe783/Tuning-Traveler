using UnityEngine;

namespace TuningTraveler
{
    public class RespawnEffect : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            animator.GetComponent<PlayerController>().Respawn();
        }
    }

}
