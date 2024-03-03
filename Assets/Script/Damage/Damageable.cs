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

        public void ApplyDamage()
        {
            
        }
    }
}