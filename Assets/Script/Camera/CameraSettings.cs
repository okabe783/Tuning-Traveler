using System;
using Cinemachine;
using UnityEngine;

namespace TuningTraveler
{
    public class CameraSettings : MonoBehaviour
    {
        public enum InputChoice
        {
            KeyboardAndMouse, 
            Controller
        }
        public InputChoice _inputChoice;
        public CinemachineFreeLook Current => _inputChoice == InputChoice.KeyboardAndMouse 
            ? _keyboardAndMouseCamera : _controllerCamera;
        
        [Serializable]
        [Tooltip("反転")]public struct InvertSettings
        {
            public bool invertX;
            public bool invertY;
        }
        public InvertSettings _keyboardAndMouseInvertSettings;　//keyboardの反転設定
        public InvertSettings _controllerInvertSettings;　//controllerの反転設定
        
        public Transform _follow;
        public Transform _lookAt;
        
        public CinemachineFreeLook _keyboardAndMouseCamera;
        public CinemachineFreeLook _controllerCamera;
        
        public bool _allowRuntimeCameraSettingsChanges;　//cameraの設を実行時に変更することを制御
        
        private void Reset()
        {
            var keyboardAndMouseCameraTransform = transform.Find("");
            if (keyboardAndMouseCameraTransform != null)
                _keyboardAndMouseCamera = keyboardAndMouseCameraTransform.GetComponent<CinemachineFreeLook>();

            var controllerCameraTransform = transform.Find("");
            if (controllerCameraTransform != null)
                _controllerCamera = controllerCameraTransform.GetComponent<CinemachineFreeLook>();

            //指定されたscriptがアタッチされているobjectを検索
            var playerController = FindObjectOfType<PlayerController>();
            //存在していて名前が同じならfollowとlookatを設定
            if (playerController != null && playerController.name == "")
            {
                _follow = playerController.transform;
                _lookAt = _follow.Find("");

                if (playerController._cameraSettings == null)
                    playerController._cameraSettings = this;
            }
        }

        private void Awake()
        {
            UpdateCameraSettings();
        }

        private void Update()
        {
            if (_allowRuntimeCameraSettingsChanges)
            {
                UpdateCameraSettings();
            }
        }

        private void UpdateCameraSettings()
        {
            _keyboardAndMouseCamera.Follow = _follow;
            _keyboardAndMouseCamera.LookAt = _lookAt;
            //keyboardの反転がtrueの場合の反転設定
            _keyboardAndMouseCamera.m_XAxis.m_InvertInput = _keyboardAndMouseInvertSettings.invertX;
            _keyboardAndMouseCamera.m_YAxis.m_InvertInput = _keyboardAndMouseInvertSettings.invertY;

            _controllerCamera.Follow = _follow;
            _controllerCamera.LookAt = _lookAt;
            //controllerの反転がtrueの場合の反転設定
            _controllerCamera.m_XAxis.m_InvertInput = _controllerInvertSettings.invertX;
            _controllerCamera.m_YAxis.m_InvertInput = _controllerInvertSettings.invertY;
            
            //同じ位置にシネマシーンがある場合の優先度を設定
            _keyboardAndMouseCamera.Priority = _inputChoice == InputChoice.KeyboardAndMouse ? 1 : 0;
            _controllerCamera.Priority = _inputChoice == InputChoice.Controller ? 1 : 0;
        }
    }
}

