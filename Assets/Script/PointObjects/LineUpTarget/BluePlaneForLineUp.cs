using UnityEngine;
using System.Collections;
public class BluePlaneForLineUp : PlaneForLineUp
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _dot = Vector3.Dot(transform.right, LineUpTargetTr.right);
        if (_dot > 0 + 0.001)
        {
            if (_isShow == true) return;
            _isShow = true;
            Utility.ChangeEnabledColliders(ColliderArray,true);
            Utility.ChangeEnabledTMPorMeshRenderers(fadeTargetList,true);

        }
        else
        {
            if (_isShow == false) return;
            _isShow = false;
            Utility.ChangeEnabledColliders(ColliderArray,false);
            Utility.ChangeEnabledTMPorMeshRenderers(fadeTargetList,false);
        }
    }
    int _collisionCount;
    void OnCollisionEnter(Collision collision)
    {
        _collisionCount++;
        if(_collisionCount != 1) return;
        if (collision.gameObject.CompareTag("RedBullet"))
        {
            StageManager.Current.AddAccidentalShoot(1);
            StageManager.Current.ResetCombo();
        }
        else if(collision.gameObject.CompareTag("BlueBullet"))
        {
            StageManager.Current.AddCombo();
            
            if(_dot >= 0.95f)
            {StageManager.Current.AddScore(1f,TimingState.PerfectTiming);}
            else if(_dot >= 0.8f)
            {StageManager.Current.AddScore(0.7f,TimingState.GreatTiming);}
            else
            {StageManager.Current.AddScore(0.4f,TimingState.GoodTiming);}

            StartCoroutine(BreakCoroutine());
        }
    }
    void OnCollisionExit(Collision collision)
    {
        _collisionCount--;
    }
    protected override IEnumerator BreakCoroutine()
    {
        Line_UpTarget.DecrementPlaneCount();
        Utility.ChangeEnabledColliders(ColliderArray,false);
        _targetBreakAnimator.PlayExplosion(_effectPivotTr.position,Color.blue,12,fadeTargetList);
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtExplosionPhase != BreakAnimator.ExplosionPhase.Completed);
        Destroy(gameObject);
    }


}
