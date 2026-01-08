using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Manager : MonoBehaviour
{

    public void antalya_menu1()
    {
        SceneManager.LoadScene("antalya_alanya");
    }
	
    public void antalya_menu2()
    {
        SceneManager.LoadScene("antalya_yivli");
    }
	
    public void ardahan_menu()
    {
        SceneManager.LoadScene("ardahan");
    }
	
	public void erzurum_menu()
    {
        SceneManager.LoadScene("erzurum");
    }
	
	public void kayseri_menu()
    {
        SceneManager.LoadScene("kayseri");
    }
	
	public void halka_begus()
    {
		SceneManager.LoadScene("konya_halka_begus");
    }

	public void konya_ince_minare(){
		
        SceneManager.LoadScene("konya_ince_minare");
	}

	public void kirsehir_menu()
    {
        SceneManager.LoadScene("kirsehir");
    }
	
	public void ana_menu()
    {
        SceneManager.LoadScene("menu");
    }
	  
	
    public void ExitButtonClick()
    {
        Application.Quit();
		
    }
}
