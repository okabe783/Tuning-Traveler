using UnityEngine;

namespace TuningTraveler
{
    /// <summary>
    /// Animationの特定の状態から出た時、PlayerCtrlのRespawnFinishを呼び出す
    /// </summary>
    public class TargetableSMB : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int Index)
        {
            var controller = animator.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.RespawnFinished();
            }
        }
    }
}
