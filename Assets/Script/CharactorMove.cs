using UnityEngine;

public class CharactorMove : MonoBehaviour
{
    private Animator animator;
    //回転
    private Quaternion targetRotation;

    private void Awake()
    {
        //コンポーネント関連付け
        TryGetComponent(out animator);
        
        //初期化
        targetRotation = transform.rotation;
    }
    
    void Update()
    {
        Move();
    }

    //Playerの動き
    void Move()
    {
        //入力ベクトルの取得
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        //カメラの方向に合わせて水平な回転を行い位置を合わせる
        var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        var velo = horizontalRotation * new Vector3(horizontal, 0, vertical).normalized;
        
        //速度の取得
        var speed = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
        var rotationSpeed = 600 * Time.deltaTime;
        
        //移動方向を向く
        if (velo.magnitude > 0.5f)
        {
            targetRotation = Quaternion.LookRotation(velo,Vector3.up);
        }
        
        //移動速度をAnimatorに反映
        animator.SetFloat("Speed",velo.magnitude * speed,0.1f,Time.deltaTime);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
    }
}
