using UnityEngine;

public class InputManager : SingletonBase<InputManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public Vector3 GetWorldInputPosition(Camera camera)
    {
        Vector3 pos;

#if UNITY_EDITOR || UNITY_STANDALONE
        pos = Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            pos = Input.GetTouch(0).position;
        else
            return Vector3.negativeInfinity; // 무효 처리
#endif

        pos = camera.ScreenToWorldPoint(pos);
        pos.z = 0;
        return pos;
    }

    public bool IsInputDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#endif
    }

    // Phase        설명
    // Began        터치가 시작됨
    // Moved        터치 중 움직임 발생
    // Stationary   터치 중이지만 움직이지 않음
    // Ended        손가락이 떼어짐
    // Canceled     터치가 시스템에 의해 취소됨(예: 전화 수신 등)

    public bool IsInputUp()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonUp(0);
#elif UNITY_ANDROID || UNITY_IOS
    return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
#endif
    }

    public bool IsInputHeld()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved;
#endif
    }
}
