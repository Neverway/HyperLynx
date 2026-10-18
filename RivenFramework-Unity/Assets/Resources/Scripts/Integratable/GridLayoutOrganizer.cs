//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayoutOrganizer : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Min(1)] public int rowCount;
    [Min(1)] public int columnCount;
    public Vector2 offsetPadding = Vector2.one;
    public Vector3 startPosition;
    public bool rowsGrowNegative;
    public enum GridPlane { XY, XZ }
    public GridPlane gridPlane;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<Transform> children = new List<Transform>();
    
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    
    private void OnValidate()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) OrganizeChildren();
        };
        #endif
    }

    private void Start()
    {
        OrganizeChildren();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    [ContextMenu("Organize Children")]
    public void OrganizeChildren()
    {
        children.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        int columns = Mathf.Max(1, columnCount);
        int capacity = Mathf.Max(1, rowCount) * columns;
        float rowDirection = rowsGrowNegative ? -1f : 1f;

        for (int i = 0; i < children.Count && i < capacity; i++)
        {
            int row = i / columns;
            int column = i % columns;
 
            float x = column * offsetPadding.x;
            float rowOffset = row * offsetPadding.y * rowDirection;
 
            Vector3 offset = gridPlane == GridPlane.XY ? new Vector3(x, rowOffset, 0f) : new Vector3(x, 0f, rowOffset);
 
            children[i].localPosition = startPosition + offset;

        }
    }
    #endregion
}
