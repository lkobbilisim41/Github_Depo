using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dusman_hareket : MonoBehaviour
{
    public float moveSpeed = 0.1f; // Düşmanın hareket hızı
    private Transform target; // Hedef (genellikle oyuncu karakteri)

    void Start()
    {
        // Hedefi oyuncu karakteri olarak ayarla
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
		/*
		Debug.Log("hedef konum x: "+transform.position.x);
		Debug.Log("hedef konum y: "+transform.position.y);
		Debug.Log("hedef konum z: "+transform.position.z);
		*/

		
        if (target != null)
        {
            // Hedefe doğru yönel
            Vector3 direction = (target.position - transform.position).normalized;

            // Yalnızca z ekseni boyunca hareket etmesini sağlamak için y eksenini sıfırla
            direction.y = 0f;

            // Hareket et (sadece z ekseni üzerinde)
			transform.Translate(new Vector3(direction.x*moveSpeed*Time.deltaTime, 0, direction.z*moveSpeed*Time.deltaTime), Space.World);
			
			/*
			Debug.Log("hedef konum x: "+direction.x);
			Debug.Log("hedef konum y: "+direction.y);
			Debug.Log("hedef konum z: "+direction.z);  
			*/
						
		}
    }
}
