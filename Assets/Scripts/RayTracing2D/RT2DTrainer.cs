using RayTracing2D;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public interface IRT2DTrainingSample : ITrainingSample
{
    void Setup(RT2DTrainer trainer, GameObject sceneRoot);
}

public class RT2DTrainer : MonoBehaviour, ITrainer
{
    public int SamplesToGenerate = 1000;
    public string sessionName = "BasicTraining";
    public double ConvergenceThreshold = 0.001;
    public int OutputSize = 1024;

    IRT2DTrainingSample[] _samples;
    int _currentSampleIndex;
    GameObject _currentSampleSceneRoot;

    public RT2DRenderer Renderer { get; set; }
    public IRT2DTrainingSample CurrentSample => _samples[_currentSampleIndex];
    IEnumerable<ITrainingSample> ITrainer.Samples => _samples;

    void Start()
    {
        _samples = new IRT2DTrainingSample[SamplesToGenerate];

        for (int i = 0; i < SamplesToGenerate; i++)
            _samples[i] = TestTrainingSample.CreateRandom();

        _currentSampleIndex = 0;
        RT2DRenderer.Trainer = this;
    }

    public void OnTrainingSampleStarting(RT2DRenderer renderer)
    {
        Renderer = renderer;
        Renderer.ConvergenceThreshold = ConvergenceThreshold;
        Renderer.TrainingWidth = OutputSize;
        Renderer.TrainingHeight = OutputSize;

        _currentSampleSceneRoot = new GameObject("$_TrainingScene");

        CurrentSample.Setup(this, _currentSampleSceneRoot);
    }

    public void OnTrainingInputRendered()
    {
        var folder = TrainingUtil.GetTrainingFolder(sessionName);

        Directory.CreateDirectory(folder);

        // Save input EXR
        Renderer.TrainingTarget.SaveTextureEXR(Path.Combine(folder, $"{CurrentSample.Name}_{_currentSampleIndex:0000}_In.exr"));
    }

    public void OnTrainingOutputRendered()
    {
        var folder = TrainingUtil.GetTrainingFolder(sessionName);

        // Save output PNG
        Renderer.TrainingTarget.SaveTextureEXR(Path.Combine(folder, $"{CurrentSample.Name}_{_currentSampleIndex:0000}_Out.exr"));

        Destroy(_currentSampleSceneRoot);
        _currentSampleSceneRoot = null;
        _currentSampleIndex++;

        if (_currentSampleIndex >= _samples.Length)
        {
            // If training generation is over, exit play mode.
            RT2DRenderer.Trainer = null;
            EditorApplication.ExitPlaymode();
            AssetDatabase.Refresh();
        }
    }
}

class TestTrainingSample : IRT2DTrainingSample
{
    public static Vector2 LightPosition_Min = new Vector2(-1, -1);
    public static Vector2 LightPosition_Max = new Vector2(1, 1);
    public static float LightRadius_Min = 0.1f;
    public static float LightRadius_Max = 5.0f;
    public static Color[] LightColor_Set = new Color[] 
    {
        Color.white, 
        new Color(0.95f, 0.1f, 0.1f, 1),
        new Color(0.1f, 0.95f, 0.1f, 1),
        new Color(0.1f, 0.1f, 0.95f, 1),
        new Color(0.95f, 0.95f, 0.1f, 1),
        new Color(0.95f, 0.1f, 0.95f, 1),
        new Color(0.1f, 0.95f, 0.95f, 1),
    };

    public static float LightBrightness_Min = 0.1f;
    public static float LightBrightness_Max = 1000.0f;

    public string Name { get; set; }
    public int Index { get; set; }
    public Vector2 LightPosition { get; set; }
    public float LightRadius { get; set; }
    public Color LightColor { get; set; }
    public float LightBrightness { get; set; }

    public TestTrainingSample(string name)
    {
        Name = name;
    }

    public static TestTrainingSample CreateRandom(string name = "Test")
    {
        return new TestTrainingSample(name)
        {
            LightPosition = TrainingUtil.RandomRanged(LightPosition_Min, LightPosition_Max),
            LightRadius = TrainingUtil.RandomRanged(LightRadius_Min, LightRadius_Max),
            LightColor = TrainingUtil.RandomFromSet(LightColor_Set),
            LightBrightness = TrainingUtil.RandomRangeLog(LightBrightness_Min, LightBrightness_Max)
        };
    }

    public void Setup(RT2DTrainer trainer, GameObject sceneRoot)
    {
        Camera.main.aspect = 1;

        var lightGO = new GameObject("Light", typeof(PointLightRT2D));
        lightGO.transform.parent = sceneRoot.transform;
        lightGO.transform.localPosition = LightPosition;

        var light = lightGO.GetComponent<PointLightRT2D>();
        light.radius = LightRadius;
        light.color = LightColor;
        light.intensity = LightBrightness;
    }
}