using System;
using UnityEngine;

namespace TuningTraveler
{
    /// <summary>
    /// FixedUpdateメソッド内で_toFollow変数に割り当てられたTransformの位置と回転をこのオブジェクトの位置と回転にコピー
    /// </summary>
    public class FixedUpdateFollow : MonoBehaviour
    {
        public Transform _toFollow;

        private void FixedUpdate()
        {
            transform.position = _toFollow.position;
            transform.rotation = _toFollow.rotation;
        }
    }
}
