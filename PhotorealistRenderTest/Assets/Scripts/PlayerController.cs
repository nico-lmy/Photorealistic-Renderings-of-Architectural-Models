using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraTransform; 

    [Header("Settings")]
    public float speed = 1.2f;
    public float mouseSensitivity = 0.2f; 
    public float gravity = -9.81f; 

    private float rotationX = 0f;
    private Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (controller == null) controller = GetComponent<CharacterController>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        rotationX -= mouseDelta.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0);
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);

        float moveForward = 0f;
        float moveRight = 0f;

        var kb = Keyboard.current;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveForward += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveForward -= 1f;

        if (kb.aKey.isPressed || kb.qKey.isPressed || kb.leftArrowKey.isPressed) moveRight -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveRight += 1f;

        Vector3 move = transform.right * moveRight + transform.forward * moveForward;
        move = Vector3.ClampMagnitude(move, 1f);
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (kb.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;
    }
}
