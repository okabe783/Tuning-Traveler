using UnityEngine;

namespace TuningTraveler
{
    public class CharMove : MonoBehaviour
    {
        protected Animator _animator;

        //回転
        protected Quaternion _targetRotation;

        private void Awake()
        {
            TryGetComponent(out _animator);
            _targetRotation = transform.rotation;
        }

        private void Update()
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            //カメラの方向に合わせて水平な回転を行い位置を合わせる
            var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
            var velo = horizontalRotation * new Vector3(h, 0, v).normalized;
            var _speed = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
            var _rotationSpeed = 600 * Time.deltaTime;
            //移動方向を向く
            if (velo.magnitude > 0.5f)
            {
                transform.rotation = Quaternion.LookRotation(velo, Vector3.up);
                _targetRotation = Quaternion.LookRotation(velo, Vector3.up);
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _rotationSpeed);
            //移動速度をAnimatorに反映
            _animator.SetFloat("Speed", velo.magnitude * _speed, 0.1f, Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _rotationSpeed);
        }
    }
}
