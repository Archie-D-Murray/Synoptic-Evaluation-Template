using UnityEngine;

namespace Utilities {

    ///<summary>Some simple maths utilities for easing values</summary>
    public static class Maths {

        ///<summary>Uses sine wave to return normalized value from normalized input</summary>
        ///<param name="value">Normalized value from [0,1] - will clamp to 0,1</param>
        ///<returns>Normalized value in range [0,1]</returns>
        public static float SineSmooth(float value) {
            value = Mathf.Clamp01(value);
            return Mathf.Lerp(0.0f, 1.0f, 0.5f + 0.5f * (Mathf.Sin(Mathf.PI * value - 0.5f * Mathf.PI)));
        }

        ///<summary>Standard ease out back easing function</summary>
        ///<param name="value">Normalized value from [0,1]</param>
        ///<returns>Non normalized value - will move from 0 -> ~1.1 -> 1 as value moves from 0 -> ~0.6 -> 1</returns>
        public static float EaseOutBack(float value) {
            return 1 + (1.70158f + 1) * Mathf.Pow(value - 1, 3) + 1.70158f * Mathf.Pow(value - 1, 2);
        }
    }
}