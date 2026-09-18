using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform handle;
    [SerializeField] private float radius = 80f;

    private RectTransform joystickArea;

    public event Action<Vector2> MovementInputChanged;

    private void Awake()
    {
        joystickArea = (RectTransform)transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickArea,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint) == false)
        {
            return;
        }

        Vector2 normalizedInput = Vector2.ClampMagnitude(localPoint / radius, 1f);
        handle.anchoredPosition = normalizedInput * radius;
        MovementInputChanged?.Invoke(normalizedInput);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = Vector2.zero;
        MovementInputChanged?.Invoke(Vector2.zero);
    }
}