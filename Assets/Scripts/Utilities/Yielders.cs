using System.Collections.Generic;

using UnityEngine;

namespace Utilities {

    ///<summary>Static collection of yield objects to reduce unecessary allocations</summary>
    public static class Yielders {

        ///<summary>Internal WaitForEndOfFrame</summary>
        private static WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        ///<summary>Internal WaitForFixedUpdate</summary>
        private static WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

        ///<summary>Cache of all WaitForSeconds as they can be resused</summary>
        private static Dictionary<float, WaitForSeconds> _waitTimes = new();

        ///<summary>Waits for end of frame (will be called after yield null)</summary>
        public static WaitForEndOfFrame WaitForEndOfFrame { get { return _waitForEndOfFrame; } }

        ///<summary>Waits for end of FixedUpdate</summary>
        public static WaitForFixedUpdate WaitForFixedUpdate { get { return _waitForFixedUpdate; } }

        ///<summary>Uses internal cache to provide wait for seconds object - creates if not present</summary>
        ///<remarks>WaitForSeconds objects can be reused so we can remove any unecessary allocations after first time</remarks>
        ///<param name="seconds">Duration required for waiting</param>
        ///<returns>WaitForSeconds object with configured time<returns>
        public static WaitForSeconds WaitForSeconds(float seconds) {
            if (!_waitTimes.ContainsKey(seconds)) {
                _waitTimes.Add(seconds, new WaitForSeconds(seconds));
            }
            return _waitTimes[seconds];
        }
    }
}