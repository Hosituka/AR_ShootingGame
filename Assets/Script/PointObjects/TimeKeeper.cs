using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

//ある時刻におけるpointObject群のライフサイクルを管理するクラスである。pointObjectGeneraterとの連携も行っている。
public class TimeKeeper : MonoBehaviour
{
    //またPointObjectGeneratorのインスタンスとの連携も担っています。
    [Header("インスペクター設定用")]
    public List<PointObject> TargetPointObjectList;
    [SerializeField,Range(0,1)] float _activateAnimRate = 0.2f;
    [SerializeField]float _deactivateAnimDuration = 0.2f;
    
    [SerializeField] float _perlinNoiseMagni = 1;

    [Header("表示用")]
    //PointObjectGeneraterにより設定される基準となる有効化の遅延時間
    public float BaseActivationDelay;
    public TargetState CurrentTargetState = TargetState.Preparing;
    public TimingState CurrentTaimingState = TimingState.GoodTiming;
    [SerializeField]float _sumLifeTime;
    [SerializeField]float _sumNextActivationDelay;
    [SerializeField] int _nextGeneratableCount;
    //BaseActivationDelayとPointObjectが持つoffsetActivationDelayにより最終的に決まる有効化の遅延時間
    [SerializeField] float _activationDelay;

    public enum TargetState
    {
        //有効化される前の状態
        Preparing,
        //有効化中の状態
        Activating,
        //有効化完了の状態
        ActivationCompleted,
        //無効化中の状態
        Deactivating
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ManageLifeCycle());
        IEnumerator ManageLifeCycle()
        {
            AllInitializePointObject();
            //対応するポイントオブジェクトの有効化に要する時間を計算
            CalculateActivationDelay();
            //対応するポイントオブジェクトの有効化までの時間可視化する関数を実行
            AllPlayAddLifeTimeGUI(_activationDelay);
            //ポイントオブジェクトの有効化時の出現アニメーションの所要時間を考慮して待機
            yield return new WaitForSeconds(_activationDelay - _activateAnimRate *_activationDelay);
            CurrentTargetState = TargetState.Activating;
            CurrentTaimingState = TimingState.GoodTiming;
            //ポイントオブジェクトの有効化処理と出現アニメーションの再生
            AllActivateMain();
            AllPlayActivateAnim(_activateAnimRate * _activationDelay);
            //出現アニメーションの所要時間分待機
            yield return new WaitForSeconds(_activateAnimRate * _activationDelay);
            CurrentTargetState = TargetState.ActivationCompleted;
            CurrentTaimingState = TimingState.PerfectTiming;
            AllAddPointObjectCost();
            NoticeGeneratableNextPointObject();
            AllPlaySubtractLifeTimeGUI(_sumLifeTime);
            yield return new WaitForSeconds(_sumLifeTime * 0.5f);
            CurrentTaimingState = TimingState.GreatTiming;
            yield return new WaitForSeconds(_sumLifeTime * 0.25f);
            CurrentTaimingState = TimingState.GoodTiming;
            yield return new WaitForSeconds(_sumLifeTime * 0.25f);
            CurrentTargetState = TargetState.Deactivating;
            //デスポーンアニメーションの開始
            AllDeactivatePointObject();
            AllPlayDeactivateAnim(_deactivateAnimDuration);
            yield return new WaitForSeconds(_deactivateAnimDuration);
            Destroy(gameObject);
        }
        void AllInitializePointObject(){
            _nextGeneratableCount =TargetPointObjectList[0].NextGeneratableCount;
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.TargetTimeKeeper = this;
                (float nextActivationDelay,float lifeTime) = targetPointObject.Initialize();
                _sumNextActivationDelay += nextActivationDelay;
                _sumLifeTime += lifeTime;
            }

        }

        void CalculateActivationDelay()
        {
            _activationDelay = BaseActivationDelay;
            foreach(PointObject targetPointObject in TargetPointObjectList){
                _activationDelay += targetPointObject.OffsetActivationDelay;
            }

        }
        void AllPlayAddLifeTimeGUI(float activationDelay){
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.PlayAddLifeTimeGUI(_activationDelay);
            }
        }
        void AllActivateMain()
        {
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.ActivateMain();
            }

        }
        
        void AllPlayActivateAnim(float activateAnimDuration){
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.PlayActivateAnim(activateAnimDuration);
            }
        }
        void AllAddPointObjectCost(){
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                PointObjectGenerater2.CurrentPointObjectGenerater2.AddSumPointObjectCost(targetPointObject.PointObjectCost);
            }
        }
        void AllPlaySubtractLifeTimeGUI(float sumLifeTime){
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.PlaySubtractLifeTimeGUI(sumLifeTime);
            }
        }
        void AllDeactivatePointObject(){
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.TimeOver();
            }
        }
        void AllPlayDeactivateAnim(float deactivateAnimDuration){
            foreach(PointObject targetPointObject in TargetPointObjectList){
                if(targetPointObject == null) continue;
                targetPointObject.PlayTimeOverAnim(deactivateAnimDuration);
            }
        }
    }
    //PointObjectGeneratorに次の生成を指定するメンバ
    public void NoticeGeneratableNextPointObject()
    {
        PointObjectGenerater2.CurrentPointObjectGenerater2.NoticeGeneratable(_sumNextActivationDelay,0,_perlinNoiseMagni,_nextGeneratableCount);
    }
    //PointObjectがTimeKeeperの管理から外れる為の関数メンバ
    public void NoticeDestruction(PointObject pointObject)
    {
        TargetPointObjectList.Remove(pointObject);
        pointObject.TargetIndicator2.Destroy();
        //管理外から外れる為　対象のポイントオブジェクトとのリンクを切る。
        pointObject.TargetTimeKeeper = null;
    }


    // Update is called once per frame
}
