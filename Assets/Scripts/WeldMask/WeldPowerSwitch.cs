using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class WeldPowerSwitch : MonoBehaviour
{
    public Animator buttonAnimator;
    public AnimationClip buttonPressedClip;

    private bool _lightIsOn;
    private bool isAnimationPlaying;
    private float animationLength;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lightIsOn = false;
        isAnimationPlaying = false;
        animationLength = buttonPressedClip.length;
        PlaytestGameManager.instance.isPowerStart = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ToggleLight()
    {
        if ( !isAnimationPlaying)
        {
            if (_lightIsOn)
            {
                _lightIsOn = false;
                buttonAnimator.SetBool("IsOn", false);
                PlaytestGameManager.instance.isPowerStart = false;
            }
            else
            {
                _lightIsOn = true;
                buttonAnimator.SetBool("IsOn", true);
                PlaytestGameManager.instance.isPowerStart = true;
            }
            isAnimationPlaying = true;

            StartCoroutine(AnimationPlayWait());

        }
    }
    private IEnumerator AnimationPlayWait()
    {
        yield return new WaitForSeconds(animationLength);
        isAnimationPlaying = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("HandInteraction"))
        {
            ToggleLight();

        }
    }


}
