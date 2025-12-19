using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TPSController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 120f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public Animator animator;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
        LookAround();
    }

    void Move()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float vertical = Input.GetAxis("Vertical"); // W-S
        float horizontal = Input.GetAxis("Horizontal"); // A-D

        // Sadece ileri–geri hareket
        Vector3 move = transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Sað–sol karakteri döndürür
        transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // Zýplama
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
                animator.SetTrigger("jump");
        }

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Animator "state" deðiþkeni
        if (animator != null)
        {
            if (vertical != 0)
                animator.SetInteger("state", 1); // yürüyüþ
            else
                animator.SetInteger("state", 0); // idle
        }
    }

    void LookAround()
    {
        // Fare kontrolü varsa aktif et
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * 2f;
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        cameraTransform.Rotate(Vector3.up * mouseX);
        cameraTransform.Rotate(Vector3.left * mouseY);
    }
}
