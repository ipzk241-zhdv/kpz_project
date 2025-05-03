using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[ExecuteAlways]
public class ObjectsLoader : MonoBehaviour
{
    public static ObjectsLoader Instance { get; private set; }

    public GameObject rootObject;
    public bool AutoRoot = true;
    public double G;

    [System.Serializable]
    public class OrbitJSON : OrbitData
    {
        public string objectName = "Planet";
        public string attractorName;
        public double G;
    }

    [System.Serializable]
    public class OrbitJSONList
    {
        public List<OrbitJSON> orbits;
        public OrbitJSONList(List<OrbitJSON> orbits)
        {
            this.orbits = orbits;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (AutoRoot)
        {
            rootObject = transform.gameObject;
        }
    }

    [ContextMenu("Load from JSON")]
    public void LoadFromJSON()
    {
        string path = $"{Application.dataPath}/Scripts/Kepler/Objects.json";
        if (!File.Exists(path))
        {
            Debug.LogError("Файл JSON не знайдено!");
            return;
        }

        string jsonText = File.ReadAllText(path);
        OrbitJSONList wrapper = JsonUtility.FromJson<OrbitJSONList>(jsonText);

        if (wrapper == null || wrapper.orbits == null)
        {
            Debug.LogError("Не вдалося зчитати дані з JSON!");
            return;
        }

        foreach (var orbitJSON in wrapper.orbits)
        {
            GameObject obj = GameObject.Find(orbitJSON.objectName);
            if (obj == null)
            {
                Debug.LogWarning($"Об'єкт {orbitJSON.objectName} не знайдено на сцені!");
                continue;
            }

            OrbitMover mover = obj.GetComponent<OrbitMover>();
            if (mover == null)
            {
                Debug.LogWarning($"OrbitMover на об'єкті {orbitJSON.objectName} не знайдено!");
                continue;
            }

            OrbitData orbitData = mover.orbitData;
            if (orbitData == null)
            {
                Debug.LogWarning($"OrbitData у OrbitMover на об'єкті {orbitJSON.objectName} не знайдено!");
                continue;
            }

            var velocity = obj.transform.Find("velocity");
            if (velocity == null)
            {
                Debug.LogWarning($"VelocityHandle на об'єкті {orbitJSON.objectName} не знайдено!");
                continue;
            }

            mover.LockOrbitEditing = false;

            OrbitJSONToOrbitData(orbitJSON, orbitData);
            if (mover.AttractorSettings != null)
            {
                GameObject attractorObj = GameObject.Find(orbitJSON.attractorName);
                if (attractorObj != null)
                {
                    mover.AttractorSettings.AttractorObject = attractorObj.transform;
                }
                else
                {
                    Debug.LogWarning($"Attractor {orbitJSON.attractorName} не знайдено на сцені для об'єкта {orbitJSON.objectName}!");
                }

                mover.AttractorSettings.AttractorMass = orbitJSON.AttractorMass;
                mover.AttractorSettings.GravityConstant = orbitJSON.G;
            }
            else
            {
                Debug.LogWarning($"AttractorSettings відсутній у OrbitMover на об'єкті {orbitJSON.objectName}!");
            }
            Transform attractorTransform = mover.AttractorSettings.AttractorObject;
            obj.transform.position = attractorTransform.TransformPoint(orbitData.positionRelativeToAttractor.ToVector3());
            velocity.position = attractorTransform.TransformPoint(orbitData.velocityRelativeToAttractor.ToVector3());

            mover.LockOrbitEditing = true;
        }
    }

    [ContextMenu("Save to JSON")]
    public void SaveToJSON()
    {
        if (rootObject == null)
        {
            return;
        }

        List<OrbitJSON> orbits = new List<OrbitJSON>();
        foreach (var om in rootObject.GetComponentsInChildren<OrbitMover>())
        {
            orbits.Add(OrbitToJSON(om.orbitData, om.name, om.AttractorSettings.AttractorObject.name));
        }

        OrbitJSONList wrapper = new OrbitJSONList(orbits);
        File.WriteAllText($"{Application.dataPath}/Scripts/Kepler/Objects.json", JsonUtility.ToJson(wrapper, true));
    }

    [ContextMenu("Destroy all")]
    public void DestroyAllBodies() { }

    private void OrbitJSONToOrbitData(object orbitJSON, object orbitData)
    {
        var orbitJSONFields = typeof(OrbitJSON).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var orbitDataFields = typeof(OrbitData).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var orbitDataField in orbitDataFields)
        {
            var matchingField = Array.Find(orbitJSONFields, f => f.Name == orbitDataField.Name);
            if (matchingField != null)
            {
                object value = matchingField.GetValue(orbitJSON);
                orbitDataField.SetValue(orbitData, value);
            }
        }
    }

    public OrbitJSON OrbitToJSON(OrbitData orbitData, string name, string attrName)
    {
        var orbitDataFields = typeof(OrbitData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var orbitJSONFields = typeof(OrbitJSON).GetFields(BindingFlags.Public | BindingFlags.Instance);

        OrbitJSON orbitJSON = new OrbitJSON();
        orbitJSON.objectName = name;
        orbitJSON.attractorName = attrName;
        orbitJSON.G = G;

        foreach (var orbitDataField in orbitDataFields)
        {
            var matchingField = Array.Find(orbitJSONFields, f => f.Name == orbitDataField.Name);
            if (matchingField != null)
            {
                object value = orbitDataField.GetValue(orbitData);
                matchingField.SetValue(orbitJSON, value);
            }
        }

        return orbitJSON;
    }
}
