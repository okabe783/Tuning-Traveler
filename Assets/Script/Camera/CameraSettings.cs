using System;
using Cinemachine;
using UnityEngine;

namespace TuningTraveler
{
    public class CameraSettings : MonoBehaviour
    {
        public Transform _follow;
        public Transform _lookAt;
        public CinemachineFreeLook _keyboardAndMouseCamera;
        public CinemachineFreeLook _controllerCamera;
        public InputChoice _inputChoice;
        public InvertSettings _keyboardAndMouseInvertSettings;
        public InvertSettings _controllerInvertSettings;
        public bool _allowRuntimeCameraSettingsChanges;
        public enum InputChoice
        {
            KeyboardAndMouse, Controller,
        }

        [Serializable]
        public struct InvertSettings
        {
            public bool invertX;
            public bool invertY;
        }
        public CinemachineFreeLook Current => _inputChoice == InputChoice.KeyboardAndMouse 
            ? _keyboardAndMouseCamera : _controllerCamera;

        private void Reset()
        {
            var keyboardAndMouseCameraTransform = transform.Find("KeyboardAndMouseFreeLookRig");
            if (keyboardAndMouseCameraTransform != null)
                _keyboardAndMouseCamera = keyboardAndMouseCameraTransform.GetComponent<CinemachineFreeLook>();

            var controllerCameraTransform = transform.Find("ControllerFreeLookRig");
            if (controllerCameraTransform != null)
                _controllerCamera = controllerCameraTransform.GetComponent<CinemachineFreeLook>();

            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.name == "Player")
            {
                _follow = playerController.transform;
                _lookAt = _follow.Find("HeadTarget");

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
            _keyboardAndMouseCamera.m_XAxis.m_InvertInput = _keyboardAndMouseInvertSettings.invertX;
            _keyboardAndMouseCamera.m_XAxis.m_InvertInput = _keyboardAndMouseInvertSettings.invertY;

            _controllerCamera.m_XAxis.m_InvertInput = _controllerInvertSettings.invertX;
            _controllerCamera.m_XAxis.m_InvertInput = _controllerInvertSettings.invertY;
            _controllerCamera.Follow = _follow;
            _controllerCamera.LookAt = _lookAt;

            _keyboardAndMouseCamera.Priority = _inputChoice == InputChoice.KeyboardAndMouse ? 1 : 0;
            _controllerCamera.Priority = _inputChoice == InputChoice.Controller ? 1 : 0;
        }
    }
}

