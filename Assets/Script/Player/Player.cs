using UnityEngine;
using Audio;
using System.Collections;
public class Player : MonoBehaviour
{
    public static  Player Current;
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
    [Header("#銃に関するメンバ")]
    [SerializeField]Transform _gunsTr;
    [Header("##赤い銃に関するメンバ")]
    [SerializeField]PlayerGun _redGun;
    [SerializeField]Transform _redGunTr;
    [SerializeField]Transform _redGunMuzzleTr;
    [Header("##青い銃に関するメンバ")]
    [SerializeField]PlayerGun _blueGun;
    [SerializeField]Transform _blueGunTr;
    [SerializeField]Transform _blueGunMuzzleTr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    GameObject _currentBlueGunAimObj;
    GameObject _currentRedGunAimObj;
    GameObject _previousBlueGunAimObj;
    GameObject _previousRedGunAimObj;

    IAimEnterHandler _currentRedGunIAimEnterHandler;
    IAimEnterHandler _currentBlueGunIAimEnterHandler;
    IAimExitHandler _previousRedGunIAimExitHandler;
    IAimExitHandler _previousBlueGunIAimExitHandler;
    // Update is called once per frame
    void Update()
    {
        AimCheck();
    }
    void AimCheck()
    {
        //現在の複合コライダーを持つゲームオブジェクトを取得
        _currentBlueGunAimObj = _blueGun.GetTarget();
        _currentRedGunAimObj = _redGun.GetTarget();

        //BlueGunのレイの先のオブジェクトがnullもしくは別のオブジェクトになった時
        if(_currentBlueGunAimObj != _previousBlueGunAimObj)
        {
            //インターフェースの関数メンバOnAimEnterを呼ぶ仕組み
            if(_currentBlueGunAimObj != null)
            {
                _currentBlueGunIAimEnterHandler = _currentBlueGunAimObj.GetComponent<IAimEnterHandler>();
                if(_currentBlueGunIAimEnterHandler != null)
                {_currentBlueGunIAimEnterHandler.OnAimEnter(KindOfGunRay.BlueGun);}
            }
            //インターフェースの関数メンバOnAimExitを呼ぶ仕組み
            if(_previousBlueGunAimObj != null)
            {
                _previousBlueGunIAimExitHandler = _previousBlueGunAimObj.GetComponent<IAimExitHandler>();
                if(_previousBlueGunIAimExitHandler != null)
                {_previousBlueGunIAimExitHandler.OnAimExit(KindOfGunRay.BlueGun);}
            }
        }
        //RedGunのレイの先のオブジェクトがnullもしくは別のオブジェクトになった時
        if(_currentRedGunAimObj != _previousRedGunAimObj)
        {
            //インターフェースの関数メンバOnAimEnterを呼ぶ仕組み
            if(_currentRedGunAimObj != null)
            {
                _currentRedGunIAimEnterHandler = _currentRedGunAimObj?.GetComponent<IAimEnterHandler>();
                if(_currentRedGunIAimEnterHandler != null)
                {_currentRedGunIAimEnterHandler.OnAimEnter(KindOfGunRay.RedGun);}
            }
            //インターフェースの関数メンバOnAimExitを呼ぶ仕組み
            if(_previousRedGunAimObj != null)
            {
                _previousRedGunIAimExitHandler = _previousRedGunAimObj?.GetComponent<IAimExitHandler>();
                if(_previousRedGunIAimExitHandler != null)
                {_previousRedGunIAimExitHandler.OnAimExit(KindOfGunRay.RedGun);}
            }
        }
        
        //一つ前の複合コライダーを持つゲームオブジェクトの更新
        _previousBlueGunAimObj = _currentBlueGunAimObj;
        _previousRedGunAimObj = _currentRedGunAimObj;

    }
    bool _isSwapping;
    public void LeftShot()
    {
        if (!_isSwapping)
        {
            _redGun.Fire();
        }
        else
        {
            _blueGun.Fire();
        }

    }

    public void RightShot()
    {
        if (!_isSwapping)
        {
            _blueGun.Fire();
        }
        else
        {
            _redGun.Fire();
        }
    }

    (Vector3 position, Quaternion rotation) _trForGunSwap;
    (Vector3 position, Quaternion localRotation) _trForGunSwapForMuzzle;
    GameObject[] _gunSwapRaycastHitObjArray = new GameObject[3];
    RaycastHit _gunsRaycast;
    public void ToggleGunSwapping()
    {
        if(Time.timeScale == 0) return;
        SoundManager.Current.PlayOneShot2D_SE(OneShot.gunSwap,0.8f);
        if (_isSwapping)
        {
            _isSwapping = false;
            _trForGunSwap.position = _redGunTr.position;
            _trForGunSwap.rotation = _redGunTr.rotation;
            _redGunTr.position = _blueGunTr.position;
            _redGunTr.rotation = _blueGunTr.rotation;
            _blueGunTr.position = _trForGunSwap.position;
            _blueGunTr.rotation = _trForGunSwap.rotation;

            _trForGunSwapForMuzzle.localRotation = _redGunMuzzleTr.localRotation;
            _redGunMuzzleTr.localRotation = _blueGunMuzzleTr.localRotation;
            _blueGunMuzzleTr.localRotation = _trForGunSwapForMuzzle.localRotation;
        }
        else
        {
            _isSwapping = true;
            _trForGunSwap.position = _blueGunTr.position;
            _trForGunSwap.rotation = _blueGunTr.rotation;
            _blueGunTr.position = _redGunTr.position;
            _blueGunTr.rotation = _redGunTr.rotation;
            _redGunTr.position = _trForGunSwap.position;
            _redGunTr.rotation = _trForGunSwap.rotation;

            _trForGunSwapForMuzzle.localRotation = _blueGunMuzzleTr.localRotation;
            _blueGunMuzzleTr.localRotation = _redGunMuzzleTr.localRotation;
            _redGunMuzzleTr.localRotation = _trForGunSwapForMuzzle.localRotation;

        }
        Physics.Raycast(_gunsTr.position,_gunsTr.forward,out _gunsRaycast,1000);
        _gunSwapRaycastHitObjArray[0] = _blueGun.GetTarget();
        _gunSwapRaycastHitObjArray[1] = _redGun.GetTarget();
        _gunSwapRaycastHitObjArray[2] = _gunsRaycast.collider?.attachedRigidbody?.gameObject;
        foreach(GameObject gunSwapObj in _gunSwapRaycastHitObjArray)
        {
            if(gunSwapObj == null) continue;
            if(gunSwapObj.GetComponent<IHitGunSwapRayHandler>() != null)
            {
                gunSwapObj.GetComponent<IHitGunSwapRayHandler>().OnHitGunSwapRay();
                break;
            }
        }
    }
}
public enum KindOfGunRay
{
    RedGun,
    BlueGun, 
}
public interface IAimEnterHandler
{
    public void OnAimEnter(KindOfGunRay kindOfGunRay);
}
public interface IAimExitHandler
{    
    public void OnAimExit(KindOfGunRay kindOfGunRay);
}
public interface IHitGunSwapRayHandler
{
    public void OnHitGunSwapRay();
}

