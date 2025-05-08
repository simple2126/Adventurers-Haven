using UnityEngine;

public class InputManager : SingletonBase<InputManager>
{
    private Vector2 dragDirection;
    private Vector2 lastTouchPosition;

    public Vector2 GetDragDirection() => dragDirection;

    private RectTransform checkButtonRect;
    private RectTransform cancelButtonRect;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void BeginDrag()
    {

#if UNITY_EDITOR || UNITY_STANDALONE
        lastTouchPosition = Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            lastTouchPosition = Input.GetTouch(0).position;
#endif
    }

    public void UpdateDrag()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Vector2 current = Input.mousePosition;
        dragDirection = current - lastTouchPosition;
        lastTouchPosition = current;
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                dragDirection = touch.position - lastTouchPosition;
                lastTouchPosition = touch.position;
            }
        }
#endif
    }

    public void EndDrag()
    {
        dragDirection = Vector2.zero;
        lastTouchPosition = Vector2.zero;
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
        return Vector3.negativeInfinity;
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
