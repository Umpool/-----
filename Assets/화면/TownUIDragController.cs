using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TownUIDragController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("이동 및 감도 설정")]
    [Range(0.1f, 2.0f)]
    [SerializeField] private float dragSensitivity = 0.5f; // 값이 작을수록 느려지고, 클수록 빨라집니다. (기본값 0.5)
    [SerializeField] private float smoothSpeed = 10f;

    private RectTransform rectTransform;
    private RectTransform parentRectTransform;

    private Vector2 targetPosition;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;
        targetPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (!isDragging && Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 0.1f)
            return;

        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * smoothSpeed);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        targetPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 캔버스 스케일과 개발자가 설정한 드래그 감도(dragSensitivity)를 곱해 속도를 제어합니다.
        Vector2 delta = (eventData.delta / parentRectTransform.localScale.x) * dragSensitivity;
        targetPosition += delta;

        targetPosition = ClampPosition(targetPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private Vector2 ClampPosition(Vector2 pos)
    {
        float minX = -(rectTransform.rect.width - parentRectTransform.rect.width) / 2f;
        float maxX = (rectTransform.rect.width - parentRectTransform.rect.width) / 2f;

        float minY = -(rectTransform.rect.height - parentRectTransform.rect.height) / 2f;
        float maxY = (rectTransform.rect.height - parentRectTransform.rect.height) / 2f;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }
}
