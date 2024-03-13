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
            Transform keyboardAndMouseCameraTransform = transform.Find("");
            if (keyboardAndMouseCameraTransform != null)
                _keyboardAndMouseCamera = keyboardAndMouseCameraTransform.GetComponent<CinemachineFreeLook>();

            Transform controllerCameraTransform = transform.Find("");
            if (controllerCameraTransform != null)
                _controllerCamera = controllerCameraTransform.GetComponent<CinemachineFreeLook>();

            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.name == "")
            {
                _follow = playerController.transform;
                _lookAt = _follow.Find("");

                if (playerController.GetComponent<Camera>())
                {
                    
                }
            }
        }
    }
}

