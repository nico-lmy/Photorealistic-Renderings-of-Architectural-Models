using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCamera : MonoBehaviour
{
    public float speed = 10.0f;
    public float mouseSensitivity = 0.2f; 
    private float rotationY = 0.0f;
    private float rotationX = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        rotationX -= mouseDelta.y * mouseSensitivity;
        rotationY += mouseDelta.x * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);

        float moveForward = 0f;
        float moveRight = 0f;
        float moveUp = 0f;

        var kb = Keyboard.current;

        // mouvements : Z, Q, S, D / W, A, S, D 

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveForward += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveForward -= 1f;

        if (kb.aKey.isPressed || kb.qKey.isPressed || kb.leftArrowKey.isPressed) moveRight -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveRight += 1f;

        Vector3 movement = new Vector3(moveRight, moveUp, moveForward) * speed * Time.deltaTime;
        transform.Translate(movement);

        if (kb.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
