using UnityEngine;

[System.Serializable]
public class PID
{
    public Vector3 pidValues;
    float integral = 0f;
    float lastError = 0f;
    public PID(Vector3 vals)
    {
        pidValues = vals;
    }
    public float Update(float error, float timeFrame)
    {
        return Mathf.Clamp(UpdateUnclamped(error, timeFrame), -1f, 1f);
    }

    public float UpdateAndDebugUnclamped(float error, float timeFrame)
    {
        //P
        Vector3 sum = Vector3.zero;
        sum.x = error * pidValues.x;
        //I
        if (pidValues.y > 0f)
        {
            integral += error * timeFrame;
            sum.y = integral * pidValues.y;
            float limit = 1f / pidValues.y;
            integral = Mathf.Clamp(integral, -limit, limit);
        }
        //D
        float deriv = (error - lastError) / timeFrame;
        //deriv *= Mathf.Lerp(Mathf.Abs(deriv)/derivPouet, 1f, Mathf.Abs(deriv)/derivPouet);
        lastError = error;
        sum.z = deriv * pidValues.z;

        //Debug.Log(sum);

        return sum.x + sum.y + sum.z;
    }
    public float UpdateUnclamped(float error, float timeFrame)
    {
        //P
        float sum = error * pidValues.x;
        //I
        if (pidValues.y > 0f)
        {
            integral += error * timeFrame;
            sum += integral * pidValues.y;
            float limit = 1f / pidValues.y;
            integral = Mathf.Clamp(integral, -limit, limit);
        }
        //D
        float deriv = (error - lastError) / timeFrame;
        //deriv *= Mathf.Lerp(Mathf.Abs(deriv)/derivPouet, 1f, Mathf.Abs(deriv)/derivPouet);
        lastError = error;
        sum += deriv * pidValues.z;

        return sum;
    }
}
