using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;
public class PlayingUI : MonoBehaviour
{
    [SerializeField]
    List<TextMeshProUGUI> doList;  //�ؾ����� ����Ʈ
    [SerializeField]
    int doCnt=0;
    [SerializeField]
    TodoList listComp;
    [SerializeField]
    TextAnimatorPlayer animComp;
    //����Ʈ ����

    public void SetDoList(bool _isFadeIn = true)
    {
        StartCoroutine(FadeList(_isFadeIn));       
    }

    public IEnumerator FadeList(bool _isFadeIn = true)
    {
        float t_time = 1f; //�۾��� ������ �ð���
        float progress = 0f; //�۾� �ۼ�������
        float smoothness = 0.02f;
        float increment = smoothness / t_time;

        WaitForSeconds waitSec = new WaitForSeconds(smoothness);
        //���̵� ���� �������
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

            //üũ�̹��� �ʱ�ȭ
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

    //�ؾ������� �������� �� �Լ� ȣ���ؼ� �������� ����
    [ContextMenu("��������üũ")]
    public void Check_ThingsToDo(string _nodeString)
    {
        //���� string�� doList�� ����� ���ٸ�
        var doNode = doList.Find(_x => _x.text.Equals(_nodeString));
        Image doNodeImage = doNode.transform.Find("check").GetComponent<Image>();
        if(doNodeImage != null)
            StartCoroutine(CheckList(doNodeImage));

        IEnumerator CheckList(Image _checkImg)
        {
            _checkImg.color = Color.white;
            //üũǥ�ð� ä�����鼭,�������� �ȴ�.
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

        PlutobiManager.PlutobiSoundManager.Instance.PlaySFXOneShot("���� �߱�");

    }
    public void DoNodeActivate(int _index)    //������ �Ϸ��� �� ����� �͵�
    {
        List<Todo_forSomeThing> comp = listComp.GetTodoList();
        if (comp[_index].interactionObj != null)         //ù��°�� ����ǥ�� Ȱ��ȭ�Ѵ�.
        {
            if (comp[_index].priorCondition.Length > 0)       //���������� �����Ѵٸ�
            {
                comp[_index].interactionObj.SetActive(false);
                //�������� ������Ʈ���� ����ǥ�� Ȱ��ȭ
                for (int j = 0; j < comp[_index].priorCondition.Length; j++)
                {
                    //�ѹ� �������ְ� ���� �θ𶧹��� ��Ȱ��ȭ �� ������Ʈ�� ���������� Ȱ��ȭ ����
                    comp[_index].priorCondition[j].SetActive(true);
                }
            }
            else                                                        //�ƴϸ� �ٷ� ����
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
