using UnityEngine;

namespace TuningTraveler
{
    public class CharMove : MonoBehaviour
    {
        private Animator _animator;

        //回転
        private Quaternion _targetRotation;

        public bool _playerCtrlInputBlocked;
        public Vector2 _move;

        public Vector2 moveInput
        {
            get
            {
                if (_playerCtrlInputBlocked)
                    return Vector2.zero;
                return _move;
            }
        }
        private void Awake()
        {
            TryGetComponent(out _animator);
            _targetRotation = transform.rotation;
        }

        private void Update()
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            _move.Set(h,v);
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
