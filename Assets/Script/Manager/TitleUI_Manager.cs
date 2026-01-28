using UnityEngine;
using Audio;

public class TitleUI_Manager : MonoBehaviour
{
    public static TitleUI_Manager current;
    void Awake()
    {
        if (current == null)
        {
            current = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    [SerializeField]AR_BackGround _aR_BackGround;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _aR_BackGround.StartShowWebCam();
        GameManager.Current.StartFadeIn();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadSceneButton(string __stageName)
    {
        GameManager.Current.LoadScene(__stageName);
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
    }
    public void ResetRotationButton()
    {
        GameManager.Current.ResetRotation();
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
    }

}
