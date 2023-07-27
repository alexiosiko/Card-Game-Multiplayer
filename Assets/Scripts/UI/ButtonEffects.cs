using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
	[SerializeField] AudioClip clickSound; // Sound to play when the button is clicked
    [SerializeField] AudioClip hoverSound; // Sound to play when the pointer hovers over the button
    [SerializeField] AudioSource audioSource; // Reference to the AudioSource component
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameObject.activeSelf && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.activeSelf && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}