using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Audio;

public class PushButton : MonoBehaviour,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler,IPointerUpHandler
{
    
    [SerializeField] Collider[] _colliders;
    [SerializeField] UnityEvent _unityEvent;
    [SerializeField] SpriteRenderer _frontSprite;
    [SerializeField] Transform _buttonTr;
    [SerializeField] Color _downColor;
    [SerializeField] Vector3 _offset = new Vector3(0, -0.35f, 0);
    Color _startColor;
    Vector3 _startPosition;
    static event Action<GameObject> _allCheckColliders;
    static int _downCount;
    //ほかのボタンにより、この自作コンポーネントにアタッチされたゲームオブジェクトが有効化された時、リセットする処理
    void OnEnable()
    {
        _allCheckColliders += CheckColliders;
        _startColor = _frontSprite.color;
        _startPosition = _buttonTr.localPosition;

        SetUpState();
    }
    void OnDisable() {
        _allCheckColliders -= CheckColliders;
    }
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        SetDownState();
        _downCount++;
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if(_downCount > 0)
        {
            SetDownState();
        }
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if(_downCount > 0)
        {
            SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
            SetUpState();
        }
    }
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        _downCount --;
        _allCheckColliders.Invoke(pointerEventData.pointerCurrentRaycast.gameObject);
    }
    private void SetDownState()
    {
        _frontSprite.color = _downColor;
        _buttonTr.localPosition = _startPosition + _offset;
    }

    private void SetUpState()
    {
        _frontSprite.color = _startColor;
        _buttonTr.localPosition = _startPosition;
    }
    void CheckColliders(GameObject _pointerCurrentRaycastObj)
    {
        foreach(Collider collider in _colliders)
        {
            if(collider.gameObject == _pointerCurrentRaycastObj)
            {
                SoundManager.Current.PlayOneShot2D_SE(OneShot.downButton,0.7f);
                _unityEvent.Invoke();
                SetUpState();
                _pointerCurrentRaycastObj = null;
                _downCount = 0;
            }
        }
    }

}
