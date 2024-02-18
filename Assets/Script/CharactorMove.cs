using System.Collections;
using UnityEngine;

/// <summary> Playerの入力を他のscriptから受け取り管理するクラス</summary>
public class CharactorMove : MonoBehaviour
{
    //シングルトン化
    public static CharactorMove Instance => _instance;

    //Charactorクラス内でのみアクセス可能
    protected static CharactorMove _instance;

    //Playerに関するフィールド
    [HideInInspector]
    //Playerの入力を無視するかどうかの判定
    public bool _playerInputIgnore;

    //外部からの入力を無視するかの判定
    protected bool _externalInputIgnore;

    protected Vector2 _move;
    protected Vector2 _camera;
    protected bool _jump;
    protected bool _attack;
    protected bool _pause;

    //外部からの入力が無効にされるかPlayerの入力を無効にされた時の処理
    public Vector2 MoveIgnore =>
        _playerInputIgnore || _externalInputIgnore ? Vector2.zero : _move;

    public Vector2 CameraInput =>
        _playerInputIgnore || _externalInputIgnore ? Vector2.zero : _camera;

    public bool JumpInput => _jump && !_playerInputIgnore && !_externalInputIgnore;
    public bool Attack => _attack && !_playerInputIgnore && !_externalInputIgnore;
    public bool Pause => _pause;

    private WaitForSeconds _attackInputWait;
    private Coroutine _attackWaitCoroutine;
    private const float _attackInputDuration = 0.03f;

    private void Awake()
    {
        _attackInputWait = new WaitForSeconds(_attackInputDuration);
        //シングルトン化
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
        {
            throw new UnityException("複数のcharactorMovescriptを持つことはできません。インスタンスは次の通りです"
                                     + _instance.name + "and" + name);
        }
    }

    void Update()
    {
        _move.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _camera.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        _jump = Input.GetButton("Jump");

        if (Input.GetButtonDown("Fire1"))
        {
            if (_attackWaitCoroutine != null)
            {
                StopCoroutine(_attackWaitCoroutine);
            }

            _attackWaitCoroutine = StartCoroutine(AttackWait());
        }

        _pause = Input.GetButtonDown("Pause");
    }

    /// <summary> 攻撃時のWait処理</summary>
    IEnumerator AttackWait()
    {
        _attack = true;
        yield return _attackInputWait;
        _attack = false;
    }
}