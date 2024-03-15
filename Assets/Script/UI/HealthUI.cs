using System.Collections;
using UnityEngine;

namespace TuningTraveler
{
    public class HealthUI : MonoBehaviour
    {
        public Damageable _damageable;
        public GameObject _healthIconPrefab;

        private Animator[] _healthIconAnimators;

        private readonly int _hashActiveParam = Animator.StringToHash("");
        private readonly int _hashInactiveState = Animator.StringToHash("");
        private const float _hearthIconWidth = 0.041f;
        private IEnumerator Start()
        {
            if(_damageable == null)
                yield break;
            yield return null;
            _healthIconAnimators = new Animator[_damageable._maxHp];

            for (var i = 0; i < _damageable._maxHp; i++)
            {
                var healthIcon = Instantiate(_healthIconPrefab);
                healthIcon.transform.SetParent(transform);
                var healthIconRect = healthIcon.transform as RectTransform; //UIを制御するためのTransform
                healthIconRect.anchoredPosition = Vector2.zero;　//位置を0にする
                healthIconRect.sizeDelta = Vector2.zero;　//sizeを0にする
                //Iconが水平方向に均等に配置される
                healthIconRect.anchorMin += new Vector2(_hearthIconWidth, 0f) * i;　
                healthIconRect.anchorMax += new Vector2(_hearthIconWidth, 0f) * i;　
                
                _healthIconAnimators[i] = healthIcon.GetComponent<Animator>();

                if (_damageable._currentHp < i + 1)
                {
                    //Damageを受けたときハートを一つ非アクティブにする
                    _healthIconAnimators[i].Play(_hashInactiveState);
                    _healthIconAnimators[i].SetBool(_hashActiveParam,false);
                }
            }
        }

        /// <summary>
        /// Damageableから渡されたHpを使用してIconを表示する
        /// </summary>
        /// <param name="damageable"></param>
        public void ChangeHpUI(Damageable damageable)
        {
            if(_healthIconAnimators == null)
                return;

            for (var i = 0; i < _healthIconAnimators.Length; i++)
            {
                _healthIconAnimators[i].SetBool(_hashActiveParam,damageable._currentHp >= i + 1);
            }
        }
    }

}
