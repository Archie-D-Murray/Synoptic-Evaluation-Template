using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace AI.Adapters {

    public enum AIAnimationType { None, Locomotion, Attack, Ranged, Dead }
    public enum AIAnimationParam { X, Y, Z, Speed, Crouch }

    [Serializable]
    public class Animation {
        public AIAnimationType AnimationType;
        public string StateName;
        public string Layer;
    }

    [Serializable]
    public class AnimationParam {
        public AIAnimationParam ParamType;
        public string Name;
    }

    ///<summary>Provides an abstract interface for animation</summary>
    ///<summary>Currently natively supports playing defined animations and provides access to underlying animator</summary>
    public class AnimationAdapter : MonoBehaviour {
        [SerializeField] protected Animation[] _animations;
        [SerializeField] protected AnimationParam[] _params = new AnimationParam[] { new AnimationParam() { ParamType = AIAnimationParam.Speed, Name = "Speed" } };
        [SerializeField] protected AIAnimationType _animation;
        [SerializeField] protected AIAnimationType _default;
        [SerializeField] protected bool _debugMessages = false;

        protected Dictionary<AIAnimationType, int> _animLookup;
        protected Dictionary<AIAnimationParam, int> _paramLookup;
        protected Animator _animator;
        protected int _currentHash;

        public Animator Animator => _animator;

        protected virtual void Awake() {
            _animator = GetComponentInChildren<Animator>();
            _animLookup = new Dictionary<AIAnimationType, int>(_animations.Length);
            foreach (Animation anim in _animations) {
                if (anim.StateName.Trim() == string.Empty) {
                    _animLookup.Add(anim.AnimationType, Animator.StringToHash(anim.StateName));
                } else {
                    _animLookup.Add(anim.AnimationType, Animator.StringToHash($"{anim.Layer}.{anim.StateName}"));
                }
            }

            _paramLookup = new Dictionary<AIAnimationParam, int>(_params.Length);
            foreach (AnimationParam param in _params) {
                _paramLookup.Add(param.ParamType, Animator.StringToHash(param.Name));
            }

            if (_default != AIAnimationType.None) {
                Play(_default, 0.0f);
            }

            _animator.fireEvents = false;
        }

        ///<summary>Plays animation if defined in animations array</summary>
        ///<param name="animation">Animation with corresponding entry in array</param>
        ///<param name="normalizedTransitionDuration">Normalised transition time forwarded to Animator.CrossFade()</param>
        public virtual void Play(AIAnimationType animation, float normalizedTransitionDuration = 0.1f) {
            if (_animLookup.TryGetValue(animation, out int hash)) {
                if (_currentHash == hash) { return; }
                _animator.CrossFade(hash, normalizedTransitionDuration);
                _currentHash = hash;
                _animation = animation;
            } else if (_debugMessages) {
                Debug.LogWarning($"[Animation Adapter]: {name} Animator did not have entry {animation}...", this);
            }
        }

        ///<summary>Gets current animation length handling multiple blended clips for a given layer</summary>
        ///<param name="layer">Layer to calculate anim length from</param>
        ///<returns>Average of clip length multiplied by clip weight for all blended clips</returns>
        public float GetCurrentAnimationLength(int layer = -1) {
            return _animator.GetCurrentAnimatorClipInfo(layer).Average(clip => clip.clip.length * clip.weight);
        }

        ///<summary>Gets current animation playing from given layer</summary>
        ///<param name="layer">Layer to calculate anim length from</param>
        ///<returns>Current clip or heightest weighted clip if multiple are blended</returns>
        public AnimationClip GetCurrentClip(int layer = 0) {
            AnimationClip clip = null;
            float weight = 0.0f;

            foreach (AnimatorClipInfo info in _animator.GetCurrentAnimatorClipInfo(layer)) {
                if (info.weight > weight) {
                    weight = info.weight;
                    clip = info.clip;
                }
            }

            return clip;
        }

        public int GetInt(AIAnimationParam param) {
            if (!_paramLookup.ContainsKey(param)) { return 0; }
            return _animator.GetInteger(_paramLookup[param]);
        }

        public float GetFloat(AIAnimationParam param) {
            if (!_paramLookup.ContainsKey(param)) { return 0; }
            return _animator.GetFloat(_paramLookup[param]);
        }

        public bool GetBool(AIAnimationParam param) {
            if (!_paramLookup.ContainsKey(param)) { return false; }
            return _animator.GetBool(_paramLookup[param]);
        }

        public void SetInt(AIAnimationParam param, int value) {
            if (!_paramLookup.ContainsKey(param)) { return; }
            _animator.SetInteger(_paramLookup[param], value);
        }

        public void SetFloat(AIAnimationParam param, float value) {
            if (!_paramLookup.ContainsKey(param)) { return; }
            _animator.SetFloat(_paramLookup[param], value);
        }

        public void SetBool(AIAnimationParam param, bool value) {
            if (!_paramLookup.ContainsKey(param)) { return; }
            _animator.SetBool(_paramLookup[param], value);
        }
    }
}