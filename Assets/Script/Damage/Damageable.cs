using System;
using UnityEngine;
using UnityEngine.Events;

namespace TuningTraveler
{
    public partial class Damageable : MonoBehaviour
    {
        [Tooltip("ダメージを受けた後無敵になる時間")] public float _invincibleTime;
        public bool _isInvincible { get; set; }

        [Tooltip("ダメージを受ける角度")] [Range(0.0f, 360.0f)]
        public float _hitAngle = 360.0f;

        [Tooltip("オブジェクトが回転したらダメージを受ける範囲も回転")] [Range(0.0f, 360.0f)]
        public float _hitForwardRotation = 360.0f;

        public int _maxHp;

        public int _currentHp { get; private set; } //現在のHP

        //死んだ、ダメージを受けた、無敵の間にダメージを受けた、無敵状態になった、ダメージをリセット
        public UnityEvent OnDeath, TakeDamage, OnHitWhileInvincible, OnBecomeInvincible, OnResetDamage;

        protected float _lastHitTime; //最後に攻撃を受けてからの時間
        protected Collider _col;

        private System.Action _schedule;


        void Start()
        {
            ResetDamage();
            _col = GetComponent<Collider>();
        }

        void Update()
        {
            if (_isInvincible)
            {
                _lastHitTime += Time.deltaTime;
                //最後にダメージを受けてから_invincibleTimeを過ぎたなら
                if (_lastHitTime > _invincibleTime)
                {
                    _lastHitTime = 0.0f;
                    _isInvincible = false;
                    OnBecomeInvincible.Invoke();
                }
            }
        }

        public void ResetDamage()
        {
            _currentHp = _maxHp;
            _isInvincible = false;
            _lastHitTime = 0.0f;
            OnResetDamage.Invoke();
        }

        /// <summary>
        /// trueならコライダーを有効にする
        /// </summary>
        /// <param name="isEnabled"></param>
        public void SetColliderState(bool isEnabled)
        {
            _col.enabled = isEnabled;
        }

        /// <summary>
        /// すでに死んでいる場合はダメージを無視
        /// </summary>
        /// <param name="data"></param>
        public void ApplyDamage(DamageMessage data)
        {
            if (_currentHp <= 0)
            {
                return;
            }

            if (_isInvincible)
            {
                OnHitWhileInvincible.Invoke();
                return;
            }

            Vector3 forward = transform.forward;
            // transform.upを中心に_hitForwardRotation度回転する
            forward = Quaternion.AngleAxis(_hitForwardRotation, transform.up) * forward;

            //ダメージを与える側から受ける側への方向
            Vector3 _positionToDamage = data._damageSource - transform.position;
            //上方向に平行な成分の除去、ダメージを与えた方向に対する反応を行うため
            _positionToDamage -= transform.up * Vector3.Dot(transform.up, _positionToDamage);

            if (Vector3.Angle(forward, _positionToDamage) > _hitAngle * 0.5)
            {
                return;
            }

            _isInvincible = true;
            _currentHp -= data._amount;
            //デリゲートactionに登録、競合の回避
            //変更可能性?
            _schedule += _currentHp <= 0 ? OnDeath.Invoke : (Action)TakeDamage.Invoke;
        }

        /// <summary>
        /// 全てのUpdateが呼ばれた後に呼び出される
        /// </summary>
        private void LateUpdate()
        {
            if (_schedule != null)
            {
                //scheduleが参照するデリゲートを呼び出す。
                _schedule();
                _schedule = null;
            }
        }
    }
}