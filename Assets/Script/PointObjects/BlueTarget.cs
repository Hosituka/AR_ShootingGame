using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTarget : PointObject
{
    [Header("BlueTargetの設定用プロパティ")]
    [SerializeField]bool _isDestruction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override (float nextBaseActivationDelay,float lifeTime) Initialize()
    {
        ActivateMain();
        return (FourthNote,FourthNote * 5);
    }
    public override void TimeOver()
    {
        StageManager.Current.AddOverlookCount(1);
    }

    // Update is called once per frame
    void Update()
    {

    }
    int _collisionCount;
    void OnCollisionEnter(Collision collision)
    {
        _collisionCount++;
        if(_collisionCount != 1) return;
        if(_isDestruction == true) return;
        if (collision.gameObject.CompareTag("RedBullet"))
        {
            StageManager.Current.AddAccidentalShoot(1);
            StageManager.Current.ResetCombo();
        }
        else if(collision.gameObject.CompareTag("BlueBullet"))
        {
            _isDestruction = true;
            StageManager.Current.AddCombo();
            switch (TargetTimeKeeper.CurrentTaimingState)
            {
                case TimingState.GoodTiming:
                StageManager.Current.AddScore(0.4f,TimingState.GoodTiming);
                break;
                case TimingState.GreatTiming:
                StageManager.Current.AddScore(0.7f,TimingState.GreatTiming);
                break;
                case TimingState.PerfectTiming:
                StageManager.Current.AddScore(1,TimingState.PerfectTiming);
                break;
            }
            StartCoroutine(BreakCoroutine());
        }
    }
    void OnCollisionExit(Collision collision)
    {
        _collisionCount--;
    }
    protected override IEnumerator BreakCoroutine()
    {
        PointObjectGenerater2.CurrentPointObjectGenerater2.SubtractSumPointObjectCost(PointObjectCost);
        PointObjectGenerater2.CurrentPointObjectGenerater2.RemovePointObjectPos(PointObjectPos,2);
        TargetTimeKeeper.NoticeDestruction(this);

        List<TMPandMeshRenderer> disableTMPandMeshRenderers = FadeTargetList;
        disableTMPandMeshRenderers[0].MeshRendererList.Add(LifeTimeGUI_MR);
        _targetBreakAnimator.PlaySpinThenExplode(transform.position,Color.blue,12,disableTMPandMeshRenderers);
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtSpinThenExplodePhase != BreakAnimator.SpinThenExplodePhase.Explosion);
        Utility.ChangeEnabledColliders(ColliderList,false); 
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtSpinThenExplodePhase != BreakAnimator.SpinThenExplodePhase.Completed);
        Destroy(gameObject);
    }

}
