using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

public class LineUpTarget : PointObject
{
    [Header("LineUpTargetの設定用プロパティ")]
    public GameObject[] PlaneForLineUpObj;
    public Transform AxisTr;
    public int MaxPlaneCount = 7;
    public int MinPlaneCount = 5;
    [Header("表示用")]
    [SerializeField] float _nextShowPlaneInterval;
    [SerializeField] GameObject[] _setPlanes;
    [SerializeField] int _planeCount;
    float _pitchRotationStep;
    float _rotationInterval;
    TextMeshPro[] _needShotCountTexts;
    GameObject _generatePlaneObj;
    GameObject _generatedPlaneObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override (float nextBaseActivationDelay,float lifeTime) Initialize()
    {
        _planeCount = Random.Range(MinPlaneCount, MaxPlaneCount + 1);
        PlaneForLineUp instancePlaneForLineUp;
        _setPlanes = new GameObject[_planeCount];
        _needShotCountTexts = new TextMeshPro[_planeCount];

        _pitchRotationStep = 360f / _planeCount;
        for (int generatedCount = 0; generatedCount < _planeCount; generatedCount++)
        {
            _generatePlaneObj = PlaneForLineUpObj[Random.Range(0, PlaneForLineUpObj.Length)];
            _generatedPlaneObj = Instantiate(_generatePlaneObj, _generatePlaneObj.transform.position, AxisTr.rotation);
            instancePlaneForLineUp = _generatedPlaneObj.GetComponent<PlaneForLineUp>();
            ColliderList.AddRange(instancePlaneForLineUp.ColliderArray);
            FadeTargetList.AddRange(instancePlaneForLineUp.fadeTargetList);
            _generatedPlaneObj.transform.SetParent(AxisTr);
            _generatedPlaneObj.transform.rotation = Quaternion.AngleAxis(generatedCount * _pitchRotationStep, transform.up) * AxisTr.rotation;
            _needShotCountTexts[generatedCount] = instancePlaneForLineUp.NeedShotCountTMPro;
            _needShotCountTexts[generatedCount].text = _planeCount.ToString();
            _generatedPlaneObj.SetActive(true);
            _setPlanes[generatedCount] = _generatedPlaneObj;
        }
        switch (GameManager.Current.CurrentDifficult)
        {
            case GameManager.Difficult.easy:
            _nextShowPlaneInterval = FourthNote;
            OffsetActivationDelay = FourthNote * 2;
            break;
            case GameManager.Difficult.normal:
            _nextShowPlaneInterval = FourthNote;
            OffsetActivationDelay = FourthNote * 2;
            break;
            case GameManager.Difficult.hard:
            _nextShowPlaneInterval = FourthNote + EighthNote;
            OffsetActivationDelay = FourthNote * 2;
            break;
        }
        SetRotationInterval();
        return (_nextShowPlaneInterval * _planeCount,
               _nextShowPlaneInterval * _planeCount * 2);
    }
    public override void TimeOver()
    {
        StageManager.Current.AddOverlookCount(_planeCount);
    }

    // Update is called once per frame
    void Update()
    {
        if(TargetTimeKeeper == null)return;
        if(TargetTimeKeeper.CurrentTargetState == TimeKeeper.TargetState.Activating) return;
        AxisTr.rotation = Quaternion.AngleAxis(1 / _rotationInterval * Time.deltaTime * 360, transform.up) * AxisTr.rotation;
    }
    void SetRotationInterval()
    {
        _rotationInterval = _nextShowPlaneInterval * _planeCount;
    }

    public void DecrementPlaneCount()
    {
        _planeCount--;
        foreach (TextMeshPro textMeshPro in _needShotCountTexts)
        {
            textMeshPro.text = _planeCount.ToString();
        }

        if (_planeCount == 0)
        {StartCoroutine(BreakCoroutine());}

    }
    protected override IEnumerator BreakCoroutine()
    {
        PointObjectGenerater2.CurrentPointObjectGenerater2.SubtractSumPointObjectCost(PointObjectCost);
        PointObjectGenerater2.CurrentPointObjectGenerater2.RemovePointObjectPos(PointObjectPos,2);
        TargetTimeKeeper.NoticeDestruction(this);

        List<TMPandMeshRenderer> disableTMPandMeshRenderers = FadeTargetList;
        disableTMPandMeshRenderers[0].MeshRendererList.Add(LifeTimeGUI_MR);
        _targetBreakAnimator.PlaySpinThenExplode(transform.position,Color.yellow,18,disableTMPandMeshRenderers);
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtSpinThenExplodePhase != BreakAnimator.SpinThenExplodePhase.Completed);
        Destroy(gameObject);
    }

}
