using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 20f;
    public float gravity = 10f;
    public float groundCheckDistance = 2f; // Yerden bir mesafe
    public LayerMask groundLayer; // Yerin hangi katman� kullanaca��n� belirler (Terrain veya yer katman�)

    private float currentSpeed;
    private float verticalVelocity = 0f;
    private CharacterController controller;

    // Ko�ma kontrol�
    private bool isRunning = false;
    private bool isGrounded = false;

    Vector3 moveDirection = Vector3.zero;
    public PlayerData playerData; // Unity Editor'da atayabileceğiniz ScriptableObject
    
    private Vector3 playerPosition; // Oyuncu pozisyonunu saklamak için
    private float x, y, z;

    void Start()
    {


    }
    void Awake()
    {
        controller = GetComponent<CharacterController>(); // Her durumda alınmalı
        currentSpeed = walkSpeed;

        if (playerData != null && playerData.is_saved)
        {
            x = playerData.playerPosition.x;
            y = playerData.playerPosition.y;
            z = playerData.playerPosition.z;

            transform.position = new Vector3(x, y + 20, z);
            playerData.is_saved = false; // Sahneye tekrar dönerken sıfırlıyoruz

            foreach (string nesneAdi in playerData.toplanan_nesne)
            {
                GameObject nesne = GameObject.Find(nesneAdi);
                if (nesne != null)
                    nesne.SetActive(false);
            }
        }
        else
        {
            GameObject startPlane = GameObject.Find("Start");
            if (startPlane != null)
            {
                Vector3 startPos = startPlane.transform.position + Vector3.up * 20f;
                transform.position = startPos;
            }
            else
            {
                Debug.LogWarning("Start adlı plane bulunamadı!");
            }
        }
    }


    void Update()
    {

        // Raycast ile yer tespiti
        isGrounded = CheckIfGrounded();

        float moveX = 0f;
        float moveZ = isRunning ? 1f : 0f;
        float movementDirectionY = transform.position.y;

        // Yer�ekimi ve z�plama i�in y ekseni
        if (isGrounded)
        {
            //verticalVelocity = -1f; // Küçük negatif değer önerilir
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        //Debug.Log(isGrounded);
        //Debug.Log(verticalVelocity);
        //Debug.Log(movementDirectionY);

        // Hareket vekt�r�
        moveDirection = new Vector3(moveX, verticalVelocity, moveZ);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection.x *= currentSpeed;
        moveDirection.z *= currentSpeed;

        controller.Move(moveDirection * Time.deltaTime);
    }

    public void StartRunning()
    {
        Debug.Log("Ko�ma ba�lad�");
        isRunning = true;
        currentSpeed = runSpeed;
    }

    public void StopRunning()
    {
        Debug.Log("Ko�ma durdu");
        isRunning = false;
        currentSpeed = walkSpeed;
    }

    public void Jump()
    {
        Debug.Log("Z�plama tu�u");
        if (isGrounded)
        {
            Debug.Log("Yerdeyim, z�pl�yorum!");
            verticalVelocity = jumpForce; // Z�plama kuvveti uygula
        }
        else
        {
            Debug.Log("Havaday�m, z�plamam!");
        }
    }

    // Raycast ile yer tespiti yapan fonksiyon
    private bool CheckIfGrounded()
    {
        RaycastHit hit;

        // Karakterin alt�ndan bir ���n g�ndererek yer ile temas�n� kontrol et
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                return true; // Yerle temas var
            }
            else
            {
                return false; // Temas var ama "Terrain" de�il
            }
        }

        return false; // Hi�bir �eyle temas yok
    }
}