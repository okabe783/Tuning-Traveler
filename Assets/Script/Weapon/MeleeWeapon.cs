using UnityEngine;

namespace TuningTraveler
{
    /// <summary>武器にアタッチ</summary>
    public class MeleeWeapon : MonoBehaviour
    {
        /// <summary>
        /// 攻撃範囲
        /// </summary>
        [System.Serializable]
        public class AttackPoint
        {
            public float _radius;
            public Vector3 _offset; //相対位置
            public Transform _attackRoot;
        }

        public int _damage = 1;
        public ParticleSystem _particlePrefab;
        public RandomAudioPlayer _attackAudio; //攻撃用サウンド
        public bool _isThrowingHit = false;
        //両手で攻撃がしたいときそれぞれの手に対応する攻撃範囲が必要になるので
        //配列にすることで必要に応じて複数の攻撃範囲を追加、削除が可能
        public AttackPoint[] _attackPoints = new AttackPoint[0];

        private const int _particleCount = 10;

        protected GameObject _owner;
        protected ParticleSystem[] _particlePool = new ParticleSystem[_particleCount];
        protected bool _inAttack　= false;　//attack中かどうか
        protected Vector3[] _previousPos = null;
        //Raycastのキャッシュを作成することでRaycastHitを新しく生成する必要がなくなる
        protected static RaycastHit[] _raycastHitCahe = new RaycastHit[32];

        public bool _killHit { get; set; }

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

        /// <summary>
        /// 攻撃開始
        /// </summary>
        /// <param name="killAttack"></param>
        public void BeginAttack(bool killAttack)
        {
            //Soundを再生
            if (_attackAudio != null)
            {
                _attackAudio.PlayRandomClip();
                _killHit = killAttack; //攻撃が敵を倒すかどうかが設定
                _inAttack = true;
                //前の位置と現在の位置の差を使用して適切な動きを行う
                _previousPos = new Vector3[_attackPoints.Length]; //攻撃ポイントの前の位置を保存

                for (int i = 0; i < _attackPoints.Length; i++)
                {
                    //攻撃範囲の親の位置を取得。相対的に配置される基準
                    Vector3 worldPos = _attackPoints[i]._attackRoot.position +
                                       //親のローカル座標からワールド座標にして攻撃範囲を計算
                                       _attackPoints[i]._attackRoot.TransformVector(_attackPoints[i]._offset);
                    _previousPos[i] = worldPos;
                }
            }
        }

        public void EndAttack()
        {
            _inAttack = false;
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < _attackPoints.Length; i++)
            {
                AttackPoint pts = _attackPoints[i];

                Vector3 worldPos = pts._attackRoot.position + pts._attackRoot.TransformVector(pts._offset);
                Vector3 attackVector = worldPos - _previousPos[i];

                //攻撃ベクトルが非常に小さかった場合、計算上のerrorが起こる可能性があるので
                //変わりの値を設定しておくことで計算の安定性を確保
                if (attackVector.magnitude < 0.001f)
                {
                    attackVector = Vector3.forward * 0.0001f;
                }
                //Rayの発射点は攻撃範囲の発生点からattackVector方向に伸びるRay
                var r = new Ray(worldPos, attackVector.normalized);

                //第１引数.Ray,2.球体の半径,3.衝突判定の格納先,4.キャストの最大距離、5.衝突を検出するレイヤーマスク
                int contacts = Physics.SphereCastNonAlloc(r, pts._radius, _raycastHitCahe, attackVector.magnitude,
                    ~0,
                    //Trigger,Colliderの衝突を無視
                    QueryTriggerInteraction.Ignore);
            }
        }

        private bool CheckDamage(Collider other,AttackPoint pts)
        {
            return true;
        }
    }
}