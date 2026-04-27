using UnityEngine;
using System;

public class PlayerExp : MonoBehaviour
{
    public static PlayerExp Instance;

    [SerializeField] private float currentExp = 0;
    [SerializeField] private int currentLevel = 1;

    public float CurrentExp => currentExp;
    public int CurrentLevel => currentLevel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    public void AddExp(float amount)
    {
        currentExp += amount;
        // level up logic could go here
        // Debug.Log($"Player gained {amount} EXP. Total: {currentExp}");
    }
}
