using System;
using System.Collections;
using System.Collections.Generic;
using Gameboard.Examples;
using UnityEngine;

public class FXPlayerCrack : MonoBehaviour
{
    private MeshRenderer _mr;

    private static readonly int CrackAmount = Shader.PropertyToID("_crackAmount");
    public AnimationCurve effectRamp;
    public PlayerBehavior myPlayer;
    // Start is called before the first frame update
    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        myPlayer.GetPlayer().OnHitpointChange += OnHitpointChange;
    }

    private void OnDestroy()
    {
        myPlayer.GetPlayer().OnHitpointChange -= OnHitpointChange;
    }

    private void OnHitpointChange(int hp)
    {
        float percent = effectRamp.Evaluate(Mathf.InverseLerp(4000, 0, hp));
        UpdateCrackAmount(percent);
    }

    void UpdateCrackAmount(float newVal)
    {
        if (gameObject.activeInHierarchy)
        {
            _mr.material.SetFloat(CrackAmount, newVal);
        }
    }
}
