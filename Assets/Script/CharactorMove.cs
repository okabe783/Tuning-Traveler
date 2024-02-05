using UnityEngine;

public class CharactorMove : MonoBehaviour
{
    private Animator animator;

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
        //入力ベクトルの取得
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        var velo = horizontalRotation * new Vector3(horizontal, 0, vertical).normalized;
        
        //速度の取得
        var speed = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
        var rotationSpeed = 600 * Time.deltaTime;
        
        //移動方向を向く
        if (velo.magnitude > 0.5f)
        {
            transform.rotation = Quaternion.LookRotation(velo,Vector3.up);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
        //移動速度をAnimatorに反映
        animator.SetFloat("Speed",velo.magnitude * speed,0.1f,Time.deltaTime);
    }
}
