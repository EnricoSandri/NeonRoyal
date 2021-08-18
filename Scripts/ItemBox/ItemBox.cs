using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public enum ItemType
    {
        Pistol,
        AssultRifle,
        Shotgun,
        Sniper,
        RocketLauncher
    }

    [Header("Values")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private int itemAmount;

    [Header("Visuals")]
    [SerializeField] private float rotatioAngle;
    [SerializeField] private float VerticalRange;
    [SerializeField] private float VerticalSpeed;

    //Getters
    public ItemType Type { get { return itemType; } }
    public int Amount { get { return itemAmount; } }
    
    // Variables
    private GameObject floatingObj;
    private float VerticalAngle;
   
    // Start is called before the first frame update
    void Start()
    {
        floatingObj = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        MoveBox();
    }

    private void MoveBox()
    {
        VerticalAngle += VerticalSpeed * Time.deltaTime;

        floatingObj.transform.Rotate(0, rotatioAngle * Time.deltaTime, 0);
        floatingObj.transform.localPosition = new Vector3(0, Mathf.Cos(VerticalAngle)* VerticalRange, 0);
    }
}
