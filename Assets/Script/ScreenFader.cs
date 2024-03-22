using System.Collections;
using UnityEngine;

namespace TuningTraveler
{
    public class ScreenFader : MonoBehaviour
    {
        public enum FadeType
        {
            Black,
            Loading,
            GameOver
        }
        
        private static ScreenFader Instance　 // シングルトン化
        {
            get
            {
                if (_instance != null)
                    return _instance;　//すでにinstanceが存在していればそれを返す

                _instance = FindObjectOfType<ScreenFader>();　//scene内全てから指定されたinstanceを探す。

                if (_instance != null)
                    return _instance;　
                Create(); //FindObjectOfTypeがnullを返したらCreateを呼び出す
                return _instance;　//instanceを返してprogram内のどこからでも唯一のinstanceにアクセス可能
            }
        }

        public static bool IsFading => Instance._isFading;
        private static ScreenFader _instance;

        /// <summary>
        /// screenFaderクラスの新しいinstanceを生成
        /// </summary>
        private static void Create()
        {
            var ctrlPrefab = Resources.Load<ScreenFader>("");
            _instance = Instantiate(ctrlPrefab);
        }

        public CanvasGroup _faderCanvasGroup;
        public CanvasGroup _loadingCanvasGroup;
        public CanvasGroup _gameOverCanvasGroup;
        public float _fadeDuration = 1f;　//fadeする時間

        private bool _isFading;　//fade中かどうか
        private const int _maxSortingLayer = 32767;

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);　//objectが破棄されない
        }

        /// <summary>
        /// alpha値を変更してfadeInやfadeOutを行う
        /// </summary>
        /// <param name="finalAlpha"></param>
        /// <param name="canvasGroup"></param>
        /// <returns></returns>
        private IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
        {
            _isFading = true;
            canvasGroup.blocksRaycasts = true;　　//userの操作がcanvasGroupに影響を与えない
            //現在のalpha値から目標のalpha値を引いてfadeする時間を割る
            var fadeSpeed = Mathf.Abs(canvasGroup.alpha - finalAlpha) / _fadeDuration;
            
            while (!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
            {
                //目標のalpha値になるまで現在のalpha値を変化させる
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha,
                    fadeSpeed * Time.deltaTime);
                yield return null;
            }

            canvasGroup.alpha = finalAlpha;
            _isFading = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static void SetAlpha(float alpha)
        {
            Instance._faderCanvasGroup.alpha = alpha;
        }

        public static IEnumerator FadeSceneIn()
        {
            CanvasGroup canvasGroup;
            if (Instance._faderCanvasGroup.alpha > 0.1f)
                canvasGroup = Instance._faderCanvasGroup;
            else if (Instance._gameOverCanvasGroup.alpha > 0.1)
                canvasGroup = Instance._gameOverCanvasGroup;
            else
                canvasGroup = Instance._loadingCanvasGroup;

            yield return Instance.StartCoroutine(Instance.Fade(0f, canvasGroup));
            canvasGroup.gameObject.SetActive(false);
        }

        public static IEnumerator FadeSceneOut(FadeType fadeType = FadeType.Black)
        {
            CanvasGroup canvasGroup;
            switch (fadeType)
            {
                case FadeType.Black:
                    canvasGroup = Instance._faderCanvasGroup;
                    break;
                case FadeType.GameOver:
                    canvasGroup = Instance._gameOverCanvasGroup;
                    break;
                default:
                    canvasGroup = Instance._loadingCanvasGroup;
                    break;
            }
            canvasGroup.gameObject.SetActive(true);
            yield return Instance.StartCoroutine(Instance.Fade(1f, canvasGroup));
        }
    }
}