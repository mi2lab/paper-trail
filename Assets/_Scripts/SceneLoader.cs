using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSampleScene()
    {
        SceneManager.LoadScene("TorqueHandout");
    }

    public void LoadAuthoringTool()
    {
        SceneManager.LoadScene("Flapdoodle-ImmersivePaper");
    }
}
