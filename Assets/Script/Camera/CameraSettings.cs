using System;
using UnityEngine;
using Cinemachine;

/// <summary>keyboardなどの操作によって異なるcameraをアクティブにする</summary>
public class CameraSettings : MonoBehaviour
{
    public enum InputChoice
    {
        keyboardAndMouse,Controller
    }
    [Serializable]
    public struct InvertSettings
    {
        public bool invertX;
        public bool invertY;
    }

    public Transform _follow;
    public Transform _lookAt;

    public CinemachineFreeLook _keyboardAndMouseCamera;
    public CinemachineFreeLook _controllerCamera;
    public InputChoice _inputChoice;
    public InvertSettings _keyboardAndMouseInvertSettings;
    public InvertSettings _controllerInvertSettings;
    public bool _allowRuntimeCameraSettingsChanges; //Cameraの設定を変更可能か判定

    private void Awake()
    {
        UpdateCameraSettings();
    }

    private void Update()
    {
        if(_allowRuntimeCameraSettingsChanges)
            UpdateCameraSettings();
    }

    /// <summary>Cameraの設定を更新</summary>
    private void UpdateCameraSettings()
    {
        _keyboardAndMouseCamera.Follow = _follow;
        _keyboardAndMouseCamera.LookAt = _lookAt;
        _keyboardAndMouseCamera.m_XAxis.m_InvertInput = _keyboardAndMouseInvertSettings.invertX;
        _keyboardAndMouseCamera.m_YAxis.m_InvertInput = _keyboardAndMouseInvertSettings.invertY;

        _controllerCamera.m_XAxis.m_InvertInput = _controllerInvertSettings.invertX;
        _controllerCamera.m_YAxis.m_InvertInput = _controllerInvertSettings.invertY;
        _controllerCamera.Follow = _follow;
        _controllerCamera.LookAt = _lookAt;

        _keyboardAndMouseCamera.Priority = _inputChoice == InputChoice.keyboardAndMouse ? 1 : 0;
        _controllerCamera.Priority = _inputChoice == InputChoice.Controller ? 1 : 0;
    }
}