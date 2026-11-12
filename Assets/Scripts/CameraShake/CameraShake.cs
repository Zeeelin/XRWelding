using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public AnimationCurve curve;

    [SerializeField]
    public bool shake = false;
    [SerializeField]
    private float duration = 1f;
    [SerializeField]
    private float magnitude = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(shake)
        {
            shake = false;
            //Debug.Log("Start shake coroutine");
            StartCoroutine(Shaking());
        }
    }

    IEnumerator Shaking()
    {
        //Store start position of cameras
        Vector3 startPosition = transform.position;

        float elapsedTime = 0f;

        //Debug.Log("Coroutine running, position:" + startPosition);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            //how strong is the shake based on the curve
            float strength = curve.Evaluate(elapsedTime / duration);

            transform.position = startPosition + Random.insideUnitSphere * magnitude * strength;
            yield return null;
        }

        transform.position = startPosition;

    }
}
