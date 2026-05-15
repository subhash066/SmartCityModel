using UnityEngine;

public class SimpleTrafficLight : MonoBehaviour
{
    public Light redLight;
    public Light yellowLight;
    public Light greenLight;

    public float greenTime = 10f;
    public float yellowTime = 2f;
    public float redTime = 5f;

    private float timer = 0f;
    private int state = 0;

    void Start()
    {
        SetState(0);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            state = (state + 1) % 4;
            SetState(state);
        }
    }

    void SetState(int s)
    {
        redLight.enabled = s == 0 || s == 3;
        yellowLight.enabled = s == 1 || s == 3;
        greenLight.enabled = s == 2;

        switch (s)
        {
            case 0: timer = redTime; break;
            case 1: timer = yellowTime; break;
            case 2: timer = greenTime; break;
            case 3: timer = yellowTime; break;
        }
    }
}
