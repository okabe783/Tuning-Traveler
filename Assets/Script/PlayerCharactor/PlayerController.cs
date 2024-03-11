using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

namespace TuningTraveler
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController _instance;
        public static PlayerController instance => _instance;
        
        private bool _respawning; //PlayerがRespawnしているかどうか
        public bool respawning => _respawning;

        public float _maxForwardSpeed = 8f;
        public bool _canAttack; //攻撃判定
        public float _gravity = 20f;
        public float _jumpSpeed = 10f;
        private bool _inAttack; //攻撃中かどうか
        private bool _inCombo; //連続攻撃をしているかどうか
        
        public Weapon _weapon;
        private Animator _animator;
        private CharacterController _charCtrl;
        private Damageable _damageable;
        private Renderer[] _renderers;
        private CharMove _charMove;
        //アニメータコントローラーの現在の状態や進行状況
        private AnimatorStateInfo _previousStateInfo;
        private AnimatorStateInfo _currentStateInfo;
        private AnimatorStateInfo _nextStateInfo;
        private bool _isAnimatorTransitioning; //トランジション中かどうか
        private float _desiredForwardSpeed; //地面を移動する速さ
        private bool _isGrounded = true; //現在地面に立っているか
        private bool _readyToJump; //jumpできる状態かどうか
        private float _verticalSpeed; //現在の上昇、下降の速さ
        //AudioSource
        public RandomAudioPlayer _footstepPlayer;
        public RandomAudioPlayer _hurtAudioPlayer;
        public RandomAudioPlayer _landingPlayer;

        private const float _stickingGravityProportion = 0.3f; //地面に接しているときの重力
        private const float _groundAcceleration = 20f; //地上での加速
        private const float _groundDeceleration = 25f;　//地上での減速
        private const float _jumpAbortSpeed = 10f;
        private float _forwardSpeed; //現在のspeed
        //パラメーター
        private readonly int _hashWeaponAttack = Animator.StringToHash("WeaponAttack");
        private readonly int _hashStateTime = Animator.StringToHash("");
        private readonly int _hashForwardSpeed = Animator.StringToHash("");
        //State
        private readonly int _hashCombo1 = Animator.StringToHash("");
        private readonly int _hashCombo2 = Animator.StringToHash("");
        private readonly int _hashCombo3 = Animator.StringToHash("");
        private readonly int _hashCombo4 = Animator.StringToHash("");
        //Tag
        private readonly int _hashBlockInput = Animator.StringToHash("BlockInput");
        public void SetCanAttack(bool canAttack)
        
        {
            this._canAttack = canAttack;
        }
        /// <summary>
        /// playerが移動入力を行っているか
        /// </summary>
        private bool IsMoveInput => !Mathf.Approximately(_charMove.moveInput.sqrMagnitude, 0f);
        /// <summary>
        /// scriptが再設定されるときに正しく機能させるための初期化処理
        /// </summary>
        private void Reset()
        {
            _weapon = GetComponent<Weapon>();
            //足音がなる場所のTransformを返す
            Transform footStepSource = transform.Find("");
            if (footStepSource != null)
                _footstepPlayer = footStepSource.GetComponent<RandomAudioPlayer>();
            
            Transform hurtSource = transform.Find("");
            if (hurtSource != null)
                _hurtAudioPlayer = hurtSource.GetComponent<RandomAudioPlayer>();

            Transform landingSource = transform.Find("");
            if (landingSource != null)
                _landingPlayer = landingSource.GetComponent<RandomAudioPlayer>();
        }

        private void Awake()
        {
            _charMove = GetComponent<CharMove>();
            _animator = GetComponent<Animator>();
            _charCtrl = GetComponent<CharacterController>();
            _weapon.SetOwner(gameObject);
            _instance = this;
        }

        private void OnEnable()
        {
            _damageable = GetComponent<Damageable>();
            _damageable._isInvincible = true;
            EquipWeapon(false);
            _renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = true;
            }
        }
        /// <summary>
        /// PhysicsステップごとにUnityから自動的に呼び出される
        /// </summary>
        private void FixedUpdate()
        {
            CacheAnimatorState();
            UpdateInputBlocking();
            EquipWeapon(IsWeaponEquip());
            //現在のアニメータの再生時間を0から1の範囲で取得し設定
            _animator.SetFloat(_hashStateTime,
                Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime,1f));
            //トリガーをリセット
            _animator.ResetTrigger(_hashWeaponAttack);
            //攻撃アニメーションの再生
            if (_charMove.Attack && _canAttack)
                _animator.SetTrigger(_hashWeaponAttack);

            CalculateForwardMovement();
            CalculateVerticalMovement();
        }

        /// <summary>
        /// animatorのBaseLayerの現在の状態を記録する
        /// </summary>
        private void CacheAnimatorState()
        {
            _previousStateInfo = _currentStateInfo;
            _currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _nextStateInfo = _animator.GetNextAnimatorStateInfo(0);
            _isAnimatorTransitioning = _animator.IsInTransition(0);
        }
        /// <summary>
        /// //アニメーターの状態がキャッシュされた後に呼び出され、このスクリプトがユーザー入力をブロックすべきかどうかを決定する
        /// </summary>
        private void UpdateInputBlocking()
        {
            bool inputBlocked = _currentStateInfo.tagHash == _hashBlockInput && !_isAnimatorTransitioning;
            inputBlocked |= _nextStateInfo.tagHash == _hashBlockInput;
            _charMove._playerCtrlInputBlocked = inputBlocked;
        }

        /// <summary>
        /// Playerが武器を装備しているかどうかの判定
        /// </summary>
        /// <returns></returns>
        private bool IsWeaponEquip()
        {
            bool equipped = _nextStateInfo.shortNameHash == _hashCombo1 ||
                            _currentStateInfo.shortNameHash == _hashCombo1;
            equipped |= _nextStateInfo.shortNameHash == _hashCombo2 ||
                        _currentStateInfo.shortNameHash == _hashCombo2;
            equipped |= _nextStateInfo.shortNameHash == _hashCombo3 ||
                        _currentStateInfo.shortNameHash == _hashCombo3;
            equipped |= _nextStateInfo.shortNameHash == _hashCombo4 ||
                        _currentStateInfo.shortNameHash == _hashCombo4;
            return equipped;
        }
        /// <summary>
        /// 物理ステップごとに武器が装備されているかを確認し、それに応じた処理を実行
        /// </summary>
        /// <param name="equip"></param>
        private void EquipWeapon(bool equip)
        {
            _weapon.gameObject.SetActive(equip);
            _inAttack = false;
            _inCombo = equip;

            if (!equip)
            {
                _animator.ResetTrigger(_hashWeaponAttack);
            }
        }
        //playerの前方向を計算しAnimationを制御するparamを設定
        private void CalculateForwardMovement()
        {
            //移動入力をキャッシュし、その大きさを1以下に制限
            Vector2 moveInput = _charMove.moveInput;
            // sqrMagnitude = ベクトルの大きさの2乗を返すプロパティ
            if(moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();
            //playerの入力に基づいて速度を計算
            _desiredForwardSpeed = moveInput.magnitude * _maxForwardSpeed; 
            //現在の移動入力に基づいて速度の変化を決定する
            float acceleration = IsMoveInput ? _groundAcceleration : _groundDeceleration;　
            //目標速度に向かって前方速度を調整
            _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, _desiredForwardSpeed, 
                acceleration * Time.deltaTime);
            //アニメーターのパラメーターを設定して、再生されるアニメーションを制御
            _animator.SetFloat(_hashForwardSpeed,_forwardSpeed);
        }

        private void CalculateVerticalMovement()
        {
            //JumpButtonが押されていない場合は、jumpすることができる
            if (!_charMove.JumpInput && _isGrounded)
                _readyToJump = true;
            if (_isGrounded)
            {
                //接地時には地面に密着させるためにわずかにマイナスの垂直スピードを加える
                _verticalSpeed = -_gravity * _stickingGravityProportion;
                //jumpがfalseではない時jumpの準備ができており現在はAttack中ではない
                if (_charMove.JumpInput && _readyToJump && !_inCombo)
                {
                    //以前に設定した垂直Speedを上書きして再びJumpできないようにする
                    _verticalSpeed = _jumpSpeed;
                    _isGrounded = false;
                    _readyToJump = false;
                }
            }
            else
            {
                //JumpButtonを離しても一時停止せずに進行方向に対して追加の上向きの速度を持続する
                if (!_charMove.JumpInput && _verticalSpeed > 0.0f)
                {
                    //Jumpの頂点に達したあと、上方向の速度を徐々に減少させる
                    _verticalSpeed -= _jumpAbortSpeed * Time.deltaTime;
                }
                //jumpの高さを制御
                if (Mathf.Approximately(_verticalSpeed, 0f))
                    _verticalSpeed = 0f;
                //空中にいるときの重力
                _verticalSpeed -= _gravity * Time.deltaTime;
            }
        }
    }
}

