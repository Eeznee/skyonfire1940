using UnityEngine;

namespace NWH.Common.Utility
{
    /// <summary>
    ///     Implementation of PID controller based on:
    ///     https://github.com/ms-iot/pid-controller/blob/master/PidController/PidController/PidController.cs
    /// </summary>
    public class PIDController
    {
        public float maxValue;
        public float minValue;

        private float _processVariable;


        public PIDController(float gainProportional, float gainIntegral, float gainDerivative, float outputMin,
            float outputMax)
        {
            GainDerivative = gainDerivative;
            GainIntegral = gainIntegral;
            GainProportional = gainProportional;
            maxValue = outputMax;
            minValue = outputMin;
        }


        /// <summary>
        ///     The derivative term is proportional to the rate of
        ///     change of the error
        /// </summary>
        public float GainDerivative { get; set; }

        /// <summary>
        ///     The integral term is proportional to both the magnitude
        ///     of the error and the duration of the error
        /// </summary>
        public float GainIntegral { get; set; }

        /// <summary>
        ///     The proportional term produces an output value that
        ///     is proportional to the current error value
        /// </summary>
        /// <remarks>
        ///     Tuning theory and industrial practice indicate that the
        ///     proportional term should contribute the bulk of the output change.
        /// </remarks>
        public float GainProportional { get; set; }

        /// <summary>
        ///     Adjustment made by considering the accumulated error over time
        /// </summary>
        /// <remarks>
        ///     An alternative formulation of the integral action, is the
        ///     proportional-summation-difference used in discrete-time systems
        /// </remarks>
        public float IntegralTerm { get; private set; }

        /// <summary>
        ///     The current value
        /// </summary>
        public float ProcessVariable
        {
            get { return _processVariable; }
            set
            {
                ProcessVariableLast = _processVariable;
                _processVariable = value;
            }
        }

        /// <summary>
        ///     The last reported value (used to calculate the rate of change)
        /// </summary>
        public float ProcessVariableLast { get; private set; }

        /// <summary>
        ///     The desired value
        /// </summary>
        public float SetPoint { get; set; } = 0;


        /// <summary>
        ///     The controller output
        /// </summary>
        /// <param name="timeSinceLastUpdate">
        ///     timespan of the elapsed time
        ///     since the previous time that ControlVariable was called
        /// </param>
        /// <returns>Value of the variable that needs to be controlled</returns>
        public float ControlVariable(float timeSinceLastUpdate)
        {
            float error = SetPoint - ProcessVariable;

            // integral term calculation
            IntegralTerm += GainIntegral * error * timeSinceLastUpdate;
            IntegralTerm = Mathf.Clamp(IntegralTerm, minValue, maxValue);

            // derivative term calculation
            float dInput = _processVariable - ProcessVariableLast;
            float derivativeTerm = GainDerivative * (dInput / timeSinceLastUpdate);

            // proportional term calcullation
            float proportionalTerm = GainProportional * error;

            float output = proportionalTerm + IntegralTerm - derivativeTerm;

            output = Mathf.Clamp(output, minValue, maxValue);

            return output;
        }
    }
}