using UnityEngine;

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
        public CameraSettings _cameraSettings;
        public float _maxTurnSpeed = 1200f; //静止しているときの回転
        public float _minTurnSpeed = 400f; //値が高いほど速く回転する
        public float _idleTimeout = 5f;
        private bool _inAttack; //攻撃中かどうか
        private bool _inCombo; //連続攻撃をしているかどうか
        private Material _currentWalkingSurface; //オーディオに関する決定を行うための参照
        
        public Weapon _weapon;
        private Animator _animator;
        private CharacterController _charCtrl;
        private Damageable _damageable;
        private Renderer[] _renderers;
        private bool _previouslyGrounded;
        private CharMove _charMove;
        //アニメータコントローラーの現在の状態や進行状況
        private AnimatorStateInfo _previousCurrentStateInfo;
        private AnimatorStateInfo _currentStateInfo;
        private AnimatorStateInfo _nextStateInfo;
        private bool _isAnimatorTransitioning; //トランジション中かどうか
        private float _desiredForwardSpeed; //地面を移動する速さ
        private bool _isGrounded = true; //現在地面に立っているか
        private bool _readyToJump; //jumpできる状態かどうか
        private float _verticalSpeed; //現在の上昇、下降の速さ
        private float _forwardSpeed; //現在のspeed
        private Collider[] _overlapResult = new Collider[8]; //playerに近いコライダーをキャッシュ(検出)するのに使用
        private float _angleDiff; //playerの現在の回転と目標回転の間の角度（度）。
        private Quaternion _targetRotation;
        private float _idleTimer; //特定の状態にある間に、一定の期間が経過するまでの時間を追跡する
        //AudioSource
        public RandomAudioPlayer _footstepPlayer;
        public RandomAudioPlayer _hurtAudioPlayer;
        public RandomAudioPlayer _landingPlayer;
        public RandomAudioPlayer _emoteJumpPlayer;

        private const float _stickingGravityProportion = 0.3f; //地面に接しているときの重力
        private const float _groundAcceleration = 20f; //地上での加速
        private const float _groundDeceleration = 25f;　//地上での減速
        private const float _jumpAbortSpeed = 10f;
        private const float _minEnemyDot = 0.2f;　//内積の一定の閾値
        private const float _inverseOneEighty = 1f / 180f; //角度を正規化するときに使用
        private const float _airborneTurnSpeedProportion = 5.4f; //地上での回転速度を基準にして空中での回転速度を調整する際に使用
        
        //パラメーター
        private readonly int _hashWeaponAttack = Animator.StringToHash("WeaponAttack");
        private readonly int _hashStateTime = Animator.StringToHash("");
        private readonly int _hashForwardSpeed = Animator.StringToHash("");
        private readonly int _hashAngleDeltaRad = Animator.StringToHash("");
        private readonly int _hashFootFall = Animator.StringToHash("");
        private readonly int _hashHurt = Animator.StringToHash("");
        private readonly int _hashDeath = Animator.StringToHash("");
        private readonly int _hashTimeoutToIdle = Animator.StringToHash("");
        private readonly int _hashInputDetected = Animator.StringToHash("");
        //State
        private readonly int _hashCombo1 = Animator.StringToHash("");
        private readonly int _hashCombo2 = Animator.StringToHash("");
        private readonly int _hashCombo3 = Animator.StringToHash("");
        private readonly int _hashCombo4 = Animator.StringToHash("");
        private readonly int _hashAirborne = Animator.StringToHash("");
        private readonly int _hashLocomotion = Animator.StringToHash("");
        private readonly int _hashLanding = Animator.StringToHash("");
        //Tag
        private readonly int _hashBlockInput = Animator.StringToHash("BlockInput");
        
        /// <summary>
        /// playerが移動入力を行っているか
        /// </summary>
        private bool IsMoveInput => !Mathf.Approximately(_charMove.moveInput.sqrMagnitude, 0f);
        
        /// <summary>
        /// scriptが初期化されるときに自動で呼び出される
        /// </summary>
        private void Reset()
        {
            _weapon = GetComponent<Weapon>();
            //指定された名前の子を探す
            var footStepSource = transform.Find("");
            //見つかった場合は参照を取得する
            if (footStepSource != null)
                _footstepPlayer = footStepSource.GetComponent<RandomAudioPlayer>();
            
            var hurtSource = transform.Find("");
            if (hurtSource != null)
                _hurtAudioPlayer = hurtSource.GetComponent<RandomAudioPlayer>();
            
            var landingSource = transform.Find("");
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
        /// <summary>
        /// scriptが有効になった時自動で呼び出される
        /// </summary>
        private void OnEnable()
        {
            _damageable = GetComponent<Damageable>();
            _damageable._isInvincible = true;
            EquipWeapon(false);
            _renderers = GetComponentsInChildren<Renderer>();
        }
        /// <summary>
        /// scriptが無効になった時に自動で呼び出される
        /// </summary>
        private void OnDisable()
        {
            foreach (var t in _renderers)
            {
                //objectが無効になるとrendererも無効になってしまうため呼び出さなければならない
                t.enabled = true;
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
            SetTargetRotation();
            if(IsOrientationUpdated() && IsMoveInput)
                UpdateOrientation();
            PlayAudio();
            TimeToIdle();
            _previouslyGrounded = _isGrounded;
        }

        /// <summary>
        /// Animatorの状態をキャッシュする
        /// </summary>
        private void CacheAnimatorState()
        {
            _previousCurrentStateInfo = _currentStateInfo;
            _currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _nextStateInfo = _animator.GetNextAnimatorStateInfo(0);
            _isAnimatorTransitioning = _animator.IsInTransition(0);
        }
        /// <summary>
        /// //アニメーターの状態がキャッシュされた後に呼び出され、このスクリプトがユーザー入力をブロックすべきかどうかを決定する
        /// </summary>
        private void UpdateInputBlocking()
        {
            var inputBlocked = _currentStateInfo.tagHash == _hashBlockInput && !_isAnimatorTransitioning;
            inputBlocked |= _nextStateInfo.tagHash == _hashBlockInput;
            _charMove._playerCtrlInputBlocked = inputBlocked;
        }

        /// <summary>
        /// Playerが武器のコンボを再生しているかどうか
        /// </summary>
        /// <returns></returns>
        private bool IsWeaponEquip()
        {
            var equipped = _nextStateInfo.shortNameHash == _hashCombo1 ||
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
        /// 武器の装備状態を制御
        /// </summary>
        /// <param name="equip"></param>
        private void EquipWeapon(bool equip)
        {
            _weapon.gameObject.SetActive(equip);
            _inAttack = false;　//装備解除時にAttackをfalseにする
            _inCombo = equip;　

            if (!equip)
            {
                _animator.ResetTrigger(_hashWeaponAttack); //装備解除時にTriggerをreset
            }
        }
        /// <summary>
        /// playerの前方向を計算しAnimationを制御するparamを設定
        /// </summary>
        private void CalculateForwardMovement()
        {
            var moveInput = _charMove.moveInput; //移動入力をキャッシュし代入
            
            if(moveInput.sqrMagnitude > 1f) // sqrMagnitudeはベクトルの大きさの2乗を返すプロパティ
                moveInput.Normalize();　//ベクトルの大きさを1に制限
            
            _desiredForwardSpeed = moveInput.magnitude * _maxForwardSpeed; //移動入力の大きさにmaxSpeedを掛けて目標速度を計算
            var acceleration = IsMoveInput ? _groundAcceleration : _groundDeceleration; //現在の移動入力で速度の変化を決定する　
            _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, _desiredForwardSpeed, 
                acceleration * Time.deltaTime); //目標速度に向かって加速or減速
            _animator.SetFloat(_hashForwardSpeed,_forwardSpeed); 
        }

        /// <summary>
        /// Jumpの計算
        /// </summary>
        private void CalculateVerticalMovement()
        {
            if (!_charMove.JumpInput && _isGrounded)
                _readyToJump = true;　//Jump可能
            
            if (_isGrounded)
            {
                _verticalSpeed = -_gravity * _stickingGravityProportion;　//接地時には地面に密着させるためにわずかにマイナスの垂直スピードを加える
                
                //jumpがfalseではない時jumpの準備ができており現在はAttack中ではない
                if (_charMove.JumpInput && _readyToJump && !_inCombo)
                {
                    _verticalSpeed = _jumpSpeed;　//JumpSpeedの設定
                    _isGrounded = false;
                    _readyToJump = false;　//2段Jump不可
                }
            }
            else
            {
                //JumpButtonが離された場合
                if (!_charMove.JumpInput && _verticalSpeed > 0.0f)
                    _verticalSpeed -= _jumpAbortSpeed * Time.deltaTime;　//Jumpの頂点に達したあと、上方向の速度を徐々に減少させる
                
                if (Mathf.Approximately(_verticalSpeed, 0f))　//2つの浮動小数点数値がほぼ等しいかどうかを判定
                    _verticalSpeed = 0f;　//jumpの高さを制御
                
                _verticalSpeed -= _gravity * Time.deltaTime;　//空中にいるときの重力
            }
        }
        /// <summary>
        /// 向きや回転を制御する
        /// </summary>
        private void SetTargetRotation()
        {
            // 移動方向、カメラ方向からforwardベクトル、回転の3つの変数を作成する
            var moveInput = _charMove.moveInput;
            var localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            var forward = Quaternion.Euler(0f, _cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();
            Quaternion targetRotation;
            //localMovementDirectionがplayerの逆向きならplayerをカメラの向きに向ける
            if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -10f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                //playerの移動方向に基づいて回転を計算
                var cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }

            var resultingForward = targetRotation * Vector3.forward; //プレイヤーが向いている方向を示すベクトル
            //Attack中なら周囲の最も近い敵に向けて回転
            if (_inAttack)
            {
                //ローカルエリア内の全ての敵を見つける
                var centre = transform.position + transform.forward * 2.0f + transform.up;
                var halfExtents = new Vector3(3.0f, 1.0f, 2.0f); //boxを定義
                var layerMask = 1 << LayerMask.NameToLayer(""); 
                var count = Physics.OverlapBoxNonAlloc(centre, halfExtents, 
                    _overlapResult, targetRotation, layerMask);　//指定されたbox内のcolを配列に格納

                var closestDot = 0.0f;　//最も近い敵との方向の内積を格納
                var closestForward = Vector3.zero;　//最も近い敵との方向を格納
                var closest = -1; //最も近い敵のindexを保持

                for (var i = 0; i < count; i++) //周囲の敵を処理
                {
                    //playerから各enemyへの方向ベクトルを計算
                    var playerToEnemy = _overlapResult[i].transform.position - transform.position;
                    playerToEnemy.y = 0;
                    playerToEnemy.Normalize();
                    // playerが進みたい方向とenemyへの方向の内積を計算。2つのベクトルがどれだけ同じ方向を向いているか
                    var d = Vector3.Dot(resultingForward, playerToEnemy);
                    
                    if (d > _minEnemyDot && d > closestDot)　// 一番近い敵のところに保持
                    {
                        closestForward = playerToEnemy;
                        closestDot = d;
                        closest = i;
                    }
                }
                
                // 戦闘中は向きの更新がUpdateOrientation関数で行われないため、回転を直接設定
                // 戦闘時には敵に対して素早く回転する必要があるため、向きを素早く調整するために直接回転を設定
                if (closest != -1)　//もし近くに敵がいたら
                {
                    //最も近い敵の方向をplayerの目標方向にする
                    resultingForward = closestForward;　
                    transform.rotation = Quaternion.LookRotation(resultingForward);
                }
            }
            //プレイヤーの現在の回転と望ましい回転の間の角度の差を計算する
            var angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            var targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;
            _angleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            
            _targetRotation = targetRotation;
        }
        /// <summary>
        /// playerが回転できるかどうかを判定するために毎フレーム呼び出される
        /// </summary>
        /// <returns></returns>
        private bool IsOrientationUpdated()
        {
            var updateOrientationForLocomotion = _isAnimatorTransitioning && _currentStateInfo.shortNameHash
                == _hashLocomotion || _nextStateInfo.shortNameHash == _hashLocomotion;
            var updateOrientationForAirborne = _isAnimatorTransitioning && _currentStateInfo.shortNameHash
                == _hashAirborne || _nextStateInfo.shortNameHash == _hashAirborne;
            var updateOrientationForLanding = _isAnimatorTransitioning && _currentStateInfo.shortNameHash
                == _hashLanding || _nextStateInfo.shortNameHash == _hashLanding;

            //移動中、空中にいる間、着陸中、または攻撃中にプレイヤーの向きを更新する必要があるか
            return updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding
                   || _inCombo && _inAttack;
        }
        /// <summary>
        /// playerの向きを更新する
        /// </summary>
        private void UpdateOrientation()
        {
            _animator.SetFloat(_hashAngleDeltaRad,_angleDiff * Mathf.Deg2Rad);
            var localInput = new Vector3(_charMove.moveInput.x, 0f, _charMove.moveInput.y);
            var groundedTurnSpeed = Mathf.Lerp(_maxTurnSpeed, _minTurnSpeed, _forwardSpeed / _desiredForwardSpeed);
            //回転速度
            var actualTurnSpeed = _isGrounded
                ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * 
                                      _inverseOneEighty * _airborneTurnSpeedProportion * groundedTurnSpeed;
            _targetRotation = Quaternion.RotateTowards(transform.rotation,
                _targetRotation, actualTurnSpeed * Time.deltaTime);
            transform.rotation = _targetRotation;
        }
        /// <summary>
        /// playerの行動に応じてaudioを再生
        /// </summary>
        private void PlayAudio()
        {
            var footfallCurve = _animator.GetFloat(_hashFootFall);

            if (footfallCurve > 0.01f && !_footstepPlayer._playing && _footstepPlayer._canPlay)
            {
                //足音を再生
                _footstepPlayer._playing = true;
                _footstepPlayer._canPlay = false;
                _footstepPlayer.PlayRandomClip(_currentWalkingSurface, _forwardSpeed < 4 ? 0 : 1);
            }
            else if (_footstepPlayer._playing)
            {
                _footstepPlayer._playing = false;　//再生中なら停止
            }
            else if (footfallCurve < 0.01f && !_footstepPlayer._canPlay)
            {
                _footstepPlayer._canPlay = true;　//条件を満たしていれば再生可能にする
            }

            if (_isGrounded && _previouslyGrounded)
            {
                _landingPlayer.PlayRandomClip(_currentWalkingSurface, bankId: _forwardSpeed < 4 ? 0 : 1);
                _emoteJumpPlayer.PlayRandomClip();　//着地音の再生
            }

            if (_currentStateInfo.shortNameHash == _hashHurt &&
                _previousCurrentStateInfo.shortNameHash != _hashHurt)
            {
                _hurtAudioPlayer.PlayRandomClip();　//damage音を再生
            }

            if (_currentStateInfo.shortNameHash == _hashDeath &&
                _previousCurrentStateInfo.shortNameHash != _hashDeath)
            {
                _emoteJumpPlayer.PlayRandomClip();　//死亡音を再生
            }

            //combo中の特定の音を再生
            if (_currentStateInfo.shortNameHash == _hashCombo1 &&
                _previousCurrentStateInfo.shortNameHash != _hashCombo1 ||
                _currentStateInfo.shortNameHash == _hashCombo2 &&
                _previousCurrentStateInfo.shortNameHash != _hashCombo2 ||
                _currentStateInfo.shortNameHash == _hashCombo3 &&
                _previousCurrentStateInfo.shortNameHash != _hashCombo3 ||
                _currentStateInfo.shortNameHash == _hashCombo4 &&
                _previousCurrentStateInfo.shortNameHash != _hashCombo4)
            {
                _emoteJumpPlayer.PlayRandomClip();
            }
        }

        /// <summary>
        /// playerが一定時間何も操作しなかった場合にアイドル状態に遷移
        /// </summary>
        private void TimeToIdle()
        {
            var inputDetected = IsMoveInput || _charMove.Attack || _charMove.JumpInput;
            if (_isGrounded && inputDetected)
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= _idleTimeout)
                {
                    _idleTimer = 0f;
                    _animator.SetTrigger(_hashTimeoutToIdle);
                }
            }
            else
            {
                _idleTimer = 0f;
                _animator.ResetTrigger(_hashTimeoutToIdle);
            }
            _animator.SetBool(_hashInputDetected,inputDetected);
        }
    }
}