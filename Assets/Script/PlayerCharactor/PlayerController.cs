using System;
using UnityEngine;
using System.Collections;

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

        public bool _canAttack; //攻撃判定
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
        //AudioSource
        public RandomAudioPlayer _footstepPlayer;
        public RandomAudioPlayer _hurtAudioPlayer;
        public RandomAudioPlayer _landingPlayer;
        //パラメーター
        private readonly int _hashWeaponAttack = Animator.StringToHash("WeaponAttack");
        private readonly int _hashStateTime = Animator.StringToHash("");
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
    }
}

