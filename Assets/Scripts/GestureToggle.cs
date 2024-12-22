using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureToggle : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isVisible = true;
   [SerializeField] private GameObject hideObject;

        // Method to toggle the GameObject visibility
    public void Toggle()
    {
        isVisible = !isVisible;
        hideObject.SetActive(isVisible);
            
    }
        
    

}
