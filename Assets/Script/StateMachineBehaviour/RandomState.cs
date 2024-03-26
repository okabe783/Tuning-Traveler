using UnityEngine;

namespace TuningTraveler
{
    /// <summary>
    /// animationをより多様化する
    /// </summary>
    public class RandomState : StateMachineBehaviour
    {
        public int _numberOfStates = 3;
        //基準となる時間
        public float _minNormTime = 0f;
        public float _maxNormTime = 5f;
        
        private float _randomNormTime;
        private readonly int _hashRandomIdle = Animator.StringToHash("RandomIdle"); 
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,int layerIndex)
        {
            _randomNormTime = Random.Range(_minNormTime, _maxNormTime);　//特定のアニメーションステートに入る際にランダムな時間を決定
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //特定のAnimationStateから遷移する際にRandomIdleParamをresetする
            if (animator.IsInTransition(0) &&
                animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateInfo.fullPathHash)
            {
                animator.SetInteger(_hashRandomIdle,-1);
            }
            //特定の条件下でrandomなIdleを設定する
            if (stateInfo.normalizedTime > _randomNormTime && !animator.IsInTransition(0))
            {
                animator.SetInteger(_hashRandomIdle,Random.Range(0,_numberOfStates));
            }
        }
    }
}
