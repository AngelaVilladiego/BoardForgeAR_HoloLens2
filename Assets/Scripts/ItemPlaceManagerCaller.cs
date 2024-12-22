using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlaceManagerCaller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NotifyManipulationEnded()
    {
        ItemPlaceManager.Instance.OnActiveObjectManipulationEnded();
        //MenuManager.Instance.ShouldUpdatePosition = false;
    }

    public void NotifyManipulationStarted()
    {
        ItemPlaceManager.Instance.OnActiveObjectManipulationStarted();
        //MenuManager.Instance.ShouldUpdatePosition = true;
    }

    public void NotifyClicked()
    {
        ItemPlaceManager.Instance.OnObjectClicked(gameObject);
    }
}
