using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSControllerWithAnimations : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    // Yeni eklenen: Animator referansı
    [SerializeField] private Animator animator = null;
    
    CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        // Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!animator) 
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Animasyon güncellemeleri
        animator.SetFloat("MoveSpeed", new Vector3(curSpeedX, 0, curSpeedY).magnitude);
        //animator.SetBool("IsRunning", isRunning);

        #endregion

        #region Handles Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
            animator.SetTrigger("Jump");
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Animasyon güncellemesi
        animator.SetBool("Grounded", characterController.isGrounded);

        #endregion

        #region Handles Rotation
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        #endregion
    }

	/*
    private void OnCollisionEnter(Collision collision)
    {
        // Çarpışma olayları ve nesne imhası
        if (collision.gameObject.tag == "toplanacak")
        {
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag != "zemin")
        {
			Debug.Log("Çarpılan Nesne: "+collision.gameObject.tag);
			
		}
	}
	*/
}
