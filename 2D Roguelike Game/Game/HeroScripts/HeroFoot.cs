using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroFoot : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HeroController.S.SetJumpEnd();
    }
}
