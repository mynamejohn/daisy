using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;
public class PlayingUI : MonoBehaviour
{
    [SerializeField]
    List<TextMeshProUGUI> doList;  //해야할일 리스트
    [SerializeField]
    int doCnt=0;
    [SerializeField]
    TodoList listComp;
    [SerializeField]
    TextAnimatorPlayer animComp;
    //리스트 설정

    public void SetDoList(bool _isFadeIn = true)
    {
        StartCoroutine(FadeList(_isFadeIn));       
    }

    public IEnumerator FadeList(bool _isFadeIn = true)
    {
        float t_time = 1f; //작업을 진행할 시간초
        float progress = 0f; //작업 퍼센테이지
        float smoothness = 0.02f;
        float increment = smoothness / t_time;

        WaitForSeconds waitSec = new WaitForSeconds(smoothness);
        //페이드 인이 있을경우
        if (_isFadeIn)
        {
            while (progress < 1f)
            {
                Color lerpColor = Color.Lerp(Color.white, Color.clear, progress);
                for (int i = 0; i < doList.Count; i++)
                {
                    doList[i].color = lerpColor;
                    doList[i].transform.Find("check").GetComponent<Image>().color = lerpColor;
                }
                progress += increment;
                yield return waitSec;
            }

            //체크이미지 초기화
            for (int i = 0; i < doList.Count; i++)
            {
                Image checkImg = doList[i].transform.Find("check").GetComponent<Image>();
                checkImg.fillAmount = 0f;
            }
        }
        
        progress = 0f;
        while (progress < 1f)
        {
            for (int i = 0; i < doList.Count; i++)
                doList[i].color = Color.Lerp(Color.clear, Color.white, progress);
            progress += increment;
            
            yield return waitSec;
            
        }
        doList[doCnt].gameObject.SetActive(true);
    }

    public IEnumerator DoListSlide(Vector2 _pos)
    {
        var list = animComp.transform.parent;
        var listRect = list.GetComponent<RectTransform>();

        while (listRect.anchoredPosition.y <= _pos.y)
        {
            listRect.anchoredPosition = Vector2.Lerp(listRect.anchoredPosition, _pos, 0.5f);
            yield return null;
        }

    }
    
    public void ToDoListInit()
    {
        var mType = PlayManager.Instance.GetManagerType<SideScrollManager>();
        var todoList = mType.todolist.GetTodoList();
        for (int i = 0; i < todoList.Count; i++)
            doList[i].text = todoList[i].txt;
        DoNodeActivate(doCnt);
    }

    //해야할일이 끝났을때 이 함수 호출해서 지워지게 하자
    [ContextMenu("오늘할일체크")]
    public void Check_ThingsToDo(string _nodeString)
    {
        //받은 string이 doList의 문장과 같다면
        var doNode = doList.Find(_x => _x.text.Equals(_nodeString));
        Image doNodeImage = doNode.transform.Find("check").GetComponent<Image>();
        if(doNodeImage != null)
            StartCoroutine(CheckList(doNodeImage));

        IEnumerator CheckList(Image _checkImg)
        {
            _checkImg.color = Color.white;
            //체크표시가 채워지면서,지워지게 된다.
            while (_checkImg.fillAmount < 1)
            {
                _checkImg.fillAmount += Time.deltaTime / .7f;
                yield return null;
            }
        }

        doCnt++;
        DoNodeActivate(doCnt);

        if (doCnt > 4)
        {
            var list = animComp.transform.parent;
            var listRect = list.GetComponent<RectTransform>();
            Vector2 targetPos = new Vector2(listRect.anchoredPosition.x, listRect.anchoredPosition.y + 130f);
            StartCoroutine(DoListSlide(targetPos));
        }

        PlutobiManager.PlutobiSoundManager.Instance.PlaySFXOneShot("할일 긋기");

    }
    public void DoNodeActivate(int _index)    //할일을 완료한 뒤 실행될 것들
    {
        List<Todo_forSomeThing> comp = listComp.GetTodoList();
        if (comp[_index].interactionObj != null)         //첫번째의 물음표를 활성화한다.
        {
            if (comp[_index].priorCondition.Length > 0)       //선행조건이 존재한다면
            {
                comp[_index].interactionObj.SetActive(false);
                //선행조건 오브젝트들의 물음표를 활성화
                for (int j = 0; j < comp[_index].priorCondition.Length; j++)
                {
                    //한번 참조해주고 나면 부모때문에 비활성화 된 오브젝트도 정상적으로 활성화 가능
                    comp[_index].priorCondition[j].SetActive(true);
                }
            }
            else                                                        //아니면 바로 생성
                comp[_index].interactionObj.SetActive(true);
        }
        doList[_index].gameObject.SetActive(true);
        StartTextAnim(doList[_index].gameObject);
    }

    public void StartTextAnim(GameObject txtobj)
    {
        string animText = txtobj.GetComponent<TextMeshProUGUI>().text;
        animComp = txtobj.GetComponent<TextAnimatorPlayer>();
        animComp.ShowText(animText);
    }
    public void Start()
    {
        transform.SetAsLastSibling();
        GetComponent<RectTransform>().sizeDelta = UI_Manager.instance.GetComponent<RectTransform>().sizeDelta;
    }
}
