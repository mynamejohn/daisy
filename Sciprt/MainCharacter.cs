using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MainCharacter : MonoBehaviour
{
    //캐릭터 상태
    public enum STATUS { IDLE = 0, MOVE, INTERACTION, SLEEP }
    
    //인터렉션 요소들...(퍼즐풀때 혹은 다른상태들)
    public enum INTERACTION_TYPE { NOTHING = 0, SEE_TV, DISGUSTING, WRITE_BOOK, SLEEP, PAINT, EAT_KITCHEN, EAT_RAMEN, EAT_SATANG }

    [Header("데이지 각요소들")]
    public Animator ani;
    [SerializeField]
    STATUS status;
    Rigidbody2D rigid;
    [SerializeField]
    float speedX;
    [SerializeField]
    float speedY;
    [SerializeField]
    List<Sprite> itemList;
    //현재 메인캐릭터와 겹쳐있는 상호작용 오브젝트들
    //inspector상에서 확인을 위해 SerializeField화
    [SerializeField]
    List<InteractionObj> interObjs;
    [SerializeField]
    GameObject colliderObj;
    [SerializeField]
    public Vector3 yoffset = new Vector3(0,-5,0);
    public Vector3 yoffset2 = new Vector3(0, -1, 0);
    [Header("데이지 말풍선 요소들")]
    public SpriteRenderer spriteRenderer_Balloon;   //말풍선
    public SpriteRenderer spriteRenderer_Item;      //말풍선안의 아이템들
    public Sprite[] sprite_items;                   //아이템 스프라이트들

    public delegate void MainCharDelegate();
    public MainCharDelegate mainCharDel;

    [SerializeField ]bool canmove = true;
    public void Start()
    {
        itemList = new List<Sprite>();
        interObjs = new List<InteractionObj>();

        rigid = GetComponent<Rigidbody2D>();
        PlayManager.Instance.SetMainCharacter(gameObject);
        PlayManager.Instance.currentMap = transform.parent;
        PlayManager.Instance.currentMap.GetComponent<Map>().SetBoundToCamera();
        PlayManager.Instance.SetFalseAllMap(transform.parent);
    }
    private void Update()
    {
        if (status == STATUS.IDLE || status == STATUS.MOVE)
            colliderObj.SetActive(true);
        else
            colliderObj.SetActive(false);
        //y축에따라 스케일 변화
        Scaling();
    }
    void Scaling()
    {
        float distanceY = yoffset.y - transform.localPosition.y;
        float percentage = (transform.localPosition.y - yoffset2.y) / (yoffset.y - yoffset2.y) * 100;
        if (status == STATUS.IDLE || status == STATUS.MOVE)
        {
            transform.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f,1), new Vector3(1, 1, 1), percentage / 100);             //크기 한계 지정
        }
    }
    public void SetScale()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }
    //마우스 클릭 델리게이트 등록
    public void ClickDeleReg()
    {
        MouseController.Instance.InitClickDele();
        MouseController.Instance.clickBegin = new MouseController.ClickBegin(() =>
        {
            //겹쳐있는 오브젝트가 1개 이상일 경우 체크
            if (interObjs.Count > 0)
            {
                var clickObj = interObjs.Find(_x => _x.gameObject == MouseController.Instance.interactionObj);
                if (clickObj != null)
                {
                    status = STATUS.INTERACTION;
                    rigid.velocity = Vector2.zero;
                    clickObj.Interaction();
                }

            }
        });
        MouseController.Instance.clicking = new MouseController.Clicking(() =>
        {
            if (status != STATUS.INTERACTION)
                Move();
        });
        MouseController.Instance.clickExit = new MouseController.ClickExit(() => {
            ani.SetBool("isMove", false);
            status = STATUS.IDLE;
            rigid.velocity = Vector2.zero;
        });
    }

    //움직임
    private void Move()
    {
        if (canmove)
        {
            Vector3 targetPos = PlayManager.Instance.GetMainCam().GetCam().ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 4, 0);
            float distanceX = Mathf.Abs(transform.position.x - targetPos.x);
            float distanceY = Mathf.Abs(transform.position.y - targetPos.y);
            //Vector3 distanceV = targetPos - transform.position;

            if (distanceX > 0.1f)
            {
                Vector2 dirX = Vector2.zero;
                Vector2 dirY = Vector2.zero;
                status = STATUS.MOVE;
                ani.SetBool("isMove", true);                                                                                //y축추가
                if (transform.position.x < targetPos.x)
                {
                    ani.SetFloat("moveDir", 1.0f);
                    dirX += Vector2.right;
                    //rigid.velocity = Vector2.right * speed;
                }
                else
                {
                    ani.SetFloat("moveDir", -1.0f);
                    dirX += Vector2.left;
                    //rigid.velocity = Vector2.left * speed;
                }
                if (distanceY > 0.1f)
                {
                    if (transform.position.y < targetPos.y)
                    {
                        dirY += Vector2.up;
                    }
                    else
                    {
                        dirY += Vector2.down;
                    }
                }
                else
                {
                    dirY = Vector2.zero;
                }

                //rigid.velocity = Vector3.Normalize(dirX+dirY) * speed;
                rigid.velocity = (dirX * speedX) + (dirY * speedY);
                PlutobiManager.PlutobiSoundManager.Instance.ContinuePlaySFX("발걸음");
            }
            else
            {
                ani.SetBool("isMove", false);
                rigid.velocity = Vector2.zero;
                status = STATUS.IDLE;
            }
        }
    }

    public void GetUpStart()
    {
        status = STATUS.IDLE;
        ani.SetTrigger("isGetUp");
    }
    public void GetUpEnd()
    {
        ClickDeleReg();
    }

    public STATUS GetStatus() { return status; }
    public void SetStatus(STATUS _status) { status = _status; }
    public List<InteractionObj> GetInterObjList() { return interObjs; }
    public void SetInteractionAniType(INTERACTION_TYPE _type) {
        ani.SetInteger("interactionType", (int)_type);
        ani.SetBool("isInteraction", true);
    }

    //나중에 말풍선 모양들 효과 추가 해주면 될듯
    public void State_The_Balloon()
    {
        //걍 한번만 소환 시키는것이 나을듯...
        if (itemList.Count == 0)
        {
            //말풍선 꺼지면서.....
            spriteRenderer_Item.sprite = null;
            spriteRenderer_Balloon.gameObject.SetActive(false);
            return;  //여기서 마치기
        }

        else if (itemList.Count != 0)
        {

            Sprite sprite = (Sprite)PlayManager.Instance.ItemDb.dataList.Find(_x => _x.valueObject == itemList[0]).valueObject;
            spriteRenderer_Item.sprite = sprite;
            spriteRenderer_Balloon.gameObject.SetActive(true);
        }
    }

    //
    public List<Sprite> ItemList
    {
        get { return itemList; }
        set { itemList = value; }
    }

    /// <summary>
    /// 특정 시점에 플레이어에게 
    /// 등록된 델리게이트를 실행하기 위한 함수
    /// </summary>
    /// <param name="_target"></param>
    public void MainCharDelStart()
    {
        if (mainCharDel != null)
            mainCharDel();
        mainCharDel = null;
    }

    public void SetCanMove(bool stat)
    {
        canmove = stat;
    }
}
