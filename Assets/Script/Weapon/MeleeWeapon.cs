using UnityEngine;

namespace TuningTraveler
{
    /// <summary>武器にアタッチ</summary>
    public class MeleeWeapon : MonoBehaviour
    {
        [System.Serializable]
        public class AttackPoint
        {
            public float _radius;
            public Vector3 _offset;
            public Transform _attackRoot;
        }
        public int _damage = 1;
        public ParticleSystem _particlePrefab;
        public RandomAudioPlayer _attackAudio;
        public bool _isThrowingHit = false;
        public AttackPoint[] _attackPoints = new AttackPoint[0];

        private const int _particleCount = 10;

        protected GameObject _owner;
        protected ParticleSystem[] _particlePool = new ParticleSystem[_particleCount];
        protected bool _inAttack;
        protected Vector3[] _previousPos = null;

        public bool _killHit
        {
            get { return _isThrowingHit; }
            set { _isThrowingHit = value; }
        }
        
        public void Awake()
        {
            //Particleをインスタンス化し、プール内に生成
            if (_particlePrefab != null)
            {
                for (int i = 0; i < _particleCount; ++i)
                {
                    _particlePool[i] = Instantiate(_particlePrefab);
                    _particlePool[i].Stop();
                }
            }
        }
        /// <summary>
        /// 自傷しないようにする
        /// </summary>
        /// <param name="owner"></param>
        public void SetOwner(GameObject owner)
        {
            _owner = owner;
        }

        public void BeginAttack(bool killAttack)
        {
            if (_attackAudio != null)
            {
                _attackAudio.PlayRandomClip();
                _killHit = killAttack;
                _inAttack = true;
                _previousPos = new Vector3[_attackPoints.Length];
                for (int i = 0; i < _attackPoints.Length; i++)
                {
                    Vector3 worldPos = _attackPoints[i]._attackRoot.position +
                                       _attackPoints[i]._attackRoot.TransformVector(_attackPoints[i]._offset);
                    _previousPos[i] = worldPos;
                }
            }
        }
    }
}
