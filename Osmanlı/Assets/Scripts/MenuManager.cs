using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Manager : MonoBehaviour
{

    public void istanbul()
    {
        SceneManager.LoadScene("istanbul");
    }
    public void balikesir()
    {
        SceneManager.LoadScene("balikesir");
    }	
    public void bursa()
    {
        SceneManager.LoadScene("bursa");
    }   
    public void canakkale()
    {
        SceneManager.LoadScene("canakkale");
    }
    public void kocaeli()
    {
        SceneManager.LoadScene("kocaeli");
    }
    public void kirklareli()
    {
        SceneManager.LoadScene("kirklareli");
    }
    public void sakarya()
    {
        SceneManager.LoadScene("sakarya");
    }
    public void tekirdag()
    {
        SceneManager.LoadScene("tekirdag");
    }
    public void yalova()
    {
        SceneManager.LoadScene("yalova");
    }
    public void edirne()
    {
        SceneManager.LoadScene("edirne");
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
