using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using TMPro;

public class hareket : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float jumpForce = 7f;
    [SerializeField] private LayerMask jumpableGround;


    private float yatay_hareket = 0f;
    private float dikey_hareket = 0f;

    private Vector3 playerPosition; // Oyuncu pozisyonunu saklamak için
    private float x, y, z;

    public PlayerData playerData; // Unity Editor'da atayabileceğiniz ScriptableObject

    //public TextMeshProUGUI Dogru_say;



    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();

        LoadPlayerPosition();

    }

    void Update()
    {
        yatay_hareket = Input.GetAxis("Horizontal");
        dikey_hareket = Input.GetAxis("Vertical");


        Vector2 hareket = new Vector2(yatay_hareket, 0f) * moveSpeed;
        rb.velocity = new Vector2(hareket.x, rb.velocity.y);

        if (yatay_hareket >= .2f)
        {
            rb.velocity = new Vector2(yatay_hareket * moveSpeed, rb.velocity.y);
        }
        else if (yatay_hareket <= -.2f)
        {
            rb.velocity = new Vector2(yatay_hareket * moveSpeed, rb.velocity.y);
        }
        if (IsGrounded())
        {
            if (Input.GetButtonDown("Jump"))
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            if (dikey_hareket >= .5f)
            {
                rb.velocity = new Vector2(rb.velocity.x, dikey_hareket * jumpForce);
            }
        }
        UpdateAnimationState();

    }
    private void UpdateAnimationState()
    {
        if (yatay_hareket >= .2f)
        {
            anim.SetInteger("state", 1);
            sprite.flipX = false;
        }
        if (yatay_hareket <= .2f)
        {
            anim.SetInteger("state", 1);
            sprite.flipX = true;
        }
        if(yatay_hareket == 0f)
        {
            anim.SetInteger("state", 0);
        }

        if (rb.velocity.y > .5f)
        {
            anim.SetInteger("state", 2);
        }

        if (rb.velocity.y < -.5f)
        {
            anim.SetInteger("state", 3);
        }

        if (rb.transform.position.y < -5)
        {

            transform.position = new Vector3(0,0,0);
        }


    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.CompareTag("tuzak"))
        {
            // Oyuncu meyveyi topladığında yapılacak işlemler
            //Meyve_say.text = "Toplanan : " + PlayerData.meyveSayisi;
            collider.gameObject.SetActive(false);
            //Destroy(collider.gameObject);
            //SavePlayerPosition(transform.position);
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);			
            playerData.playerPosition = new Vector3(0, 0, 0);
        }


        if (collider.gameObject.CompareTag("toplanacak"))
        {
            // Oyuncu meyveyi topladığında yapılacak işlemler
            //Meyve_say.text = "Toplanan : " + PlayerData.meyveSayisi;
            collider.gameObject.SetActive(false);
            Destroy(collider.gameObject);
            SavePlayerPosition(transform.position);
            //Quiz();

            //soru.SetActive(true);
        }
        if (collider.gameObject.CompareTag("gameover"))
        {
            Debug.Log("Finish1");
            playerData.toplanan_nesne.Clear();
            playerData.cevaplanan_soru.Clear();
            playerData.playerPosition = new Vector3(0, 0, 0);
            playerData.meyveSayisi = 0;
            playerData.dogru_sayisi = 0;
            playerData.yanlis_sayisi = 0;
            GameManager.instance.Game_Finish();
            //soru.SetActive(true);
        }



    }
    public void Quiz()
    {
        GameManager.instance.LoadQuizScene();
    }

    void Awake()
    {


    }
    public void SavePlayerPosition(Vector3 position)
    {
        playerData.playerPosition = position;

    }
    public void LoadPlayerPosition()
    {
        x = playerData.playerPosition.x;
        y = playerData.playerPosition.y - 1;
        z = playerData.playerPosition.z;


        transform.position = new Vector3(x, y, z);

        List<string> toplananNesneler = playerData.toplanan_nesne;

        foreach (string nesneAdi in toplananNesneler)
        {
            GameObject nesne = GameObject.Find(nesneAdi);

            if (nesne != null)
            {
                // Toplanan nesneyi etkinleştir
                nesne.SetActive(false);
            }
        }

    }



}