using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlutobiManager;
public class Option : MonoBehaviour
{
    [SerializeField]
    int currentMenu = 0;
    [SerializeField]
    GameObject[] menu = new GameObject[3];
    [SerializeField]
    GameObject[] content = new GameObject[2];
    [SerializeField]
    Slider slidercomp;
    [Header("½ºÅ×ÀÌÁö¼±ÅÃ°ü·Ã")]
    [SerializeField]    int selectedstage_num = 0;
    [SerializeField]    GameObject[] stage_selected = new GameObject[3];
    [SerializeField] AudioClip[] soundfx = new AudioClip[2];
    // Start is called before the first frame update


    private void Start()
    {
        SetSound(slidercomp.value);
    }
    // Update is called once per frame
    void Update()
    {
        SetSound(slidercomp.value);
    }
    public void SetMenus(int index)
    {
        if (index > 3)
        {
            Debug.Log("Index Error");
            return;
        }
        if (currentMenu == index)
            return;
        else if (index == 0 || index == 1 || index == 2)
        {
            PlutobiSoundManager.Instance.PlaySFXOneShot(soundfx[0]);
            Refresh(index);
        }
    }
    void Refresh(int index)
    {

        menu[currentMenu].transform.GetChild(0).gameObject.SetActive(false);                                    //¹è°æ²¨Áü , ±Û»ö º¹±Í
        menu[currentMenu].transform.GetChild(1).GetComponent<Image>().color = new Color(255, 255, 255, 255);
        if(currentMenu!=2)
            content[currentMenu].SetActive(false);
        currentMenu = index;
        menu[currentMenu].transform.GetChild(0).gameObject.SetActive(true);                                    //¹è°æÄÑÁü , ±Û»ö ±î¸¸»ö
        menu[currentMenu].transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 255);
        if(currentMenu!=2) 
            content[currentMenu].SetActive(true);

        if (currentMenu == 2)
            ReturnToMenu();
    }

    //ÄÁÅÙÃ÷

    public void SetSound(float vol)
    {
        PlutobiManager.PlutobiSoundManager.Instance.SetBGMVolume(vol);
        PlutobiManager.PlutobiSoundManager.Instance.SetSFXVolume(vol);
    }

    public void SelectStage(int stage)                              
    {
        if(stage>7)
        {
            return;
        }
        stage_selected[selectedstage_num].SetActive(true);
        selectedstage_num = stage;
        stage_selected[selectedstage_num].SetActive(false);
        PlutobiSoundManager.Instance.PlaySFXOneShot(soundfx[0]);
    }
    public void LoadStage(int stagenum)                                 //ºôµåÀÇ ¾À¼ø¼­¿Í ÄÁÅÙÃ÷ÀÇ ³ª¿­µÈ ¼ø¼­°¡ »ìÂ¦´Ù¸§
    {
        if ((stagenum-2) == selectedstage_num)
        {
            if (SceneManager.GetActiveScene().buildIndex == stagenum)
            {
                Debug.Log("°°Àº¾ÀÀÔ´Ï´Ù.");
                return;
            }
            PlayManager.Instance.StartWhiteOut();
            StartCoroutine(StageChange(stagenum));
        }
    }
    //public void CancelReturnToMenu()
    //{
    //    Refresh(0);
    //}

    public void ReturnToMenu()
    {
        PlayManager.Instance.StartWhiteOut();
        StartCoroutine(StageChange(1));
    }
    IEnumerator StageChange(int stagenum)
    {
        Debug.Log(stagenum);
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(stagenum);
    }
    public void OptionOff()
    {
        PlutobiSoundManager.Instance.PlaySFXOneShot(soundfx[1]);
        gameObject.SetActive(false) ;
    }
}
