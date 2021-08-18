using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class StormManager : NetworkBehaviour
{
    public delegate void ShrinkHandler();
    public event ShrinkHandler OnShrink;

    [SerializeField] private float[] shrinkTimes;
    [SerializeField] private float[] distanceFromCentre;
    [SerializeField] private GameObject[] StormObjects;
    [SerializeField] public AudioSource StormHorn;
    private float timer;
    private int stormIndex = -1;



    private bool shouldShrink;
    public bool ShouldShrink
    {
        set { shouldShrink = value; }
    }


    private void Update()
    {
        if (!isServer) return;
        if (!shouldShrink) return;

        timer += Time.deltaTime;

        for (int i = 0; i <shrinkTimes.Length; i++)
        {
            float currentShrinkTime = shrinkTimes[i];
            if(timer > currentShrinkTime && stormIndex < i)
            {
                //shrink the storm
                stormIndex = i;

                float targetDistance = distanceFromCentre[i];
                foreach (GameObject stormObj in StormObjects)
                {
                    stormObj.GetComponent<StormObject>().MoveToDistance(targetDistance);
                }
               
                //Show Storm Alert
                if (OnShrink != null)
                {
                    OnShrink();
                }
            }
        }

    }
}
