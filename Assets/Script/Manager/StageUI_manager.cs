using System;
using TMPro;
using UnityEngine;
using Audio;
public class StageUI_manager : MonoBehaviour
{
    public static StageUI_manager Current;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    [Header("#通常のUIに関するメンバ")]
    [SerializeField]ShowWebCamera _showWebCamera;
    [SerializeField] GameObject _indicatorToTarget;
    [SerializeField] GameObject _indicatorsUIObj;
    [SerializeField] TextMeshProUGUI _timeTextTM;
    [SerializeField] ScoreValueText _scoreValueText;
    [SerializeField] TextMeshProUGUI _accidentalShootTextTM;
    [SerializeField] GameObject _addScoreTextObj;
    [SerializeField] TextMeshProUGUI _comboTextTM;
    [SerializeField] GameObject _scoreMultiplierTextObj;
    [SerializeField] GameObject _explosionEffectObj;
    [SerializeField] Transform _standardUI_Tr;
    [SerializeField,Header("##判定を表すUIに関するメンバ")]
    GameObject _goodTextPrefab;
    [SerializeField]GameObject _greatTextPrefab;
    [SerializeField]GameObject _perfectTextPrefab;


    [SerializeField,Header("#ゲームクリアUIに関するメンバ")]
    GameObject _gameClearUIObj;
    [SerializeField] GameObject _indicatorToClear;
    [SerializeField] TextMeshPro _breakableCountTM;
    [SerializeField] TextMeshPro _perfectCountTM;
    [SerializeField] TextMeshPro _greatCountTM;
    [SerializeField] TextMeshPro _goodCountTM;
    [SerializeField] TextMeshPro _overlookCountTM;
    [SerializeField] TextMeshPro _totalScoreTM;
    [SerializeField] TextMeshPro _accidentalShoot;



    void Start()
    {
        _showWebCamera.StartShowWebCam();
        GameManager.Current.StartFadeIn();
        _gameClearUIObj.SetActive(false);
        _indicatorToClear.SetActive(false);
        _indicatorToTarget.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateScoreText(double score)
    {
        score = Math.Round(score, 1);
        _scoreValueText.UpdateText(score.ToString());
    }
    public void UpdateAccidentalShootText(double miss)
    {
        _accidentalShootTextTM.text = Math.Round(miss, 1).ToString() + "：誤射";
    }
    public void UpdateTimeText(double time)
    {
        _timeTextTM.text = "時間：" + Math.Round(time, 1).ToString();
    }
    public void UpdateComboText(int combo)
    {
        _comboTextTM.text = combo.ToString();
    }
    public void GenerateScoreMultiplierText(int __scoreMultiplier)
    {
        Debug.Log("スコア倍率増加");
        Vector3 playerPos = Player.Current.transform.position;
        GameObject gameObject = Instantiate(_scoreMultiplierTextObj);
        gameObject.transform.position = Player.Current.transform.forward * 30;
        gameObject.transform.rotation = Quaternion.LookRotation(playerPos - gameObject.transform.position);
        gameObject.GetComponent<ScoreMultiplierText1>().ScoreMultiplier = __scoreMultiplier;
    }
    public void GenerateAddScoreText(float addScore)
    {
        GameObject gameObject = Instantiate(_addScoreTextObj, _standardUI_Tr);
        gameObject.GetComponent<AddScoreText>().AddScore = addScore;
    }
    public ExplosionEffect GenerateExplosionEffect(Vector3 pos,Color color,float size)
    {
        GameObject gameObject = Instantiate(_explosionEffectObj,pos,Quaternion.LookRotation(pos));
        gameObject.transform.localScale = new Vector3(size,size,1);
        ExplosionEffect explosionEffect = gameObject.GetComponent<ExplosionEffect>();
        explosionEffect.BaseColor = color;
        return explosionEffect;
    }
    public void GenerateJudgeTexts(TimingState timing)
    {
        switch (timing)
        {
            case TimingState.GoodTiming:
            Instantiate(_goodTextPrefab,_standardUI_Tr);
            break;
            case TimingState.GreatTiming:
            Instantiate(_greatTextPrefab,_standardUI_Tr);
            break;
            case TimingState.PerfectTiming:
            Instantiate(_perfectTextPrefab,_standardUI_Tr);
            break;
        }
    }
    public void ShowGameClearUI(double score, double accidentalShoot,int perfectCount,int greatCount,int goodCount,int overlookCount)
    {
        score = Math.Round(score,1);
        accidentalShoot = Math.Round(accidentalShoot,1);
        _gameClearUIObj.SetActive(true);
        _gameClearUIObj.transform.RotateAround(Player.Current.transform.position, Vector3.up, Player.Current.transform.rotation.eulerAngles.y);
        _indicatorToClear.SetActive(true);

        _perfectCountTM.text = perfectCount.ToString();
        _greatCountTM.text = greatCount.ToString();
        _goodCountTM.text = goodCount.ToString();
        _overlookCountTM.text = overlookCount.ToString();
        _breakableCountTM.text = (overlookCount + goodCount + greatCount + perfectCount).ToString();

        _accidentalShoot.text = accidentalShoot.ToString();
        _totalScoreTM.text = (score - accidentalShoot).ToString();
    }
    public Indicator2 GenerateIndicatorToTarget(Transform targetTr)
    {
        GameObject indicator2Obj = Instantiate(_indicatorToTarget);
        Indicator2 indicator2 = indicator2Obj.GetComponent<Indicator2>();
        indicator2.TargetTr = targetTr;
        indicator2Obj.transform.SetParent(_indicatorsUIObj.transform);
        indicator2Obj.transform.SetSiblingIndex(0);
        indicator2Obj.SetActive(true);
        return indicator2;
    }
    public void RestartButton()
    {
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
        GameManager.Current.ReloadCurrentScene();
    }
    public void GoBackTitle2Button()
    {
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
        GameManager.Current.LoadTitle2();
    }
    public void LoadStageButton(string stageName)
    {
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
        GameManager.Current.LoadScene(stageName);
    }
    public void ResetRotationButton()
    {
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
        GameManager.Current.ResetRotation();
    }

    public void ShowSettingsButton()
    {
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
        Time.timeScale = 0;
    }
    public void CloseSettingsButton()
    {
        SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
        Time.timeScale = 1;
    }
    public void RestartShootableButton()
    {
        GameManager.Current.ReloadCurrentScene();
    }
    public void GoBackTitleShootableButton()
    {
        GameManager.Current.LoadTitle2();
    }
    public void LoadStageShootableButton(string stageName)
    {
        GameManager.Current.LoadScene(stageName);
    }

}
    public enum TimingState
    {
        //パーフェクト判定となる期間
        PerfectTiming,
        //グレート判定となる期間
        GreatTiming,
        //グッド判定となる期間
        GoodTiming,

    }
