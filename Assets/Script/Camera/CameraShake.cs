using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

namespace TuningTraveler
{
    public class CameraShake : MonoBehaviour
    {
        private static List<CameraShake> _cameras = new List<CameraShake>();

        public const float _playerHitShakeAmount = 0.05f;
        public const float _playerHitShakeTime = 0.4f;

        private float _shakeAmount;
        private float _remainingShakeTime;
        private CinemachineVirtualCameraBase _cinemachineVCam;
        private bool _isShaking = false;
        private Vector3 _originalLocalPosition;

        private void Awake()
        {
            _cinemachineVCam = GetComponent<CinemachineVirtualCameraBase>();
        }

        private void OnEnable()
        {
            _cameras.Add(this);
        }

        private void OnDisable()
        {
            _cameras.Remove(this);
        }

        private void LateUpdate()
        {
            if (_isShaking)
            {
                _cinemachineVCam.LookAt.localPosition = _originalLocalPosition + Random.insideUnitSphere * _shakeAmount;
                _remainingShakeTime -= Time.deltaTime;
                if (_remainingShakeTime <= 0)
                {
                    _isShaking = false;
                    _cinemachineVCam.LookAt.localPosition = _originalLocalPosition;
                }
            }
        }

        private void StartShake(float amount,float time)
        {
            if (!_isShaking)
            {
                _originalLocalPosition = _cinemachineVCam.LookAt.localPosition;
            }

            _isShaking = true;
            _shakeAmount = amount;
            _remainingShakeTime = time;
        }

        public static void Shake(float amount, float time)
        {
            foreach (var t in _cameras)
            {
                t.StartShake(amount,time);
            }
        }

        private void StopShake()
        {
            _originalLocalPosition = _cinemachineVCam.LookAt.localPosition;
            _isShaking = false;
            _shakeAmount = 0f;
            _remainingShakeTime = 0f;
        }
        public static void Stop()
        {
            foreach (var t in _cameras)
            {
                t.StopShake();
            }
        }
    }
}