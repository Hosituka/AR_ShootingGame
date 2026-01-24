
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using AttitudeSensor = UnityEngine.InputSystem.AttitudeSensor;

public class Title1UI_Manager : MonoBehaviour
{
    public static Title1UI_Manager Current;
    void Awake()
    {
        if (Current == null)
        {
            Current = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    [Header("Title1UI")]
    
    [SerializeField]GameObject _getWebCamButtonObj;
    [SerializeField]GameObject _loadTitle2ButtonObj;

    [SerializeField]TextMeshProUGUI _notifierTextToUser;


    [Header("WebCamSettingUI")]
    [SerializeField]ShowWebCamera _showWebCamera;
    [SerializeField]TextMeshProUGUI _webCumNumberText;

    [SerializeField]GameObject _changeWebCamsObj ;  

    [SerializeField]phase _currentPhase;
    enum phase
    {
        idle,
        enablingAttitudeSensor,
        waitingForVertical,
        completed 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(OneShot());
        IEnumerator OneShot()
        {
            while(Application.HasUserAuthorization(UserAuthorization.WebCam) == false)
            {
                _notifierTextToUser.text = "カメラの許可をしてください";
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
                _notifierTextToUser.text = "";
            }
        }

    }
    public void StartAR_GameButton()
    {
        StartCoroutine(Main());
        IEnumerator Main()
        {
            yield return new WaitWhile(() => Application.HasUserAuthorization(UserAuthorization.WebCam) == false);
            if(AttitudeSensor.current != null)
            {
                _notifierTextToUser.text = "姿勢センサーを有効化しています。";
                InputSystem.EnableDevice(AttitudeSensor.current);
                yield return new WaitWhile(() => AttitudeSensor.current.enabled == false);
                yield return new WaitForSecondsRealtime(2f);

            }
            _notifierTextToUser.text = "背面カメラを取得中";
            yield return new WaitForEndOfFrame();
            DebugUI_manager.DebugUI_Manager.UpdateCameraDeviceLength(WebCamTexture.devices.Length.ToString());
            foreach (WebCamDevice webCamDevice in WebCamTexture.devices)
            {
                if (webCamDevice.isFrontFacing == false)
                {
                    GameManager.Current.WebCamTexture = new WebCamTexture(webCamDevice.name);
                    break;
                }
                GameManager.Current.CurrentWebCamIndex++;
            }

            if (GameManager.Current.WebCamTexture == null)
            {
                GameManager.Current.CurrentWebCamIndex = 0;
                int currentWebCamIndex = GameManager.Current.CurrentWebCamIndex;
                currentWebCamIndex = (int)Mathf.Repeat(1, WebCamTexture.devices.Length);
                GameManager.Current.WebCamTexture = new WebCamTexture(WebCamTexture.devices[currentWebCamIndex].name);
                GameManager.Current.CurrentWebCamIndex = currentWebCamIndex;
                _showWebCamera.StartShowWebCam();

                _notifierTextToUser.text = "背面カメラを検出出来ず、代わりに<br>１のカメラを表示しました。背面<br>カメラでないなら変更を押してください";
                _getWebCamButtonObj.SetActive(false);
                _loadTitle2ButtonObj.SetActive(true);
                _changeWebCamsObj.SetActive(true);
            }
            else
            {
                _notifierTextToUser.text = "";
                GameManager.Current.ResetRotation();
                GameManager.Current.LoadTitle2();
            }
        }

    }
    public void LoadTitle2Button()
    {
        GameManager.Current.ResetRotation();
        GameManager.Current.LoadTitle2();
    }


    public void ChangeWebCamIndexButton()
    {
        int currentWebCamIndex = GameManager.Current.CurrentWebCamIndex;
        currentWebCamIndex++;
        currentWebCamIndex = (int)Mathf.Repeat(currentWebCamIndex, WebCamTexture.devices.Length);
        if (GameManager.Current.WebCamTexture != null)
        {
            GameManager.Current.WebCamTexture.Stop();
            Destroy(GameManager.Current.WebCamTexture);
        }
        GameManager.Current.WebCamTexture = new WebCamTexture(WebCamTexture.devices[currentWebCamIndex].name, Screen.width, Screen.height, 30);
        _showWebCamera.StartShowWebCam();
        _webCumNumberText.text = GameManager.Current.CurrentWebCamIndex.ToString();
        GameManager.Current.CurrentWebCamIndex = currentWebCamIndex;
        _notifierTextToUser.text = "";
    }

}
