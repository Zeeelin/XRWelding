using System.Collections;
using UnityEngine;

public class EmissionController : MonoBehaviour
{

    [Header("Glow Setting")]
    [SerializeField]
    private float glowDuration = 2f;
    [SerializeField]
    private float minGlowStrength = 0f;
    [SerializeField]
    private float maxGlowStrength = 5f;

    public Material _mat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TriggerGlow();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TriggerGlow()
    {
        StopAllCoroutines();
        StartCoroutine(GlowFade());
    }

    IEnumerator GlowFade()
    {
        //taking the GlowStrength to max
        _mat.SetFloat("_GlowStrength", maxGlowStrength);
        float timeElapsed = 0f;

        while (timeElapsed < glowDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / glowDuration;
            _mat.SetFloat("_GlorStrength", Mathf.Lerp(maxGlowStrength, 0f, t * t));
            yield return null;
        }

        _mat.SetFloat("_GlowStrength", minGlowStrength);
        // Attempt to destroy the material after glow
        //Destroy(_mat);
    }
}
