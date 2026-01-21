using UnityEngine;
using System.Collections;
using Audio;
using System;

public class Gun : MonoBehaviour
{
    [SerializeField]Transform _muzzleTr;
    [SerializeField]GameObject _bullet;
    [SerializeField]Animator _animator;
    [SerializeField]ParticleSystem _coreFlamePs;
    [SerializeField]ParticleSystem _burstFlamePs;
    [SerializeField]Light _muzzleFlashLight;
    RaycastHit _raycastHit;
    bool _isShot;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    /// <summary>
    //銃口の先に複合コライダーを持つゲームオブジェクトがある時、それを返す関数
    /// </summary>
    /// <returns>銃口の先にゲームオブジェクトが存在しない、or複合コライダーが無い時nullを返します。</returns>
    public GameObject GetTarget()
    {
        GameObject targetObj;
        Physics.Raycast(_muzzleTr.position,_muzzleTr.forward,out _raycastHit, 1000,LayerMask.NameToLayer("Player"));
        targetObj = _raycastHit.collider?.attachedRigidbody?.gameObject;
        return targetObj;
    }
    public void Fire()
    {
        if(Time.timeScale == 0) return;
        if(_isShot == true)return;
        _isShot = true;
        SoundManager.Current.PlayOneShot2D_SE(OneShot.shot,0.133f);
        _animator.SetTrigger("Fire");
        Instantiate(_bullet, _muzzleTr.position, _muzzleTr.rotation);
        StartCoroutine(CoolDownShot(0));

        IEnumerator CoolDownShot(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isShot = false;
        }

    }
    //これはFireと言う名のをAnimationClipにより呼ばれる関数です。
    int _baseRotationOfBurstFlame;
    public void OnStartShot()
    {
        _coreFlamePs.Play();
        _baseRotationOfBurstFlame = (int)Mathf.Repeat(_baseRotationOfBurstFlame + 1,2);
        _burstFlamePs.transform.localRotation = Quaternion.Euler(0,0,_baseRotationOfBurstFlame * 36);
        _burstFlamePs.Play();
        _muzzleFlashLight.enabled = true;
    }
    //これはFireと言う名のAnimationClipにより呼ばれる関数です。
    public void OnEndShot()
    {
        _muzzleFlashLight.enabled = false;
    }
}
