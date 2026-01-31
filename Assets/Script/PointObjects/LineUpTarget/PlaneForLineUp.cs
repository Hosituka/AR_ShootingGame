using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
//LineUpTargetが持つ複数の的の抽象クラスです。
public abstract class PlaneForLineUp : MonoBehaviour
{
    public Transform LineUpTargetTr;
    public List<TMPandMeshRenderer> fadeTargetList;
    public Collider[] ColliderArray;
    public LineUpTarget Line_UpTarget;
    public TextMeshPro NeedShotCountTMPro;
    [SerializeField]protected BreakAnimator _targetBreakAnimator;
    [SerializeField]protected Transform _effectPivotTr;
    protected bool _isShow = true;
    protected float _dot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    abstract protected IEnumerator BreakCoroutine();

}
