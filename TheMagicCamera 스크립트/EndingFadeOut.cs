using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingFadeOut : MonoBehaviour
{
    //Fade ==================================================

    public float animTime = 0.5f;         // Fade �ִϸ��̼� ��� �ð� (����:��).  
    public Image fadeImage;  

    private float start = 1f;           // Mathf.Lerp �޼ҵ��� ù��° ��.  
    private float end = 0f;             // Mathf.Lerp �޼ҵ��� �ι�° ��.  
    private float time = 0f;            // Mathf.Lerp �޼ҵ��� �ð� ��.  

    public bool stopIn = true; //false�϶� ����Ǵ°ǵ�, �ʱⰪ�� false�� �� ������ ���� �����Ҷ� ���̵������� ������...�װ� ������ true�� �ϸ��.
    public bool stopOut = true;

    //=======================================================

    void Start()
    {
        StartCoroutine(PlayFadeIn());
    }

    IEnumerator PlayFadeIn()
    {
        while (true)
        {
            if (time >= animTime)
            {
                time = 0;
                yield break;
            }

            time += Time.deltaTime / animTime;
            Color color = fadeImage.color;
            color.a = Mathf.Lerp(start, end, time);
            fadeImage.color = color;

            yield return null;
        }
    }
}
