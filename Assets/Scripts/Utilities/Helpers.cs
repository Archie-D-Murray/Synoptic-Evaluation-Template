using UnityEngine;

namespace Utilities {

    ///<summary>A collection of 3D and 2D helper functions</summary>
    public class Helpers : Singleton<Helpers> {

        ///<summary>Fallback offset for tilemap mouse position</summary>
        public static readonly Vector2 Offset = new Vector2(0.5f, 0.5f);

        protected override void OnAwake() {
            MainCamera = Camera.main;
            PhysicsFPS = Mathf.Round(1.0f / Time.fixedUnscaledDeltaTime);
            Debug.Log($"Target physics frameRate => {PhysicsFPS}");
        }

        ///<summary>Cached main camera - found on Awake()</summary>
        public Camera MainCamera;

        ///<summary>Mouse position cache</summary>
        private Vector2 _mousePosition;

        ///<summary>Gets mouse position on tilemap (assumes square unit tilesize if not specified)</summary>
        ///<remarks>Assumes Camera.main is valid for raycasts - will not work in 3D</remarks>
        ///<param name="tilemap">Tilemap for use as reference</param>
        ///<returns>Tilemap position for use in functions like Tilemap.GetTile()</returns>
        public Vector2 TileMapMousePosition(UnityEngine.Tilemaps.Tilemap tilemap) {
            return Vector2Int.FloorToInt(GetWorldMousePosition() - (Vector2) tilemap.tileAnchor) + (Vector2) tilemap.cellSize * 0.5f;
        }

        ///<summary>Screen space mouse position</summary>
        public Vector2 MousePosition => _mousePosition;

        ///<summary>Constructs a unit vector with clockwise rotation</summary>
        ///<summary>
        ///<example>Ex: radians: 0.0f -> Vector2 { 0, 1 }, 0.5PI -> Vector2 { 1, 0 } </example>
        ///</summary>
        ///<param name="radians">Clockwise rotation in radians</param>
        ///<returns>Unit vector on circle</returns>
        public static Vector2 FromRadians(float radians) {
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }

        ///<summary>Gets angle to mouse using Vector2.up as reference (CW)</summary>
        ///<remarks>Assumes Camera.main is valid for raycasts - will not work in 3D</remarks>
        ///<param name="obj">Reference for mouse pos difference</param>
        ///<returns>Clockwise angle to mouse (0 is up)</returns>
        public float AngleToMouse(Transform obj) {
            return Vector2.SignedAngle(
                Vector2.up,
                DirectionToMouse(obj.position)
            );
        }

        ///<summary>Gets angle to mouse using direction as reference (ACW)</summary>
        ///<remarks>Assumes Camera.main is valid for raycasts - will not work in 3D</remarks>
        ///<param name="obj">Reference for mouse pos difference</param>
        ///<returns>Anti clockwise angle to mouse (0 is up)</returns>
        public float AngleToMouseOpposite(Transform obj) {
            return Vector2.SignedAngle(
                DirectionToMouse(obj.position),
                Vector2.up
            );
        }

        ///<summary>Gets world space mouse position using Camera.main as raycast</summary>
        ///<remarks>Assumes Camera.main is valid for raycasts - will not work in 3D</remarks>
        ///<returns>World space mouse position</returns>
        public Vector2 GetWorldMousePosition() {
            return MainCamera.ScreenToWorldPoint(_mousePosition);
        }

        ///<summary>Creates a vector expressing the difference between world space mouse position and position</summary>
        ///<remarks>Assumes Camera.main is valid for raycasts - will not work in 3D</remarks>
        ///<param name="position">Reference position</param>
        ///<returns>Normalized direction</returns>
        public Vector2 DirectionToMouse(Vector2 position) {
            return (GetWorldMousePosition() - position).normalized;
        }

        private void FixedUpdate() {
#if ENABLE_INPUT_SYSTEM
            _mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            _mousePosition = Input.mousePosition;
#endif
        }

        ///<summary>Creates a rotation expressing the absolute rotation to be oriented at (to) with reference (from)</summary>
        ///<param name="from">Reference position</param>
        ///<param name="to">Target to look at</param>
        ///<returns>Quaternion representing absolute rotation (ex: set transform.rotation to the result of this)</returns>
        public static Quaternion Look2D(Vector2 from, Vector2 to) {
            return Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.up, (to - from).normalized), Vector3.forward);
        }

        ///<summary>Target Physics Frame Rate (default 50 in Unity - calculated on start)</summary>
        public static float PhysicsFPS = 50.0f;

        ///<summary>Expresses Time.fixedDeltaTime as a ratio of target physics frame rate</summary>
        public static float NormalizedFixedDeltaTime => Time.fixedDeltaTime * PhysicsFPS;

        ///<summary>Clamps value between min and max handling wrapping around 360.0f</summary>
        ///<param name="degrees">Angle in degrees</param>
        ///<param name="min">Min angle in degrees</param>
        ///<param name="max">Max angle in degrees</param>
        ///<returns>Clamped angle in degrees</returns>
        public static float ClampAngle(float degrees, float min, float max) {
            while (degrees < -360.0f) { degrees += 360.0f; }
            while (degrees > +360.0f) { degrees -= 360.0f; }
            return Mathf.Clamp(degrees, min, max);
        }

        ///<summary>Logs message with context of MonoBehaviour and object</summary>
        ///<summary>Logs are in form "[[{type name} (Object: {context name})]]: {message}"</summary>
        ///<param name="context">MonoBehaviour to use as context (uses class name and object name)</param>
        ///<param name="message">Message to follow context</param>
        public static void ContextLog(MonoBehaviour context, object message) {
            if (context) {
                Debug.Log($"[{context.GetType().Name} (Object: {context.name})]: {message}");
            } else {
                Debug.Log($"[{context.GetType().Name} (Object: null)]: {message}");
            }
        }

        public static string ClassNameOrNull<T>(T value) where T : class {
            return value != null ? value.GetType().Name : "Null";
        }
    }
}