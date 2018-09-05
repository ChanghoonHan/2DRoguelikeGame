using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroFootUnderJumpTrigger : MonoBehaviour {

    public Collider2D footCollider;

    public bool CollWithGround = false;
    public bool CollWihtJumpGround = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            CollWithGround = true;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("JumpGround"))
        {
            CollWihtJumpGround = true;
        }

        if (!CollWithGround && CollWihtJumpGround)
        {
            HeroController.S.SetCanUnderJump(true);
        }
        else
        {
            HeroController.S.SetCanUnderJump(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            CollWithGround = false;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("JumpGround"))
        {
            CollWihtJumpGround = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HeroController.S.GetCurState() == HeroController.HeroState.underJump)
        {
            HeroController.S.SetJumpEnd();
        }
    }
}
