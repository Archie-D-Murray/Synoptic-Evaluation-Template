using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Utilities;

using Random = UnityEngine.Random;

public static class Extensions {
    ///<summary>Gets, or adds if doesn't contain a component</summary>
    ///<typeparam name="T">Component Type</typeparam>
    ///<param name="gameObject">GameObject to get component from</param>
    ///<returns>Component</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
        T component = gameObject.GetComponent<T>();
        if (!component) {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    ///<summary>Returns true if a GameObject has a component of type T</summary>
    ///<typeparam name="T">Component Type</typeparam>
    ///<param name="gameObject">GameObject to check for component on</param>
    ///<returns>If the component is present</returns>
    public static bool HasComponent<T>(this GameObject gameObject) where T : Component {
        return gameObject.GetComponent<T>() != null;
    }

    ///<summary>Returns null validated for use in null coalescence</summary>
    ///<typeparam name="T">Component Type</typeparam>
    ///<param name="obj">Object reference</param>
    ///<returns>Component if not 'Unity null' or null</returns>
    public static T OrNull<T>(this T obj) where T : UnityEngine.Object {
        return obj ? obj : null;
    }

    ///<summary>Starts a coroutine to set sprite renderer material colour before resetting back to previous</summary>
    ///<param name="spriteRenderer">Sprite Renderer reference</param>
    ///<param name="colour">Colour to set material colour to</param>
    ///<param name="duration">Time before fading back to previous colour</param>
    ///<param name="monoBehaviour">MonoBehaviour to start coroutine on</param>
    public static void FlashColour(this SpriteRenderer spriteRenderer, Color colour, float duration, MonoBehaviour monoBehaviour) {
        monoBehaviour.StartCoroutine(Flash(spriteRenderer, colour, duration));
    }

    private static IEnumerator Flash(SpriteRenderer spriteRenderer, Color colour, float duration) {
        Color original = spriteRenderer.material.color;
        spriteRenderer.material.color = colour;
        yield return Yielders.WaitForSeconds(duration);
        spriteRenderer.material.color = original;
    }

    ///<summary>Starts a coroutine to set sprite renderer material before resetting back to previous creating the classic damage flash</summary>
    ///<param name="spriteRenderer">Sprite Renderer reference</param>
    ///<param name="flashMaterial">Flash material - often a whiteout shader</param>
    ///<param name="originalMaterial">Original material to set back to after flash is done</param>
    ///<param name="duration">Time before fading back to previous colour</param>
    ///<param name="monoBehaviour">MonoBehaviour to start coroutine on</param>
    public static void FlashDamage(this SpriteRenderer spriteRenderer, Material flashMaterial, Material originalMaterial, float duration, MonoBehaviour monoBehaviour) {
        monoBehaviour.StartCoroutine(FlashDamage(spriteRenderer, flashMaterial, originalMaterial, duration));
    }

    private static IEnumerator FlashDamage(SpriteRenderer spriteRenderer, Material flashMaterial, Material originalMaterial, float duration) {
        spriteRenderer.material = flashMaterial;
        yield return Yielders.WaitForSeconds(duration);
        spriteRenderer.material = originalMaterial;
    }

    ///<summary>Starts a coroutine to lerp sprite renderer colour to a target colour</summary>
    ///<param name="spriteRenderer">Sprite Renderer reference</param>
    ///<param name="target">Colour to lerp towards</param>
    ///<param name="original">Original colour to start lerping</param>
    ///<param name="duration">Duration for lerp to happen over</param>
    ///<param name="monoBehaviour">MonoBehaviour to start coroutine on</param>
    public static void LerpColour(this SpriteRenderer spriteRenderer, Color original, Color target, float duration, MonoBehaviour monoBehaviour) {
        monoBehaviour.StartCoroutine(LerpColour(spriteRenderer, original, target, duration));
    }

    private static IEnumerator LerpColour(SpriteRenderer spriteRenderer, Color original, Color target, float duration) {
        float timer = 0.0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(original, target, timer / duration);
            yield return Yielders.WaitForEndOfFrame;
        }
        spriteRenderer.color = target;
    }

