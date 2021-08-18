using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Return : MonoBehaviour
{
    public HUDController hud;
    
    public void ReturnToGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        hud.ShowScreen("inGame");
    }
}
