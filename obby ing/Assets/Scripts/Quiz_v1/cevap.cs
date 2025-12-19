using UnityEngine;
using UnityEngine.UI;

public class cevap : MonoBehaviour
{
    public bool isCorrect;
    public quiz_manager quizManager;

    public void Cevapla()
    {
        Button btn = GetComponent<Button>();

        if (isCorrect)
            quizManager.DogruCevap(btn);
        else
            quizManager.YanlisCevap(btn);
    }
}
