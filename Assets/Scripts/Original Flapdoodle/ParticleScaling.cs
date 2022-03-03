using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleScaling : MonoBehaviour
{
    private void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }
}
