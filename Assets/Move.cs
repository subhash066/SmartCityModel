using UnityEngine;

public class Move : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        public float moveDistance = 5f;
        public Vector3 rotation;
    }

    public Step[] steps;

    public float moveSpeed = 2f;
    public float rotateSpeed = 180f;

    private int currentStep = 0;
    private float moved = 0f;
    private bool rotating = false;
    private Quaternion targetRotation;
    private bool finished = false;

    void Update()
    {
        if (finished || steps.Length == 0) return;

        Step step = steps[currentStep];

        if (!rotating)
        {
            float moveAmount = moveSpeed * Time.deltaTime;

            transform.position += transform.forward * moveAmount;
            moved += moveAmount;

            if (moved >= step.moveDistance)
            {
                moved = 0f;
                rotating = true;

                targetRotation = transform.rotation * Quaternion.Euler(step.rotation);
            }
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                rotating = false;
                currentStep++;

                if (currentStep >= steps.Length)
                {
                    finished = true; // STOP HERE (NO LOOP)
                }
            }
        }
    }
}