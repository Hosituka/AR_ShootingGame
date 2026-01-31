using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointObjectGenerater2 : MonoBehaviour
{
    public GameManager.Difficult DifficultAsGenerater;
    [Header("生成に関するパラメータ")]
    public GameObject[] PointObjects;
    public float DistanceForGenerate = 500;
    public int GenerateHalfYawAngle = 40;
    public int GenerateYawStep = 15;

    public int GenerateHalfPitchAngle = 30;
    public int GeneratePitchStep = 20;

    public float PerlinNoiseScale;
    public float PerlinNoiseMagni;
    [Header("難易度ごとの設定")]
    public float MaxPointObjectCost = 10;
    public float Bpm;


    [Header("その他")]
    [SerializeField] GameObject _timeKeeperObj;
    public Transform PlayerTr;
    [Header("表示専用")]
    [SerializeField]bool _isGenerationComplete;
    [SerializeField]float _fourthNote;
    [SerializeField]float _eighthNote;
    [SerializeField]float _sixteenthNote;
    [SerializeField]float _perlinNoiseSeed;
    
    [SerializeField] int _generatableCount = 1;
    [SerializeField]float _sumPointObjectCost;
    enum GeneratePhase
    {

        completed,
    }
    public static PointObjectGenerater2 CurrentPointObjectGenerater2;
    public (int x, int y) PointObjectMapLength;

    float _currentBaseActivationDelay;
    bool[,] _pointObjectMap;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if(DifficultAsGenerater != GameManager.Current.CurrentDifficult)
        {
            Destroy(this.gameObject);
            return;
        }
        if (CurrentPointObjectGenerater2 == null)
        {
            Debug.Log(GameManager.Current.CurrentDifficult);
            CurrentPointObjectGenerater2 = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        PointObjectMapLength.x = (GenerateHalfYawAngle / GenerateYawStep * 2) + 1;
        PointObjectMapLength.y = (GenerateHalfPitchAngle / GeneratePitchStep * 2) + 1;
        _pointObjectMap = new bool[PointObjectMapLength.x, PointObjectMapLength.y];

        SetNotes();
        StartCoroutine(StayFadeIn());
        IEnumerator StayFadeIn()
        {
            yield return new WaitWhile(()=>GameManager.Current.FadeInComplete == false);
            Debug.Log("test2");
            NoticeGeneratable(2f,0.5f,1,1);
        }
    }
    public void NoticeGeneratable(float nextActivationDelay,float delay,float perlinNoiseMagni,int maxGeneratableCount)
    {
        StartCoroutine(OneShot());
        IEnumerator OneShot()
        {
            yield return new WaitForSeconds(delay);            
            _currentBaseActivationDelay = nextActivationDelay;
            PerlinNoiseMagni = perlinNoiseMagni;
            _perlinNoiseSeed += Random.Range(0f,1f) * PerlinNoiseScale * PerlinNoiseMagni;
            _generatableCount = maxGeneratableCount;
            _isGenerationComplete = false;
            while(_isGenerationComplete == false)
            {
                if(enabled == false) yield break;
                GeneratePointObject();
                yield return null;
            }
        }
    }
    void GeneratePointObject()
    {
        
        if(_generatableCount == 0){Debug.LogError("生成可能数が0になっています。"); return;}

        int generatedCount = 0;
        GameObject currentTimeKeeperObj = null;
        TimeKeeper currentTimeKeeper = null;

        while (generatedCount < _generatableCount)
        {
            int genertablePointObjectsIndex = GetGeneratablePointObjectIndex();
            if(genertablePointObjectsIndex == -1){Debug.Log("生成予算オーバーです。");break;}
            Vector2Int pointObjectPos = SearchPointObjectPos();
            if(pointObjectPos == -Vector2Int.one){Debug.Log("生成可能な場所がありません");break;}
            
            if (currentTimeKeeperObj == null)
            {
                currentTimeKeeperObj = Instantiate(_timeKeeperObj);
                currentTimeKeeperObj.transform.SetParent(transform.root);
                currentTimeKeeper = currentTimeKeeperObj.GetComponent<TimeKeeper>();
            }

            //前方ベクトルにランダムなヨー角を適用
            transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis((pointObjectPos.x - (GenerateHalfYawAngle / GenerateYawStep)) * GenerateYawStep, Vector3.up) * Vector3.forward);
            //上によって作られたベクトルにランダムなピッチ角を適用
            transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(-(pointObjectPos.y - (GenerateHalfPitchAngle / GeneratePitchStep)) * GeneratePitchStep, transform.right) * transform.forward);
            Vector3 pointObjectPosition = PlayerTr.position + transform.forward * DistanceForGenerate;
            Quaternion pointObjectRotation = Quaternion.LookRotation((PlayerTr.position - pointObjectPosition).normalized);

            GameObject currentPointObjectObj = Instantiate(PointObjects[genertablePointObjectsIndex], pointObjectPosition, pointObjectRotation);
            currentPointObjectObj.transform.SetParent(currentTimeKeeperObj.transform);
            PointObject currentPointObject = currentPointObjectObj.GetComponent<PointObject>();
            currentPointObject.PointObjectPos = new Vector2(pointObjectPos.x, pointObjectPos.y);
            currentPointObject.TargetIndicator2 = StageUI_manager.Current.GenerateIndicatorToTarget(currentPointObjectObj.transform);
            currentPointObject.TargetTimeKeeper = currentTimeKeeper;
            currentTimeKeeper.TargetPointObjectList.Add(currentPointObject);
            
            currentTimeKeeper.BaseActivationDelay = _currentBaseActivationDelay;

            generatedCount++;
        }
        if (generatedCount > 0)
        {
            _isGenerationComplete = true;
        }
    }

    // Update is called once per frame
    int GetGeneratablePointObjectIndex()
    {
        float pointObjectBudget = MaxPointObjectCost - _sumPointObjectCost;
        int basisPointObjectArrayIndex = Random.Range(0, PointObjects.Length);
        int generatablePointObjectArrayIndex;
        for (int offsetPointObjectsIndex = 0; offsetPointObjectsIndex < PointObjects.Length; offsetPointObjectsIndex++)
        {
            generatablePointObjectArrayIndex = (int)Mathf.Repeat(offsetPointObjectsIndex + basisPointObjectArrayIndex, PointObjects.Length);
            if (PointObjects[generatablePointObjectArrayIndex].GetComponent<PointObject>().PointObjectCost <= pointObjectBudget)
            {
                return generatablePointObjectArrayIndex;
            }

        }
        return -1;
    }
    
    Vector2Int SearchPointObjectPos()
    {
        _perlinNoiseSeed += Random.Range(0f,1f) * PerlinNoiseScale * PerlinNoiseMagni;

        float perlinNoiseInputX = _perlinNoiseSeed  + PointObjectMapLength.x ;
        float perlinNoiseInputY = _perlinNoiseSeed  + PointObjectMapLength.y ;

        Vector2Int startSearchPoint = new Vector2Int((int)(Mathf.PerlinNoise1D(perlinNoiseInputX) * PointObjectMapLength.x), (int)(Mathf.PerlinNoise1D(perlinNoiseInputY) * PointObjectMapLength.y));
        
        // スタート地点が範囲外になる可能性を考慮
        startSearchPoint.x = Mathf.Clamp(startSearchPoint.x, 0, PointObjectMapLength.x - 1);
        startSearchPoint.y = Mathf.Clamp(startSearchPoint.y, 0, PointObjectMapLength.y - 1);

        bool[,] visited = new bool[PointObjectMapLength.x,PointObjectMapLength.y];
        //探索候補を入れるキュー
        Queue<Vector2Int> searchCandidates = new Queue<Vector2Int>();
        visited[startSearchPoint.x,startSearchPoint.y] = true;
        searchCandidates.Enqueue(startSearchPoint);

        Vector2Int[] offsets = {Vector2Int.right,Vector2Int.down,Vector2Int.left,Vector2Int.up};
        while(searchCandidates.Count > 0)
        {
            Vector2Int searchPoint = searchCandidates.Dequeue();
            //探索の基点が空いているかの確認
            if(_pointObjectMap[searchPoint.x,searchPoint.y] == false)
            {
                _pointObjectMap[searchPoint.x,searchPoint.y] = true;
                return searchPoint;
            }
            
            //基点が埋まっている場合その四方の「未訪問の」マスを探索候補に追加
            int startOffsetIndex = Random.Range(0,4);
            for(int i = 0;i < offsets.Length; i++)
            {
                int offsetIndex = (int)Mathf.Repeat(startOffsetIndex + i,offsets.Length);
                Vector2Int neighborPoint = searchPoint + offsets[offsetIndex];
                //境界外チェック
                if(neighborPoint.x >= PointObjectMapLength.x || neighborPoint.y < 0 || neighborPoint.x < 0 || neighborPoint.y >= PointObjectMapLength.y)
                {
                    //境界外なら次の方向へ
                    continue;
                }
                //訪問済みチェック
                if (visited[neighborPoint.x, neighborPoint.y])
                {
                    //訪問済みなら、次の方向へ
                    continue;
                }
                
                //訪問済みとしてマークし、キューに追加
                visited[neighborPoint.x, neighborPoint.y] = true;
                searchCandidates.Enqueue(neighborPoint);
            }
        }
        return -Vector2Int.one;
    }

    public void SetNotes()
    {
        _fourthNote = 60 / Bpm;
        _eighthNote = _fourthNote / 2;
        _sixteenthNote = _eighthNote / 2;
        PointObject.FourthNote = _fourthNote;
        PointObject.EighthNote = _eighthNote;
        PointObject.SixteenthNote = _sixteenthNote;        
    }

    public void AddSumPointObjectCost(float pointObjectCost){
        _sumPointObjectCost += pointObjectCost;
    }
    public void SubtractSumPointObjectCost(float pointObjectCost){
        _sumPointObjectCost -= pointObjectCost;
    }
    public void RemovePointObjectPos(Vector2 pointObjectPos, float delay)
    {
        StartCoroutine(OneShot());
        IEnumerator OneShot()
        {
            yield return new WaitForSeconds(delay);
            _pointObjectMap[(int)pointObjectPos.x, (int)pointObjectPos.y] = false;
        }
    }

    public Vector2Int WorldPosToPointObjectPos(Vector3 worldPos)
    {
        (float yaw,float pitch) angles = GetYawPitch(worldPos);
        float yawWithOffset = angles.yaw + (GenerateHalfYawAngle / GenerateYawStep * GenerateYawStep);
        float pitchWithOffset = angles.pitch + (GenerateHalfPitchAngle / GeneratePitchStep * GeneratePitchStep);
        int pointObjectMapX = Mathf.RoundToInt(yawWithOffset / GenerateYawStep);
        int pointObjectMapY =  Mathf.RoundToInt(pitchWithOffset / GeneratePitchStep);
        pointObjectMapX = Mathf.Clamp(pointObjectMapX,0,PointObjectMapLength.x - 1);
        pointObjectMapY = Mathf.Clamp(pointObjectMapY,0,PointObjectMapLength.y - 1);
        return new Vector2Int(pointObjectMapX, pointObjectMapY);
    }
    
    public void ChangePointObjectMap(Vector3 worldPos,bool change)
    {
        (int x, int y) targetPointObjectPos;
        targetPointObjectPos.x = WorldPosToPointObjectPos(worldPos).x;
        targetPointObjectPos.y = WorldPosToPointObjectPos(worldPos).y;
        _pointObjectMap[targetPointObjectPos.x, targetPointObjectPos.y] = change;
    }
    
    public (float yaw,float pitch) GetYawPitch(Vector3 worldPos)
    {
        float yaw = Vector3.SignedAngle(Vector3.forward, new Vector3(worldPos.x, 0, worldPos.z), Vector3.up);
        float pitch = Vector3.SignedAngle(new Vector3(worldPos.x, 0, worldPos.z), worldPos, Vector3.Cross(Vector3.up,new Vector3(worldPos.x, 0, worldPos.z)));
        return (yaw,pitch);

    }
}
