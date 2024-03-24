using System;
using UnityEngine;

namespace TuningTraveler
{
    public class PlayerSpawn : MonoBehaviour
    {
        [HideInInspector]
        public float _effectTime;
        public Material[] _playerRespawnMaterials;
        public GameObject _respawnParticles;
        private Material[] _playerMaterials;
        private MaterialPropertyBlock _propertyBlock; //複数のobjectのmaterialを効率的に設定
        private Renderer _renderer;
        private Vector4 _pos;
        private Vector3 _renderBounds;

        private const string _boundsName = "_bounds";
        private const string _cutoffName = "_cutoff";
        private float _timer;
        private float _endTime;

        private bool _started = false;
        private void Awake()
        {
            _respawnParticles.SetActive(false);
            _propertyBlock = new MaterialPropertyBlock();
            _renderer = GetComponentInChildren<Renderer>();
            _playerMaterials = _renderer.materials;
            _renderBounds = _renderer.bounds.size;
            _pos.y = _renderBounds.y;
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(_boundsName,_pos);
            _propertyBlock.SetFloat(_cutoffName,0.0001f);
            _renderer.SetPropertyBlock(_propertyBlock);
            _pos = new Vector4(0, 0, 0, 0);
            _started = false;
            this.enabled = false;
        }

        private void OnEnable()
        {
            _started = false;
            _renderer.materials = _playerRespawnMaterials;
            Set(0.001f);
            _renderer.enabled = false;
        }

        public void StartEffect()
        {
            _renderer.enabled = true;
            _respawnParticles.SetActive(true);
            _started = true;
            _timer = 0.0f;
        }

        private void Update()
        {
            if(!_started)
                return;
            //カットオフ値は特定の値以下の色や効果を描画するかどうかを制御するために使用
            var cutoff = Mathf.Clamp(_timer / _effectTime, 0.01f, 1.0f);
            Set(cutoff);
            _timer += Time.deltaTime;
            if (cutoff >= 1.0f)
            {
                _renderer.materials = _playerMaterials;
                this.enabled = false;
            }
        }

        private void Set(float cutoff)
        {
            _renderBounds = _renderer.bounds.size;
            _pos.y = _renderBounds.y;
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(_boundsName,_pos);
            _propertyBlock.SetFloat(_cutoffName,cutoff);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
