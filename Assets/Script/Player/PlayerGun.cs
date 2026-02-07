using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Audio;
using System.Diagnostics;

public class PlayerGun : MonoBehaviour
{
    public Light SingleMuzzleFlashLight;
    [SerializeField]Light _frontLight;
    [SerializeField]Material _shellMaterialForAddIntensity;
    [SerializeField]Material[] _gunMaterialsForAddIntensity;
    [SerializeField]Transform _muzzleTr;
    [SerializeField]GameObject _bullet;
    [SerializeField]Animator _animator;
    [SerializeField]ParticleSystem _coreFlamePs;
    [SerializeField]ParticleSystem _burstFlamePs;
    [SerializeField]ParticleSystem _muzzleSmokePs;
    [SerializeField]ParticleSystem _shellEjectPs;
    [SerializeField]Light _dualMuzzleFlashLight;
    [SerializeField]PlayerGun _partnerGun;
    RaycastHit _raycastHit;
    bool _isShot;
    void Start()
    {
        switch(_playingShotAnim)
        {
        　　//前のシーンで銃の反動モーションが終わっているor再生されてすらないとき
            case 0:
            break;
            case 1:
        　　//前のシーンで片方の銃の反動モーションが終わっていない時
            SubtractIntensityOfMaterial();
            _playingShotAnim = 0;
            break;
            case 2://前のシーンで二つの銃の反動モーションが終わっていない時
            SubtractIntensityOfMaterial();
            _playingShotAnim = 0;
            break;
        }
    }
    /// <summary>
    //銃口の先に複合コライダーを持つゲームオブジェクトがある時、それを返す関数
    /// </summary>
    /// <returns>銃口の先にゲームオブジェクトが存在しない、or複合コライダーが無い時nullを返します。</returns>
    public GameObject GetTarget()
    {
        Physics.Raycast(_muzzleTr.position,_muzzleTr.forward,out _raycastHit, 1000,LayerMask.NameToLayer("Player"));
        GameObject targetObj = _raycastHit.collider?.attachedRigidbody?.gameObject;
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
    static int _playingShotAnim;
    public void OnStartShot()
    {
        _coreFlamePs.Play();
        _baseRotationOfBurstFlame = (int)Mathf.Repeat(_baseRotationOfBurstFlame + 1,2);
        _burstFlamePs.transform.localRotation = Quaternion.Euler(0,0,_baseRotationOfBurstFlame * 36);
        _burstFlamePs.Play();
        _muzzleSmokePs.Play();
        //ライトの数を違和感なく減らすための処理　これによって軽量化を図る。
        _playingShotAnim ++;
        switch (_playingShotAnim)
        {
            case 1:
                AddIntensityOfMaterial();
                _frontLight.enabled = false;
                SingleMuzzleFlashLight.enabled = true;
            break;
            case 2:
                _partnerGun.SingleMuzzleFlashLight.enabled = false;
                _dualMuzzleFlashLight.enabled = true;
            break;
        }
        void AddIntensityOfMaterial()
        {
            Color materialEmissionColor = _shellMaterialForAddIntensity.GetColor("_EmissionColor");
            materialEmissionColor *= 1.2f;
            _shellMaterialForAddIntensity.SetColor("_EmissionColor",materialEmissionColor);

            foreach(Material material in _gunMaterialsForAddIntensity)
            {
                materialEmissionColor = material.GetColor("_EmissionColor");
                materialEmissionColor *= 3f;
                material.SetColor("_EmissionColor",materialEmissionColor);
            }
        }    
    }
    //これはFireと言う名のAnimationClipにより呼ばれる関数です。
    public void OnOpenChamber()
    {
        _shellEjectPs.Play();
    }
    //これはFireと言う名のAnimationClipにより呼ばれる関数です。
    public void OnEndShot()
    {
        //ライトの数を違和感なく減らすための処理　これによって軽量化を図る。
        _playingShotAnim --;
        switch (_playingShotAnim)
        {
            case 1:
                _dualMuzzleFlashLight.enabled = false;
                _partnerGun.SingleMuzzleFlashLight.enabled = true;
            break;
            case 0:
                SubtractIntensityOfMaterial();
                SingleMuzzleFlashLight.enabled = false;
                _frontLight.enabled = true;
            break;
        }        

    }
    void SubtractIntensityOfMaterial()
    {
        Color materialEmissionColor = _shellMaterialForAddIntensity.GetColor("_EmissionColor");
        materialEmissionColor /= 1.2f;
        _shellMaterialForAddIntensity.SetColor("_EmissionColor",materialEmissionColor);

        foreach(Material material in _gunMaterialsForAddIntensity)
        {
            materialEmissionColor = material.GetColor("_EmissionColor");
            materialEmissionColor /= 3f;
            material.SetColor("_EmissionColor",materialEmissionColor);
        }

    }
}
