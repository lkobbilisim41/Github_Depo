using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mermi_yokol : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision){
        if (collision.gameObject.tag!="Player" || collision.gameObject.tag != "zemin"){
            Debug.Log(collision.gameObject.tag);
            if (collision.gameObject.CompareTag("hedef")){
                // Eğer çarpılan obje "Target" etiketine sahipse

                Destroy(collision.gameObject);
                // Hedefi vurulduğunda puan kazanmak için HitTarget fonksiyonunu çağır
            }
        }
    }
}
