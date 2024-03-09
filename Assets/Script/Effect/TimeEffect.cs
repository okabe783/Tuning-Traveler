using System.Collections;
using UnityEngine;

namespace TuningTraveler
{
    /// <summary>
    /// オブジェクトがアクティブになった時アニメーションを再生。再生が終了したら非アクティブにする
    /// </summary>
    public class TimeEffect : MonoBehaviour
    {
        public Light _staffLight;
        private Animation _animation;

        private void Awake()
        {
            _animation = GetComponent<Animation>();
            gameObject.SetActive(false);
        }

        public void Active()
        {
            gameObject.SetActive(true);
            _staffLight.enabled = true;

            if (_animation)
            {
                _animation.Play();
            }

            StartCoroutine(DisableAtEndOfAnimation());
        }

        IEnumerator DisableAtEndOfAnimation()
        {
            //Animationの再生が終わるまで待機
            yield return new WaitForSeconds(_animation.clip.length);
            
            gameObject.SetActive(false);
            _staffLight.enabled = false;
        }
    }
}

