using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HandCameraCtrl : MonoBehaviour
{
    private GameObject player;

    public Inventory inventory;

    public Camera mainCamera;
    public Camera photoCamera;

    public RawImage cameraFilter;
    public Image photoButton;
    public GameObject photoBook;

    private GameObject grabPoint;
    private GameObject shootPoint;

    private bool cameraMode;
    private float cameraCoolTime;
    private float curCamCoolTime;

    private float xRotate, yRotate, xRotateMove, yRotateMove;
    public float rotateSpeed = 100.0f;
    private bool initRot = true;

    //Fade ==================================================

    public float animTime = 0.5f;         // Fade �ִϸ��̼� ��� �ð� (����:��).  
    public Image fadeImage;            // UGUI�� Image������Ʈ ���� ����.  

    private float start = 1f;           // Mathf.Lerp �޼ҵ��� ù��° ��.  
    private float end = 0f;             // Mathf.Lerp �޼ҵ��� �ι�° ��.  
    private float time = 0f;            // Mathf.Lerp �޼ҵ��� �ð� ��.  

    public bool stopIn = true; //false�϶� ����Ǵ°ǵ�, �ʱⰪ�� false�� �� ������ ���� �����Ҷ� ���̵������� ������...�װ� ������ true�� �ϸ��.
    public bool stopOut = true;

    //=======================================================

    [SerializeField]
    LayerMask layerMask;

    private bool textActivate;
    public Text itemText;
    public Text clickToTakePictureText;

    private AudioMgr audioManager;
    private AudioSource audioSource = null;

    private void Awake()
    {
        grabPoint = GameObject.FindGameObjectWithTag("EquipPoint");
        shootPoint = GameObject.FindGameObjectWithTag("ShootPoint");
        player = GameObject.FindGameObjectWithTag("Player");
        audioManager = GameObject.FindGameObjectWithTag("Audio Manager").GetComponent<AudioMgr>();
        audioSource = GameObject.FindGameObjectWithTag("Audio Manager").GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraMode = false;
        cameraCoolTime = 3;
        curCamCoolTime = cameraCoolTime;
        textActivate = false;
        MainCameraView();
    }

    // Update is called once per frame
    void Update()
    {
        CarmeraMode();
        RotateCamera();
        TryTakePicture();
        SetTextActivation();
    }

    void CarmeraMode()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!cameraMode)
            {
                this.transform.SetParent(shootPoint.transform);
                this.transform.localPosition = new Vector3(0, (float)-0.125, 0);
                this.transform.rotation = new Quaternion(0, 0, 0, 0);
                this.transform.Rotate(0, 180, 0);
                cameraMode = true;
                initRot = true;
                PhotoCameraView();
            }
            else
            {
                this.transform.SetParent(grabPoint.transform);
                this.transform.localPosition = new Vector3(0, (float)-0.125, 0);
                this.transform.rotation = new Quaternion(0, 0, 0, 0);
                curCamCoolTime = cameraCoolTime;
                cameraMode = false;
                MainCameraView();
            }
        }
    }

    public void MainCameraView()
    {
        photoCamera.enabled = false;
        mainCamera.enabled = true;
        cameraFilter.enabled = false;
        Cursor.visible = true;
        photoButton.enabled = true;
    }

    public void PhotoCameraView()
    {
        photoCamera.enabled = true;
        mainCamera.enabled = false;
        cameraFilter.enabled = true;
        Cursor.visible = false;
        photoButton.enabled = false;
        photoBook.SetActive(false);
    }

    public void RotateCamera()
    {
        if(cameraMode)
        {
            if(initRot)
            {
                xRotate = 0;
                yRotate = 180 + player.transform.eulerAngles.y;
                initRot = false;
            }    

            xRotateMove = Input.GetAxis("Mouse Y") * Time.deltaTime * rotateSpeed;
            yRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed;


            yRotate = yRotate + yRotateMove;
            xRotate = xRotate + xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -90, 90); // ��, �Ʒ� ����
            yRotate = Mathf.Clamp(yRotate, 90 + player.transform.eulerAngles.y, 270 + player.transform.eulerAngles.y);
            
            transform.eulerAngles = new Vector3(xRotate, yRotate, 0);
        }
    }

    public void TryTakePicture()
    {
        
        if(cameraMode)
        {
            
            if (Input.GetMouseButtonDown(0) && curCamCoolTime <= 0)
            {
                TakeShot();
                curCamCoolTime = cameraCoolTime;
                StartCoroutine(this.PlayFadeIn());
            }
            ScanItem();
        }

        if (curCamCoolTime > 0)
            curCamCoolTime -= Time.deltaTime;
    }

    public void TakeShot()
    {
        audioSource.PlayOneShot(audioManager.sfx.shutterSound, 0.9f);
        Debug.DrawRay(this.transform.position, -transform.forward * 10, Color.red, 1f);

        RaycastHit hit;

        if(Physics.Raycast(this.transform.position, -transform.forward, out hit, 100f, layerMask))
        {
            GameObject itemObject = hit.collider.gameObject;
            Item item = itemObject.GetComponent<Item>();

            if(itemObject.transform.name == "endScene")
            {
                LoadScene();
                return;
            }

            if (inventory.AddItem(item))
            {
                if(item.GetItemName() != "Cloud" && item.GetItemName() != "Mushroom")
                    itemObject.SetActive(false);
                Debug.Log("Item Successfully Added!");

            }

        }
    }

    public void ScanItem()
    {
        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, -transform.forward, out hit, 100f, layerMask))
        {
            GameObject itemObject = hit.collider.gameObject;

            if (itemObject.transform.name != "endScene")
            {
                textActivate = true;
                Item item = hit.collider.gameObject.GetComponent<Item>();

                itemText.text = "<color=#ff0000>" + item.GetItemName() + "</color>";
            }

        }
        else
            textActivate = false;
    }

    public void SetTextActivation()
    {
        if(cameraMode && textActivate)
        {
            itemText.enabled = true;
            clickToTakePictureText.enabled = true;
        }
        else
        {
            itemText.enabled = false;
            clickToTakePictureText.enabled = false;
        }
    }

    IEnumerator PlayFadeIn()
    {
        while (true)
        {
            if(time >= animTime)
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

    public void LoadScene()
    {
        SceneManager.LoadScene("Ending");
    }

    public bool getCamMode() { return cameraMode; }
}
