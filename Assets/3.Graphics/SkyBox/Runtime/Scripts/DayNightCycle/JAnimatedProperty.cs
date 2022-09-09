using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    [System.Serializable]
    public class JAnimatedProperty
    {
        [SerializeField]
        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                {
                    name = string.Empty;
                }
                return name;
            }
            set
            {
                name = value;
            }
        }

        [SerializeField]
        private string displayName;
        public string DisplayName
        {
            get
            {
                if (displayName == null)
                {
                    displayName = string.Empty;
                }
                return displayName;
            }
            set
            {
                displayName = value;
            }
        }

        [SerializeField]
        private JCurveOrGradient curveOrGradient;
        public JCurveOrGradient CurveOrGradient
        {
            get
            {
                return curveOrGradient;
            }
            set
            {
                curveOrGradient = value;
            }
        }

        [SerializeField]
        private AnimationCurve curve;
        public AnimationCurve Curve
        {
            get
            {
                if (curve == null)
                {
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 0);
                }
                return curve;
            }
            set
            {
                curve = value;
            }
        }

        [SerializeField]
        private Gradient gradient;
        public Gradient Gradient
        {
            get
            {
                if (gradient == null)
                {
                    gradient = JUtilities.CreateFullWhiteGradient();
                }
                return gradient;
            }
            set
            {
                gradient = value;
            }
        }

        public float EvaluateFloat(float t)
        {
            return Curve.Evaluate(t);
        }

        public Color EvaluateColor(float t)
        {
            return Gradient.Evaluate(t);
        }

        public static JAnimatedProperty Create(string name, string displayName, JCurveOrGradient curveOrGradient)
        {
            JAnimatedProperty props = new JAnimatedProperty();
            props.name = name;
            props.displayName = displayName;
            props.curveOrGradient = curveOrGradient;
            return props;
        }
    }
}
