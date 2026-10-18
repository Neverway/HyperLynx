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

public class ZoneManager : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public List<ZoneEntry> ZoneEntries = new List<ZoneEntry>();
    public GameObject currentCreatedZone;
    public event Action OnZoneChanged;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public ZoneEntry GetZoneEntry(GameObject target)
    {
        foreach (var zoneEntry in ZoneEntries)
        {
            if (zoneEntry.zoneObject == target)
            {
                return zoneEntry;
            }
        }

        return null;
    }

    public void LoadZone(GameObject target)
    {
        if (currentCreatedZone != null)
        {
            Destroy(currentCreatedZone);
        }
        
        foreach (var zoneEntry in ZoneEntries)
        {
            if (zoneEntry.zoneObject == target)
            {
                currentCreatedZone = Instantiate(zoneEntry.zoneObject, null);
            }
        }
        
        OnZoneChanged?.Invoke();
    }


    #endregion
}

[Serializable]
public class ZoneEntry
{
    public GameObject zoneObject;
    public ZoneState zoneState = ZoneState.Locked;
}

public enum ZoneState
{
    Locked,
    Unlocked,
    Completed,
}
