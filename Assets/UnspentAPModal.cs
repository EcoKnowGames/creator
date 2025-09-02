using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnspentAPModal : MonoBehaviour
{
    private void Start()
    {
        HideModal();
    }

    public void ShowModal()
    {
        this.gameObject.SetActive(true);
    
    }

    public void HideModal()
    {
        this.gameObject.SetActive(false);
    }

}
