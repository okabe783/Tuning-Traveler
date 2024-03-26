using UnityEngine;
using UnityEngine.Animations;

namespace TuningTraveler
{
    /// <summary>
    /// Animationの拡張
    /// </summary>
    public class SceneLinkedSMB<TMonoBehaviour> : SealedSMB
        where TMonoBehaviour : MonoBehaviour
    {
        protected TMonoBehaviour _monoBehaviour;
        private bool _firstFrameHappened;
        private bool _lastFrameHappened;

        /// <summary>
        /// SceneLinkedSMBクラスが複数のAnimStateに関連付けられた場合に、それらのすべてのアニメーションステートに対して一括で初期化処理を行う
        /// </summary>
        public static void Initialise(Animator animator, TMonoBehaviour monoBehaviour)
        {
            var sceneLinkedSMBs = animator.GetBehaviours<SceneLinkedSMB<TMonoBehaviour>>();
            for (var i = 0; i < sceneLinkedSMBs.Length; i++)
            {
                sceneLinkedSMBs[i].InternalInitialise(animator,monoBehaviour);
            }
        }

        /// <summary>
        /// 内部的な初期化を行いアニメーターとMonoBehaviourの参照を保存し関連付けて初期化
        /// </summary>
        private void InternalInitialise(Animator animator, TMonoBehaviour monoBehaviour)
        {
            _monoBehaviour = monoBehaviour;
            OnStart(animator);
        }

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,AnimatorControllerPlayable controller)
        {
            _firstFrameHappened = false;
            OnSLStateEnter(animator, stateInfo, layerIndex);
            OnSLStateEnter(animator,stateInfo,layerIndex,controller);
        }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller)
        {
            if(!animator.gameObject.activeSelf)
                return;
            if (animator.IsInTransition(layerIndex) &&
                animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
            {
                OnSLTransitionToStateUpdate(animator, stateInfo, layerIndex);
                OnSLTransitionToStateUpdate(animator,stateInfo,layerIndex,controller);
            }

            if (!animator.IsInTransition(layerIndex) && _firstFrameHappened)
            {
                OnSLTransitionToStateUpdate(animator,stateInfo,layerIndex);
                OnSLTransitionToStateUpdate(animator,stateInfo,layerIndex,controller);
            }

            if (!animator.IsInTransition(layerIndex) && _firstFrameHappened)
            {
                OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);
                OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex, controller);
            }

            if (animator.IsInTransition(layerIndex) && !_lastFrameHappened && _firstFrameHappened)
            {
                _lastFrameHappened = true;
                OnSLStatePreExit(animator, stateInfo, layerIndex);
                OnSLStatePreExit(animator, stateInfo, layerIndex, controller);
            }

            if (!animator.IsInTransition(layerIndex) && !_firstFrameHappened)
            {
                _firstFrameHappened = true;
                OnSLStatePostEnter(animator, stateInfo, layerIndex);
                OnSLStatePostEnter(animator, stateInfo, layerIndex,controller);
            }

            if (animator.IsInTransition(layerIndex) && animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash ==
                stateInfo.fullPathHash)
            {
                OnSLTransitionFromStateUpdate(animator, stateInfo, layerIndex);
                OnSLTransitionFromStateUpdate(animator, stateInfo, layerIndex,controller);
            }
        }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller)
        {
            _lastFrameHappened = false;
            OnSLStateExit(animator,stateInfo,layerIndex);
            OnSLStateExit(animator,stateInfo,layerIndex,controller);
        }
        protected virtual void OnStart(Animator animator) { }
        /// <summary> アニメーションステートが実行され始める直前（ステートへの遷移時に呼び出される </summary>
        protected virtual void OnSLStateEnter(Animator animator,AnimatorStateInfo stateInfo,int layerIndex){ }
        /// <summary>AnimStateのトランジション中にStateが実行されている際に毎フレーム呼び出す</summary>
        protected virtual void OnSLTransitionToStateUpdate(Animator animator,AnimatorStateInfo stateInfo,int layerIndex){ }
        /// <summary>Updateの後stateの実行が最初に終了した時（ステートから遷移した後）に呼び出す</summary>
        protected virtual void OnSLStatePreExit(Animator animator,AnimatorStateInfo stateInfo,int layerIndex){ }
        protected virtual void OnSLStateNoTransitionUpdate(Animator animator,AnimatorStateInfo stateInfo,int layerIndex){ }
        /// <summary>アニメーションステートへの遷移が完了した直後の最初のフレームで特定の処理が呼び出される</summary>
        protected virtual void OnSLStatePostEnter(Animator animator,AnimatorStateInfo stateInfo,int layerIndex){ }
        /// <summary>OnSLStatePreExitの後状態への遷移中の各フレームに呼び出される</summary>
        protected virtual void OnSLTransitionFromStateUpdate(Animator animator,AnimatorStateInfo stateInfo,int layerIndex){ }
        protected virtual void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){ }
        
        protected virtual void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
        protected virtual void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
        protected virtual void OnSLTransitionToStateUpdate(Animator animator,AnimatorStateInfo stateInfo,int layerIndex,AnimatorControllerPlayable controller){ }
        /// <summary>
        /// 前のアニメーションステートからの遷移が開始された瞬間またはその直後の最初のフレームでこの処理が行われる
        /// 遷移の継続時間が1フレーム未満の場合この処理は呼び出されない
        /// </summary>
        protected virtual void OnSLStatePreExit(Animator animator,AnimatorStateInfo stateInfo,int layerIndex,AnimatorControllerPlayable controller){ }
        protected virtual void OnSLStatePostEnter(Animator animator,AnimatorStateInfo stateInfo,int layerIndex,AnimatorControllerPlayable controller){ }
        protected virtual void OnSLTransitionFromStateUpdate(Animator animator,AnimatorStateInfo stateInfo,int layerIndex,AnimatorControllerPlayable controller){ }
        protected virtual void OnSLStateExit(Animator animator,AnimatorStateInfo stateInfo,int layerIndex,AnimatorControllerPlayable controller){ }
    }

    public abstract class SealedSMB : StateMachineBehaviour
    {
        //sealed =　派生クラスでoverrideできなくなる
        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }
}