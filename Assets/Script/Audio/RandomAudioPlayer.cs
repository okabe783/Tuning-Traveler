using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TuningTraveler
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioPlayer : MonoBehaviour
    {
        /// <summary>
        ///　Materialに音を関連つけることで地形によって異なる音をだすことができる
        /// </summary>
        //サブプロパティー
        [Serializable]
        public class MaterialOverrideSound
        {
            public Material[] _materials;
            public SoundBank[] _bank;
        }

        public MaterialOverrideSound[] _overrideSound;

        [Serializable]
        public class SoundBank
        {
            public string _name;
            public AudioClip[] _clips;
        } 
        //Editor上に表示させない
        [HideInInspector] public bool _playing;
        [HideInInspector] public bool _canPlay;

    public SoundBank _defaultBank = new SoundBank();
        public AudioClip _clip { get; private set; } //外部から変更不可

        private AudioSource _audioSource;
        protected AudioSource audioSource => _audioSource; //参照のみ可
        
        private Dictionary<Material, SoundBank[]> _soundDic = new Dictionary<Material, SoundBank[]>();
        
        public bool _randomizePitch = true;
        public float _pitchRandomRange = 0.2f;
        public float _playDelay = 0;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            for (var i = 0; i < _overrideSound.Length; i++)
            {
                //辞書に追加
                foreach (var material in _overrideSound[i]._materials)
                {
                    _soundDic[material] = _overrideSound[i]._bank;
                }
            }
        }

        /// <summary>
        /// 割り当てられたリストのクリップをランダムに再生。materialに対するoverrideを探し見つからなければdefaultを再生。
        /// </summary>
        /// <param name="overrideMaterial"></param>
        /// <param name="bankId"></param>
        /// <returns></returns>
        public AudioClip PlayRandomClip(Material overrideMaterial, int bankId = 0)
        {
            if (overrideMaterial == null)
            {
                return null;
            }

            return InternalPlayRandomClip(overrideMaterial, bankId);
        }

        public void PlayRandomClip()
        {
            _clip = InternalPlayRandomClip(null, bankId: 0);
        }

        AudioClip InternalPlayRandomClip(Material overrideMaterial, int bankId)
        {
            //配列宣言と初期化
            SoundBank[] banks = null;
            //Bankの初期値をdefaultに
            var bank = _defaultBank;
            //overrideMaterialがnullでない、soundDicにキーが存在、bankIdがbanks内に収まっていれば
            //TryGetValue　= キーが存在する場合、その値をbanksにセットしtrueを返す。outを使用してメソッド内で格納。
            if (overrideMaterial != null && _soundDic.TryGetValue(overrideMaterial, out banks)
                                         && bankId < banks.Length)
            {
                bank = banks[bankId];
            }

            if (bank._clips == null || bank._clips.Length == 0)
            {
                return null;
            }

            var clip = bank._clips[Random.Range(0, bank._clips.Length)];
            if (clip == null)
            {
                return null;
            }

            //音の高さをrandomに変化させる、Delayをかける
            _audioSource.pitch = _randomizePitch
                ? Random.Range(1.0f - _pitchRandomRange, 1.0f + _pitchRandomRange) : 1.0f;
            _audioSource.clip = _clip;
            _audioSource.PlayDelayed(_playDelay);
            
            return clip;
        }
    }
}