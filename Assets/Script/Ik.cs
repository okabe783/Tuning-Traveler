using UnityEngine;

public class Ik : MonoBehaviour
{
    protected Animator _animator;
    
    public bool _ikActive = true;
    public Transform _leftHandObj = null;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    //IKを計算するためのコールバック
    void OnAnimatorIK(int layerIndex)
    {
        if (_animator)
        {
            //IKが有効ならば、位置と回転を直接設定する
            if (_ikActive)
            {
                if (_leftHandObj != null)
                {
                    _animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
                    _animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);
                    
                    _animator.SetIKPosition(AvatarIKGoal.RightHand,_leftHandObj.position);
                    _animator.SetIKRotation(AvatarIKGoal.RightHand,_leftHandObj.rotation);
                }
            }
            //IKが有効でなければ、手と頭の位置と回転を元の位置に戻す
            else
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightHand,0);
            }
        }
    }
}
