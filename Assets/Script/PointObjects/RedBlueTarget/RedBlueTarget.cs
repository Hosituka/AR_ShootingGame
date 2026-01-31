using UnityEngine;
using System.Collections;

public class RedBlueTarget : PointObject
{
    [Header("RedBlueTargetの設定用プロパティ")]
    [SerializeField]GameObject _redPlane;
    [SerializeField]GameObject _bluePlane;
    //レッドプレーン＋ブループレンの個数
    int _currentPlaneCount = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override (float nextBaseActivationDelay,float lifeTime) Initialize()
    {
        ActivateMain();
        return (FourthNote,FourthNote * 5);
    }
    public override void TimeOver()
    {
        StageManager.Current.AddOverlookCount(_currentPlaneCount);
    }

    // Update is called once per frame
    public void DecrementCurrentPlaneCount()
    {
        _currentPlaneCount--;
        if(_currentPlaneCount == 0)
        {StartCoroutine(BreakCoroutine());}

    }
    protected override IEnumerator BreakCoroutine()
    {
        PointObjectGenerater2.CurrentPointObjectGenerater2.SubtractSumPointObjectCost(PointObjectCost);
        PointObjectGenerater2.CurrentPointObjectGenerater2.RemovePointObjectPos(PointObjectPos,2);
        TargetTimeKeeper.NoticeDestruction(this);

        FadeTargetList[0].MeshRendererList.Add(LifeTimeGUI_MR);
        _targetBreakAnimator.PlaySpinThenFadeOut(FadeTargetList);
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtSpinThenFadeOutPhase != BreakAnimator.SpinThenFadeOutPhase.Completed);
        Destroy(gameObject);
    }


}
