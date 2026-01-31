using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSwapTarget : PointObject,IHitGunSwapRayHandler
{
    [Header("GunSwapTargetの設定用プロパティ")]
    public int Test;
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
        if (collision.gameObject.CompareTag("BlueBullet") || collision.gameObject.CompareTag("RedBullet"))
        {
            StageManager.Current.AddAccidentalShoot(1);
            StageManager.Current.ResetCombo();
        }
    }
    void OnCollisionExit(Collision collision)
    {
        _collisionCount--;
    }

    public void OnHitGunSwapRay()
    {
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
            StageManager.Current.AddScore(1f,TimingState.PerfectTiming);
            break;
        }
        StartCoroutine(BreakCoroutine());
    }
    protected override IEnumerator BreakCoroutine()
    {
        PointObjectGenerater2.CurrentPointObjectGenerater2.SubtractSumPointObjectCost(PointObjectCost);
        PointObjectGenerater2.CurrentPointObjectGenerater2.RemovePointObjectPos(PointObjectPos,2);
        TargetTimeKeeper.NoticeDestruction(this);

        Utility.ChangeEnabledColliders(ColliderList,false);
        List<TMPandMeshRenderer> disableTMPandMeshRenderers = FadeTargetList;
        disableTMPandMeshRenderers[0].MeshRendererList.Add(LifeTimeGUI_MR);
        _targetBreakAnimator.PlayExplosion(transform.position,Color.gray,12,disableTMPandMeshRenderers);
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtExplosionPhase != BreakAnimator.ExplosionPhase.Completed);
        Destroy(gameObject);
    }

}
