using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private float _movePower = 3; //移動速度
    private Rigidbody _rb;
    private Vector3 _dir; //キャラクターの移動方向を表すベクトル
    private Animator _animator;
    private Vector2 _movement;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
    }

    private void FixedUpdate()
    {
        var velocity = _rb.velocity;
        velocity.y = 0;
        _animator.SetFloat("ForwardSpeed", velocity.magnitude);
    }

    private void Move()
    {
        //水平方向と垂直方向の移動を取得
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");
        
        //入力方向を計算しカメラの向きを合わせる
        _dir = Vector3.forward * v + Vector3.right * h;
        if (Camera.main != null) 
            _dir = Camera.main.transform.TransformDirection(_dir);
        
        _dir.y = 0; //上下方向の移動を無効
        
        //移動方向を正規化
        var forward = _dir.normalized;
        _rb.velocity = forward * _movePower;
        forward.y = 0;
        
        //キャラクターの向きを移動方向に合わせる
        if (forward != Vector3.zero)
        {
            this.transform.forward = forward;
        }
    }

    private void OnAnimatorMove()
    {
        transform.position = _animator.rootPosition;
    }
}