    ///<summary>Starts a coroutine to fade sprite renderer colour.alpha to 0 or 1</summary>
    ///<param name="spriteRenderer">Sprite Renderer reference</param>
    ///<param name="speed">Speed to change alpha at</param>
    ///<param name="fadeToTransparent">Move alpha towards 0 or 1 (true or false respectively)</param>
    ///<param name="monoBehaviour">MonoBehaviour to start coroutine on</param>
    public static Coroutine FadeAlpha(this SpriteRenderer spriteRenderer, float speed, bool fadeToTransparent, MonoBehaviour monoBehaviour) {
        return monoBehaviour.StartCoroutine(FadeAlpha(spriteRenderer, speed, fadeToTransparent));
    }

    private static IEnumerator FadeAlpha(SpriteRenderer spriteRenderer, float speed, bool fadeToTransparent) {
        Color rgb = spriteRenderer.color;
        float target = fadeToTransparent ? 0.0f : 1.0f;
        while (spriteRenderer.color.a != target) {
            rgb.a = Mathf.MoveTowards(rgb.a, target, speed * Time.deltaTime);
            spriteRenderer.color = rgb;
            yield return Yielders.WaitForEndOfFrame;
        }
    }

    ///<summary>Starts a coroutine to fade sprite renderer colour.alpha to 0 or 1</summary>
    ///<param name="canvasGroup">CanvasGroup reference</param>
    ///<param name="fadeSpeed">Speed to change alpha at</param>
    ///<param name="fadeToTransparent">Move alpha towards 0 or 1 (true or false respectively)</param>
    ///<param name="monoBehaviour">MonoBehaviour to start coroutine on</param>
    ///<param name="debug">Print debug info</param>
    public static void FadeCanvas(this CanvasGroup canvasGroup, float fadeSpeed, bool fadeToTransparent, MonoBehaviour monoBehaviour, bool debug = false) {
        monoBehaviour.StartCoroutine(CanvasFade(canvasGroup, fadeSpeed, fadeToTransparent, debug));
    }

    ///<summary>Starts a coroutine to fade sprite renderer colour.alpha to 0 or 1</summary>
    ///<param name="canvasGroup">CanvasGroup reference</param>
    ///<param name="fadeSpeed">Speed to change alpha at</param>
    ///<param name="fadeToTransparent">Move alpha towards 0 or 1 (true or false respectively)</param>
    ///<param name="monoBehaviour">MonoBehaviour to start coroutine on</param>
    ///<param name="debug">Print debug info</param>
    ///<returns>Reference to started coroutine</returns>
    public static Coroutine FadeCanvasC(this CanvasGroup canvasGroup, float fadeSpeed, bool fadeToTransparent, MonoBehaviour monoBehaviour, bool debug = false) {
        return monoBehaviour.StartCoroutine(CanvasFade(canvasGroup, fadeSpeed, fadeToTransparent, debug));
    }

