using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// Based on: https://forum.unity.com/threads/fpstotext-free-fps-framerate-calculator-with-options.463667/
namespace NWH.Common.Demo
{
    /// <summary>
    ///     <para>Pushes the Framerate value to a Text component.</para>
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class FpsToText : MonoBehaviour
    {
        public bool groupSampling = true;
        public int sampleSize = 120;
        public int updateTextEvery = 1;
        public bool smoothed = true;
        public bool forceIntResult = true;
        public bool useSystemTick;
        public bool useColors = true;
        public Color good = Color.green;
        public Color okay = Color.yellow;
        public Color bad = Color.red;
        public int okayBelow = 60;
        public int badBelow = 30;

        private Text _targetText;
        private float[] _fpsSamples;
        private int _sampleIndex;
        private int _textUpdateIndex;
        private float _fps;

        private int _sysLastSysTick;
        private int _sysLastFrameRate;
        private int _sysFrameRate;


        protected virtual void Start()
        {
            _targetText = GetComponent<Text>();
            _fpsSamples = new float[sampleSize];
            for (int i = 0; i < _fpsSamples.Length; i++)
            {
                _fpsSamples[i] = 0.001f;
            }

            if (!_targetText)
            {
                enabled = false;
            }
        }


        protected virtual void Update()
        {
            if (groupSampling)
            {
                Group();
            }
            else
            {
                SingleFrame();
            }

            string fps = _fps.ToString(CultureInfo.CurrentCulture);

            _sampleIndex = _sampleIndex < sampleSize - 1 ? _sampleIndex + 1 : 0;
            _textUpdateIndex = _textUpdateIndex > updateTextEvery ? 0 : _textUpdateIndex + 1;
            if (_textUpdateIndex == updateTextEvery)
            {
                _targetText.text = fps;
            }

            if (!useColors)
            {
                return;
            }

            if (_fps < badBelow)
            {
                _targetText.color = bad;
                return;
            }

            _targetText.color = _fps < okayBelow ? okay : good;
        }


        protected virtual void Reset()
        {
            sampleSize = 20;
            updateTextEvery = 1;
            smoothed = true;
            useColors = true;
            good = Color.green;
            okay = Color.yellow;
            bad = Color.red;
            okayBelow = 60;
            badBelow = 30;
            useSystemTick = false;
            forceIntResult = true;
        }


        protected virtual void SingleFrame()
        {
            _fps = useSystemTick
                       ? GetSystemFramerate()
                       : smoothed
                           ? 1 / Time.smoothDeltaTime
                           : 1 / Time.deltaTime;
            if (forceIntResult)
            {
                _fps = (int)_fps;
            }
        }


        protected virtual void Group()
        {
            _fpsSamples[_sampleIndex] = useSystemTick
                                            ? GetSystemFramerate()
                                            : smoothed
                                                ? 1 / Time.smoothDeltaTime
                                                : 1 / Time.deltaTime;

            _fps = 0;
            bool loop = true;
            int i = 0;
            while (loop)
            {
                if (i == sampleSize - 1)
                {
                    loop = false;
                }

                _fps += _fpsSamples[i];
                i++;
            }

            _fps /= _fpsSamples.Length;
            if (forceIntResult)
            {
                _fps = (int)_fps;
            }
        }


        protected virtual int GetSystemFramerate()
        {
            if (Environment.TickCount - _sysLastSysTick >= 1000)
            {
                _sysLastFrameRate = _sysFrameRate;
                _sysFrameRate = 0;
                _sysLastSysTick = Environment.TickCount;
            }

            _sysFrameRate++;
            return _sysLastFrameRate;
        }
    }
}