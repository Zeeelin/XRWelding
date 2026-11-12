using UnityEngine;
using UnityEngine.Events;

public class LightSwitch : MonoBehaviour
{

    public UnityEvent onPressed, onReleased;
    public GameObject lightObject;

    public Renderer ringRenderer;
    public Material ringOnMaterial;
    public Material ringOffMaterial;

    [SerializeField]
    private float threashold = .1f; //percent of button pressed needed to activate the button
    [SerializeField]
    private float deadZone = .025f; //error tolerance


    private bool _isPressed;
    private bool _lightIsOn;
    private Vector3 _StartPos;
    private ConfigurableJoint _joint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _StartPos = transform.localPosition;
        _joint = GetComponent<ConfigurableJoint>();

        ringRenderer.material = ringOffMaterial;
        lightObject.SetActive(false);
        _lightIsOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isPressed && (GetDisValue() + threashold) >= 1)
            Pressed();
        if (_isPressed && (GetDisValue() - threashold) <= 0)
            Released();

    }

    private float GetDisValue()
    {
        var value = Vector3.Distance(_StartPos, transform.localPosition) / _joint.linearLimit.limit;

        if (Mathf.Abs(value) < deadZone)
            value = 0;

        return Mathf.Clamp(value, -1f, 1f);
    }

    void Pressed()
    {
        if (!_isPressed) {
            _isPressed = true;
            ToggleLight();
        }
        Debug.Log("Pressed");
    }

    void Released()
    {
        _isPressed = false;
        /*
        _isPressed = false;
        onReleased.Invoke();
        Debug.Log("Released");
        */
    }

    public void ToggleLight()
    {
        if (lightObject != null)
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
        }
    }

        /*
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(handTag))
            {
                ToggleLight();
            }
        }
        */
}
