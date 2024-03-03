using UnityEngine;

namespace TuningTraveler
{
    public partial class Damageable : MonoBehaviour
    {
        public struct DamageMessage
        {
            public MonoBehaviour _damager;
            public int _amount;
            public Vector3 _direction;
            public Vector3 _damageSource;
            public bool _throwing;
            public bool _stopCamera;
        }
    }
}

