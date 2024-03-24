using UnityEngine;

namespace TuningTraveler
{
    /// <summary>武器にアタッチ</summary>
    public class Weapon : MonoBehaviour
    {
        /// <summary>
        /// 攻撃範囲の内部クラス
        /// </summary>
        [System.Serializable]
        public class AttackPoint
        {
            public float _radius;　//半径
            public Vector3 _offset; //相対位置
            public Transform _attackRoot;　//攻撃の起点
        }

        public int _damage = 1;
        
        [Header("Audio")]
        public RandomAudioPlayer _hitAudio;
        public RandomAudioPlayer _attackAudio; 
        
        public LayerMask _targetLayers;
        //両手で攻撃がしたいときそれぞれの手に対応する攻撃範囲が必要になるので
        //配列にすることで必要に応じて複数の攻撃範囲を追加、削除が可能
        public AttackPoint[] _attackPoints = new AttackPoint[0];
        public TimeEffect[] _effects;
       
        //particle
        public ParticleSystem _particlePrefab;
        private ParticleSystem[] _particlePool = new ParticleSystem[PARTICLE_COUNT];
        private const int PARTICLE_COUNT = 10;
        private int _currentParticle = 0;
        
        private GameObject _owner;
        private Vector3[] _previousPos = null;
        private Vector3 _direction;
        
        //Raycastのキャッシュを作成することでRaycastHitを新しく生成する必要がなくなる
        private static RaycastHit[] _raycastHitCahe = new RaycastHit[32];
        
        private bool _isThrowingHit = false;

        public bool _throwingHit { get; set ; }
        public bool _killHit { get; set; }
        protected bool _inAttack　= false;　//attack中かどうか

        public void Awake()
        {
            //Particleをインスタンス化し、プール内に生成
            if (_particlePrefab != null)
            {
                for (int i = 0; i < PARTICLE_COUNT; ++i)
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
        /// 攻撃が始まると音声を再生し攻撃範囲の位置を計算し前の位置を保存。
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

        /// <summary>
        /// 攻撃の終わり。攻撃フラグのリセット
        /// </summary>
        public void EndAttack()
        {
            _inAttack = false;
        }

        /// <summary>
        /// 攻撃の位置を計算して、SphereCastを行って攻撃範囲内の対象を検出
        /// </summary>
        public void FixedUpdate()
        {
            if (_inAttack)
            {
                for (var i = 0; i < _attackPoints.Length; i++)
                {
                    var pts = _attackPoints[i];

                    var worldPos = pts._attackRoot.position + pts._attackRoot.TransformVector(pts._offset);
                    var attackVector = worldPos - _previousPos[i];

                    //攻撃ベクトルが非常に小さかった場合、計算上のerrorが起こる可能性があるので
                    //変わりの値を設定しておくことで計算の安定性を確保
                    if (attackVector.magnitude < 0.001f)
                    {
                        attackVector = Vector3.forward * 0.0001f;
                    }
                    //Rayの発射点は攻撃範囲の発生点からattackVector方向に伸びるRay
                    var r = new Ray(worldPos, attackVector.normalized);

                    //第１引数.Ray,2.球体の半径,3.衝突判定の格納先,4.キャストの最大距離、5.衝突を検出するレイヤーマスク
                    var contacts = Physics.SphereCastNonAlloc(r, pts._radius, _raycastHitCahe, attackVector.magnitude,
                        ~0,
                        //Trigger,Colliderの衝突を無視
                        QueryTriggerInteraction.Ignore);
                }
            }
        }

        /// <summary>
        /// ダメージを与える。hitしたらParticleを再生
        /// </summary>
        /// <param name="other"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        private bool CheckDamage(Collider other,AttackPoint pts)
        {
            //ダメージを受けることのできるオブジェクトを検索するためにコライダーから検索
            var d = other.GetComponent<Damageable>();

            if (d == null)
            {
                //存在しない場合はダメージを受けられないオブジェクトなのでfalseを返す
                return false;
            }

            if (d.gameObject == _owner)
            {
                //自傷しても攻撃が中断されない
                return true;
            }

            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                //攻撃対象のlayerではない場合、攻撃が終了する。アクションが起こる
                return false;
            }

            //hitしたオブジェクトにrendererがある場合、そのマテリアルに応じた音を再生
            if (_hitAudio != null)
            {
                var _renderer = other.GetComponent<Renderer>();
                if (!_renderer)
                {
                    _renderer = other.GetComponentInChildren<Renderer>();
                }

                if (_renderer)
                {
                    _hitAudio.PlayRandomClip(_renderer.sharedMaterial);
                }
                //見つからない場合はランダムな音を再生
                else
                {
                    _hitAudio.PlayRandomClip();
                }
            }

            Damageable.DamageMessage data;

            data._amount = _damage;
            data._damager = this;
            data._direction = _direction.normalized;
            data._damageSource = _owner.transform.position;
            data._throwing = _isThrowingHit;
            data._stopCamera = false;
            
            d.ApplyDamage(data);

            if (_particlePrefab != null)
            {
                //ptsの位置にparticleを配置
                _particlePool[_currentParticle].transform.position = pts._attackRoot.transform.position;
                //再生時間のリセット
                _particlePool[_currentParticle].time = 0;
                _particlePool[_currentParticle].Play();
                //particleの総数で割ってインデックス内のparticleをループ
                _currentParticle = (_currentParticle + 1) % PARTICLE_COUNT;
            }

            return true;
        }
    }
}