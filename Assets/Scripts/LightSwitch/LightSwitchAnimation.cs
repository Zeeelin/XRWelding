using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LightSwitchAnimation : MonoBehaviour
{
    public GameObject lightObject;

    public Renderer ringRenderer;
    public Material ringOnMaterial;
    public Material ringOffMaterial;

    public Animator buttonAnimator;
    public AnimationClip buttonPressedClip;

    private bool _lightIsOn;
    private bool isAnimationPlaying;
    private float animationLength;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ringRenderer.material = ringOffMaterial;
        lightObject.SetActive(false);
        _lightIsOn = false;
        isAnimationPlaying = false;
        animationLength = buttonPressedClip.length;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ToggleLight()
    {
        if (lightObject != null && !isAnimationPlaying)
        {
            if (_lightIsOn)
            {
                lightObject.SetActive(false);
                _lightIsOn = false;
                ringRenderer.material = ringOffMaterial;
            }
            else
            {
                lightObject.SetActive(true);
                _lightIsOn = true;
                ringRenderer.material = ringOnMaterial;
            }
            isAnimationPlaying=true;
            buttonAnimator.SetTrigger("IsPressed");
            StartCoroutine(AnimationPlayWait());

        }
    }
    private IEnumerator AnimationPlayWait() {
        yield return new WaitForSeconds(animationLength);
        isAnimationPlaying = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("HandInteraction"))
        {
            ToggleLight();
        }
    }
    
}
