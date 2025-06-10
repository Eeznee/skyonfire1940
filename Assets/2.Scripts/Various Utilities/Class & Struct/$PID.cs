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
    public float Integral => integral;
    public float Update(float error, float timeFrame)
    {
        return Mathf.Clamp(UpdateUnclamped(error, timeFrame), -1f, 1f);
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
            float limit = 1f / pidValues.y * 1f;
            integral = Mathf.Clamp(integral, -limit, limit);
        }
        //D
        if(pidValues.z > 0f)
        {
            float deriv = (error - lastError) / timeFrame;
            lastError = error;
            sum += deriv * pidValues.z;
        }

        return sum;
    }
}
