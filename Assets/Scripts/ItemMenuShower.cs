using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMenuShower : MonoBehaviour
{
    private BoundsControl boundsControl;

    // Start is called before the first frame update
    void Start()
    {
        boundsControl = GetComponent<BoundsControl>();
    }

    public void ShowMenu()
    {
        if (boundsControl != null)
        {
            MenuManager.Instance.ShowMenu(gameObject);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
