using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speech : MonoBehaviour
{
    public GameObject inventoryCanvas; // Assign the Canvas in the Inspector
    public StatefulInteractable statefulInteractable;
    private SpeechInteractor speechInteractor;

    public void AddKeyword()
    {
        speechInteractor.RegisterInteractable(statefulInteractable, statefulInteractable.SpeechRecognitionKeyword);
    }
    public void ToggleVisibility()
    {
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(!inventoryCanvas.activeSelf);
        }
    }
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
