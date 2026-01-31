using UnityEngine;
using System.Collections;
public class MoveRedTarget : PointObject
{
    [Header("MoveRedTargetの設定用プロパティ")]

    public float IntervalForMove = 1;

    Transform _playerTr;
    bool _isVerticalToRotate;
    Vector3 _directionToPlayer;
    float _distanceForGenerate;
    float _remapPingPong;
    int _generateYawStep;
    int _generatePitchStep;
    float _startGenerateYaw;
    float _startGeneratePitch;
    float _pingPongTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override (float nextBaseActivationDelay,float lifeTime) Initialize()
    {
        _playerTr = Player.Current.GetComponent<Transform>();
        _isVerticalToRotate = Random.Range(0, 2) == 1;
        _distanceForGenerate = PointObjectGenerater2.CurrentPointObjectGenerater2.DistanceForGenerate;
        _generateYawStep = PointObjectGenerater2.CurrentPointObjectGenerater2.GenerateYawStep;
        _generatePitchStep = PointObjectGenerater2.CurrentPointObjectGenerater2.GeneratePitchStep;
        _startGenerateYaw = PointObjectGenerater2.CurrentPointObjectGenerater2.GetYawPitch(transform.position).yaw;
        _startGeneratePitch = PointObjectGenerater2.CurrentPointObjectGenerater2.GetYawPitch(transform.position).pitch;
        if (_isVerticalToRotate)
        {
            _pingPongTime = 1 * IntervalForMove / 2; 
        }
        else
        {
            _pingPongTime = 1 * IntervalForMove / 2;
        }
        return (FourthNote,FourthNote * 5);
    }
    public override void TimeOver()
    {
        StageManager.Current.AddOverlookCount(1);
    }

    // Update is called once per frame
    void Update()
    {
        if(TargetTimeKeeper == null)return;
        if(TargetTimeKeeper.CurrentTargetState != TimeKeeper.TargetState.ActivationCompleted) return;

        _pingPongTime += Time.deltaTime;
        if (_isVerticalToRotate)//ローカル座標系のy軸方向にRotateAroundさせる
        {
            _remapPingPong = Mathf.PingPong(_pingPongTime / IntervalForMove, 1) * 2 - 1; 
            transform.position = Quaternion.AngleAxis(_startGenerateYaw + _remapPingPong * _generateYawStep, Vector3.up) * Vector3.forward;
            transform.position = Quaternion.AngleAxis(_startGeneratePitch,Vector3.Cross(Vector3.up,transform.position)) * transform.position;

        }
        else//ローカル座標系のx軸方向にRotateAroundさせる。
        {
            _remapPingPong = Mathf.PingPong(_pingPongTime / IntervalForMove, 1) * 2 - 1; 
            transform.position = Quaternion.AngleAxis(_startGenerateYaw, Vector3.up) * Vector3.forward;
            transform.position = Quaternion.AngleAxis(_startGeneratePitch + _remapPingPong * _generatePitchStep, Vector3.Cross(Vector3.up,new Vector3(transform.position.x,0,transform.position.z))) * transform.position;
        }
        transform.position *= _distanceForGenerate - 3;
        _directionToPlayer = _playerTr.position - transform.position;
        transform.rotation = Quaternion.LookRotation(_directionToPlayer);

    }
    int _collisionCount;
    void OnCollisionEnter(Collision collision)
    {
        _collisionCount++;
        if(_collisionCount != 1) return;
        if (collision.gameObject.CompareTag("BlueBullet"))
        {
            StageManager.Current.AddAccidentalShoot(1);
            StageManager.Current.ResetCombo();
        }
        else if(collision.gameObject.CompareTag("RedBullet"))
        {
            StageManager.Current.AddCombo();
            switch (TargetTimeKeeper.CurrentTaimingState)
            {
                case TimingState.GoodTiming:
                StageManager.Current.AddScore(0.5f,TimingState.GoodTiming);
                break;
                case TimingState.GreatTiming:
                StageManager.Current.AddScore(1,TimingState.GreatTiming);
                break;
                case TimingState.PerfectTiming:
                StageManager.Current.AddScore(1.5f,TimingState.PerfectTiming);
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
        
        Utility.ChangeEnabledColliders(ColliderList,false);
        FadeTargetList[0].MeshRendererList.Add(LifeTimeGUI_MR);
        _targetBreakAnimator.PlaySpinAndFadeOut(FadeTargetList);
        yield return new WaitWhile(()=> _targetBreakAnimator.CurtSpinAndFadeOutPhase != BreakAnimator.SpinAndFadeOutPhase.Completed);
        Destroy(gameObject);
    }

}
