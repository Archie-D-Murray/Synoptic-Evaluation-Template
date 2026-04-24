#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System;
using System.Linq;

using System.Reflection;
using AI.HSM;
using System.IO;

[InitializeOnLoad]
public class StateEnumCreator {

    private static Type _enumType;

    [SerializeField] private static string _path;

    static StateEnumCreator() {
        _path = Path.Combine(Application.dataPath, "Scripts", "AI", "States.cs");
        _enumType = GetEnumType();
    }

    public static IEnumerable<string> GetStateTypes() {
        return Assembly.GetAssembly(typeof(State)).GetTypes().Where(type => type.IsSubclassOf(typeof(State))).Select(type => type.Name.Replace("State", ""));
    }

    public static Type GetEnumType() {
        return Assembly.GetAssembly(typeof(State)).GetTypes().Where(type => type.IsEnum && type.Name == "AIState").FirstOrDefault();
    }

    [MenuItem("Assets/AI/Force Recreate State Enums", false, 1)]
    public static void HardInitStateEnum() {
        InitStateEnum(true);
    }

    [MenuItem("Assets/AI/Recreate State Enums", false, 1)]
    public static void SoftInitStateEnum() {
        InitStateEnum(false);
    }

    public static void InitStateEnum(bool force) {
        _enumType = GetEnumType();

        Array knownEnumValues = Enum.GetValues(_enumType);
        HashSet<string> found = new HashSet<string>(knownEnumValues.Length);

        foreach (object enumValue in knownEnumValues) {
            found.Add(enumValue.ToString());
        }


        HashSet<string> valid = new HashSet<string>();
        valid.Add("None");
        foreach (string state in GetStateTypes()) {
            valid.Add(state);
        }

        bool hasNewStates = valid.Count > found.Count;

        string separator = ", ";
        Debug.Log($"States:\nValid: {string.Join(separator, valid)}\nFound: {string.Join(separator, found)}");

        hasNewStates = found.RemoveWhere(state => !valid.Contains(state)) > 0;

        foreach (string newState in valid) {
            if (found.Add(newState)) {
                hasNewStates = true;
            }
        }

        Debug.Log($"Validated States:\nValid: {string.Join(separator, valid)}\nFound: {string.Join(separator, found)}");

        if (!hasNewStates && !force) {
            Debug.Log("Did not need to forcibly re-create state enum");
            return;
        }

        File.Delete(_path + ".meta");

        string data = GetEnumString(found);
        Debug.Log($"[StateEnumCreator]: Attempting to write new AIStates enum with values: {data}");

        StreamWriter stream = new StreamWriter(_path, false);
        foreach (char ch in data.ToCharArray()) {
            stream.Write(ch);
        }

        stream.Flush();
        stream.Close();
        WaitForFilesystem(stream);
    }

    async static void WaitForFilesystem(StreamWriter stream) {
        await stream.DisposeAsync();

        AssetDatabase.Refresh();
    }

    public static string GetEnumString(IEnumerable<string> types) {
        return
            "namespace AI {\n" +
            $"    public enum AIState {{ {string.Join(", ", types)} }}\n" +
            "}\n";
    }
}

#endif