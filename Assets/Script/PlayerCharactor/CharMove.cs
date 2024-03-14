using System.Collections;
using UnityEngine;

    public class CharMove : MonoBehaviour
    {
        public static CharMove Instance => _instance;
        private static CharMove _instance;
        
        [HideInInspector] 
        public bool _playerCtrlInputBlocked; //playerの入力を受け付けるかどうか
        
        private Vector2 _move;
        private Vector2 _camera;
        private bool _jump;
        private bool _attack;
        private bool _pause;
        private bool _externalInputBlocked; //外部からの入力を無視するかの判定
        public Vector2 moveInput
        {
            get
            {
                if (_playerCtrlInputBlocked || _externalInputBlocked)
                    return Vector2.zero;
                return _move;
            }
        }

        public bool JumpInput => _jump && !_playerCtrlInputBlocked && _externalInputBlocked;
        public bool Attack => _attack && !_playerCtrlInputBlocked && _externalInputBlocked;
        public bool Pause => _pause;

        private WaitForSeconds _attackInputWait;
        private Coroutine _attackWaitCoroutine;
        private const float _attackDuration = 0.03f;
        private void Awake()
        {
            _attackInputWait = new WaitForSeconds(_attackDuration);
            if (_instance == null)
                _instance = this;
        }

        private void Update()
        {
            _move.Set(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
            _camera.Set(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
            _jump = Input.GetButton("Jump");

            if (Input.GetButtonDown("Fire1"))
            {
                if(_attackWaitCoroutine != null)
                    StopCoroutine(_attackWaitCoroutine);

                _attackWaitCoroutine = StartCoroutine(AttackWait());
            }

            _pause = Input.GetButtonDown("Pause");
        }

        private IEnumerator AttackWait()
        {
            _attack = true;
            yield return _attackInputWait;
            _attack = false;
        }
    }