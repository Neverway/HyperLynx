//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using DG.Tweening;
using TMPro;
using UnityEngine;

public class SimpleTextbox : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Vector3 worldOffset = Vector3.zero;
    [Min(0)] public float smoothTime = 0.15f;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Transform targetTransform;
    private Vector3 targetPoint;
    private bool hasTarget;
 
    private Camera cam;
    private Canvas canvas;
    private RectTransform rect;
    private RectTransform parentRect;
    private bool placed;



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_Text title, content;
    public AudioSource audioSource;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void LateUpdate()
    {
        if (hasTarget) UpdatePosition();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void UpdatePosition()
    {
        if (targetTransform) targetPoint = targetTransform.position;
 
        if (!cam) cam = FindObjectOfType<Camera>();
        if (!cam) return;
 
        if (!rect)
        {
            rect = (RectTransform)transform;
            parentRect = rect.parent as RectTransform;
            canvas = GetComponentInParent<Canvas>();
        }
        if (!parentRect || !canvas) return;
 
        Vector3 screenPos = cam.WorldToScreenPoint(targetPoint + worldOffset);
        if (screenPos.z < 0) return;
 
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
 
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out Vector2 localPoint))
            return;

        Vector3 desired = new Vector3(localPoint.x, localPoint.y, rect.localPosition.z);

        rect.DOKill();
        if (!placed || smoothTime <= 0f)
        {
            rect.localPosition = desired;
            placed = true;
        }
        else
        {
            rect.DOLocalMove(desired, smoothTime)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void FollowTarget(Transform _target)
    {
        targetTransform = _target;
        hasTarget = _target;
        if (hasTarget) UpdatePosition();
    }
 
    public void MoveToPoint(Vector3 _point)
    {
        targetTransform = null;
        targetPoint = _point;
        hasTarget = true;
        UpdatePosition();
    }



    #endregion
}
