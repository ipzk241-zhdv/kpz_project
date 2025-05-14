using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

[ExecuteAlways]
public class RocketLoader : MonoBehaviour
{
    public static RocketLoader Instance { get; private set; }
    private const string RocketTag = "Rocket";
    public string fileName = "Rockets.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (!IsTagDefined(RocketTag))
        {
            Debug.LogWarning($"Tag '{RocketTag}' is not defined in project tags; disabling RocketLoader.");
            enabled = false;
            return;
        }
    }

    private bool IsTagDefined(string tag)
    {
        try
        {
            GameObject.FindWithTag(tag);
            return true;
        }
        catch (UnityException)
        {
            return false;
        }
    }


    [ContextMenu("Save rockets to JSON")]
    public void SaveToJSON()
    {
        var rockets = GameObject.FindGameObjectsWithTag(RocketTag);
        var systemJson = new SystemJSON();

        foreach (var go in rockets)
        {
            var mover = go.GetComponent<OrbitMover>();
            if (mover == null) continue;
            systemJson.bodies.Add(ConvertMoverToJSON(mover));
        }

        string path = Path.Combine(Application.dataPath, "RocketSaves", fileName);
        File.WriteAllText(path, JsonUtility.ToJson(systemJson, true));
        Debug.Log($"Saved {systemJson.bodies.Count} rockets to {path}");
    }

    [ContextMenu("Load rockets from JSON")]
    public void LoadFromJSON()
    {
        string path = Path.Combine(Application.dataPath, "RocketSaves", fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"JSON file not found at {path}");
            return;
        }

        var jsonText = File.ReadAllText(path);
        var systemJson = JsonUtility.FromJson<SystemJSON>(jsonText);
        if (systemJson?.bodies == null)
        {
            Debug.LogError("Failed to parse rockets JSON");
            return;
        }

        var rockets = GameObject.FindGameObjectsWithTag(RocketTag);
        var rocketByName = new Dictionary<string, OrbitMover>(StringComparer.Ordinal);
        foreach (var go in rockets)
        {
            var mover = go.GetComponent<OrbitMover>();
            if (mover != null && !rocketByName.ContainsKey(go.name))
                rocketByName.Add(go.name, mover);
        }

        foreach (var body in systemJson.bodies)
        {
            if (!rocketByName.TryGetValue(body.name, out var mover))
            {
                Debug.LogWarning($"Rocket '{body.name}' not found in scene");
                continue;
            }

            CopyDTOtoOrbitData(body.orbitData, mover.orbitData);
            var attr = body.attractorSettings;
            if (!string.IsNullOrEmpty(attr.attractorName)
                && rocketByName.TryGetValue(attr.attractorName, out var parentMover))
            {
                mover.AttractorSettings.AttractorObject = parentMover.transform;
                mover.AttractorSettings.AttractorMass = attr.attractorMass;
            }

            mover.LockOrbitEditing = true;
            mover.ForceUpdateViewFromInternalState();
            mover.ForceUpdateVelocityHandleFromInternalState();
        }

        Debug.Log($"Loaded {systemJson.bodies.Count} rockets from JSON");
    }

    private BodyNodeJSON ConvertMoverToJSON(OrbitMover mover)
    {
        var dto = new OrbitDataDTO();
        var dataFields = typeof(OrbitData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var dtoFields = typeof(OrbitDataDTO).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in dataFields)
        {
            var target = Array.Find(dtoFields, x => x.Name == f.Name);
            if (target != null)
                target.SetValue(dto, f.GetValue(mover.orbitData));
        }

        string attrName = mover.AttractorSettings?.AttractorObject?.name ?? string.Empty;
        return new BodyNodeJSON
        {
            name = mover.name,
            orbitData = dto,
            attractorSettings = new AttractorSettingsJSON
            {
                attractorName = attrName,
                attractorMass = mover.AttractorSettings?.AttractorMass ?? 0
            }
        };
    }

    private void CopyDTOtoOrbitData(OrbitDataDTO dto, OrbitData data)
    {
        var dtoFields = typeof(OrbitDataDTO).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var dataFields = typeof(OrbitData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in dtoFields)
        {
            var target = Array.Find(dataFields, x => x.Name == f.Name);
            if (target != null)
                target.SetValue(data, f.GetValue(dto));
        }
    }
}