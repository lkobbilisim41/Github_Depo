using UnityEngine;

public class FlyingFPSController : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 10f;
    public float verticalSpeed = 6f;

    [Header("Mouse")]
    public float mouseSensitivity = 2f;
    public Transform cameraHolder;

    private float xRotation = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MouseLook();
        VerticalMovement();
    }

    void FixedUpdate()
    {
        Move();
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100 * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        rb.AddForce(move * moveSpeed, ForceMode.Acceleration);
    }

    void VerticalMovement()
    {
        if (Input.GetKey(KeyCode.Space))
            rb.AddForce(Vector3.up * verticalSpeed, ForceMode.Acceleration);

        if (Input.GetKey(KeyCode.LeftControl))
            rb.AddForce(Vector3.down * verticalSpeed, ForceMode.Acceleration);
    }
}
