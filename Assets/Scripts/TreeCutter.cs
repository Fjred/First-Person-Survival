using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TreeCutter : MonoBehaviour
{
    public float distance = 2;

    public TMP_Text woodText;
    public float woodNumber;
    public AudioSource punch;
    public AudioSource treeCutOff;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Hit();
        }
    }

    public void Hit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance))
        {
            if (hit.collider.CompareTag("Tree"))
            {
                punch.Play();

                TreeHealth treeHealth = hit.collider.GetComponent<TreeHealth>();

                if (treeHealth != null)
                {
                    treeHealth.TakeDamage(1);

                    if(treeHealth.cutOff == true)
                    {
                        treeCutOff.Play();
                    }

                    woodNumber += treeHealth.woodCount;

                    woodText.text = $"{woodNumber}";
                }
            }
        }
    }
}