    private static IEnumerator CanvasFade(CanvasGroup canvasGroup, float fadeSpeed, bool fadeToTransparent, bool debug) {
        float target = fadeToTransparent ? 0.0f : 1.0f;
        float deltaTime = 0;
        float lastTime = Time.time;
        while (canvasGroup.alpha != target) {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.fixedDeltaTime);
            if (debug) {
                deltaTime = Time.time - lastTime;
                lastTime = Time.time;
                Debug.Log($"Faded to {canvasGroup.alpha} at step: {fadeSpeed * Time.fixedDeltaTime} and deltaTime {deltaTime}");
            }
            yield return Yielders.WaitForFixedUpdate;
        }
        if (fadeToTransparent) {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        } else {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    ///<summary>Clamps vector between min and max</summary>
    ///<param name="vector">Reference vector</param>
    ///<param name="min">Min value</param>
    ///<param name="max">Max value</param>
    ///<returns>Clamped vector</returns>
    public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max) {
        return new Vector2(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y)
        );
    }

    ///<summary>Checks for infinite values in vector</summary>
    ///<param name="vector">Reference vector</param>
    ///<returns>True if no infinite components to vector</returns>
    public static bool IsFinite(this Vector3 vector) {
        return float.IsFinite(vector.x) && float.IsFinite(vector.y) && float.IsFinite(vector.z);
    }

    ///<summary>Set position and rotation of transform to same as other</summary>
    ///<param name="transform">Transform to modify</param>
    ///<param name="other">Transform reference</param>
    public static void SetPositionAndRotation(this Transform transform, Transform other) {
        if (other) {
            transform.SetPositionAndRotation(other.position, other.rotation);
        }
    }

    ///<summary>Checks if vector magnitude is less than range</summary>
    ///<param name="delta">Vector to use as magnitude</param>
    ///<param name="range">Max range (inclusive)</param>
    ///<returns>True if magnitude of delta is less than or equal to range</returns>
    public static bool InRange(this Vector3 delta, float range) {
        return delta.sqrMagnitude <= (range * range);
    }

    ///<summary>Checks if difference between two vectors is less than range</summary>
    ///<param name="vector">Reference point</param>
    ///<param name="other">Distance to check to</param>
    ///<param name="range">Max range (inclusive)</param>
    ///<returns>True if magnitude of delta is less than or equal to range</returns>
    public static bool InRange(this Vector3 vector, Vector3 other, float range) {
        return (vector - other).sqrMagnitude <= (range * range);
    }


    ///<summary>Converts a Vector2 to a 3D XZ plane value</summary>
    ///<param name="vector">Reference vector</param>
    ///<returns>Vector3 with y = 0</returns>
    public static Vector3 ToXZ(this Vector2 vector) {
        return new Vector3(vector.x, 0, vector.y);
    }

    ///<summary>Sets y only</summary>
    ///<param name="vector">Reference vector</param>
    ///<param name="y">New y value</param>
    ///<returns>Vector3 with y = y</returns>
    public static Vector3 WithY(this Vector3 vector, float y) {
        return new Vector3(vector.x, y, vector.z);
    }

    ///<summary>Sets z only</summary>
    ///<param name="vector">Reference vector</param>
    ///<param name="z">New z value</param>
    ///<returns>Vector3 with z = z</returns>
    public static Vector3 WithZ(this Vector3 vector, float z) {
        return new Vector3(vector.x, vector.y, z);
    }

    ///<summary>Offsets another vector component wise</summary>
    ///<param name="vector">Reference vector</param>
    ///<param name="x">Offset x value</param>
    ///<param name="y">Offset y value</param>
    ///<param name="z">Offset z value</param>
    ///<returns>Vector3 offset by (x, y, z)</returns>
    public static Vector3 Offset(this Vector3 vector, float x = 0.0f, float y = 0.0f, float z = 0.0f) {
        return vector + new Vector3(x, y, z);
    }

    ///<summary>Returns random value in array</summary>
    ///<remarks>Returns default(T) if array length below 1 or null</remarks>
    ///<param name="array">Reference array</param>
    ///<returns>Random element value</returns>
    public static T GetRandomValue<T>(this T[] array) {
        if (!Populated(array)) { return default(T); }
        return array[Random.Range(0, array.Length)];
    }

    ///<summary>Returns if index is valid in list</summary>
    ///<param name="collection">Reference list</param>
    ///<param name="index">Index to check bounds on</param>
    ///<returns>If index is safe to index collection with</returns>
    public static bool InBounds<T>(this List<T> collection, int index) {
        return collection != null && index >= 0 && index < collection.Count;
    }

    ///<summary>Get the clockwise angle from origin to point</summary>
    ///<param name="origin">Reference position</param>
    ///<param name="point">Target angle to</param>
    ///<returns>Clockwise angle</returns>
    public static float AngleTo(this Vector2 origin, Vector2 point) {
        return Vector2.SignedAngle(Vector2.up, (point - origin).normalized);
    }

    ///<summary>Is value between min and max inclusive</summary>
    ///<param name="value">Reference value</param>
    ///<param name="min">Min value</param>
    ///<param name="max">Max value</param>
    ///<returns>Value within range</returns>
    public static bool InRange(this float value, float min, float max) {
        return value <= max && value >= min;
    }

    ///<summary>Get the absolute rotation from origin to point</summary>
    ///<param name="origin">Reference position</param>
    ///<param name="point">Target rotation to</param>
    ///<returns>Quaternion representing absolute rotation</returns>
    public static Quaternion RotationTo(this Vector2 origin, Vector2 point) {
        return Quaternion.AngleAxis(origin.AngleTo(point), Vector3.forward);
    }

    ///<summary>Get the absolute rotation aligned to cardinal directions from origin to point</summary>
    ///<param name="origin">Reference position</param>
    ///<param name="point">Target rotation to</param>
    ///<returns>Quaternion representing absolute rotation</returns>
    public static Quaternion RotationCardinalTo(this Vector2 origin, Vector2 point) {
        float angle = origin.AngleTo(point);
        angle = (int) (Mathf.RoundToInt(angle < 0 ? 360 + angle : angle + 45) / 90) * 90;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }

    ///<summary>Checks for presence of parent int transform parent hierarchy</summary>
    ///<param name="transform">Reference transform to traverse parents</param>
    ///<param name="parent">Parent to look for</param>
    ///<returns>If parent is present in path to root</returns>
    public static bool ContainsParentInHierarchy(this Transform transform, Transform parent) {
        if (!transform) { return false; }
        for (Transform p = transform.parent; p; p = p.parent) {
            if (parent == p) {
                return true;
            }
        }
        return false;
    }

    ///<summary>Finds closest collider to position</summary>
    ///<param name="colliders">Colliders to check through</param>
    ///<param name="position">Position to use as reference</param>
    ///<returns>Closest collider or null if colliders is empty</returns>
    public static Collider2D Closest(this Collider2D[] colliders, Vector2 position) {
        Collider2D returnValue = null;
        float closest = float.MaxValue;
        foreach (Collider2D collider in colliders) {
            float distance = Vector2.Distance(position, collider.ClosestPoint(position));
            if (distance < closest) {
                closest = distance;
                returnValue = collider;
            }
        }

        return returnValue;
    }

    ///<summary>Checks if array has been populated</summary>
    ///<param name="array">Array to check</param>
    ///<returns>True if array is not null and length is greater than 0</returns>
    public static bool Populated<T>(this T[] array) {
        return array != null && array.Length > 0;
    }

    ///<summary>Preserves rgb components and returns with new alpha</summary>
    ///<param name="colour">Colour to inherit rgb from</param>
    ///<param name="alpha">New alpha value</param>
    ///<returns>New colour with rgb from colour and alpha provided</returns>
    public static Color WithAlpha(this Color colour, float alpha = 0.0f) {
        return new Color(colour.r, colour.g, colour.b, alpha);
    }

    ///<summary>Gets animation clip from hash</summary>
    ///<param name="animator">Reference animator</param>
    ///<param name="hash">Hash of clip (could be obtains from Animator.StringToHash("value"))</param>
    ///<returns>Animation clip if found, null if not</returns>
    public static AnimationClip GetRuntimeClip(this Animator animator, int hash) {
        return animator.runtimeAnimatorController.OrNull()?.animationClips.FirstOrDefault(clip => Animator.StringToHash(clip.name) == hash) ?? null;
    }
}