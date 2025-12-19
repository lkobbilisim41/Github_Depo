using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class tuzak : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Reload()
    {
        // Eğer oyunu pause'ladıysanız timeScale'i sıfırlayın
        Time.timeScale = 1f;
        // Aynı sahneyi ismiyle yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // veya build index ile:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    } 
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "tuzak")
        {
            Reload();
        }
    }

}
