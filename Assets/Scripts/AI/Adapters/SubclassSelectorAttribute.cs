using System;

using UnityEngine;

[System.Diagnostics.Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class SubclassSelectorAttribute : PropertyAttribute { }