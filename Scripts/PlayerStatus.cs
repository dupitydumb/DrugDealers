using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerStatus
{
    public int Health { get; set; }
    public int Reputation { get; set; }
    public float Funds { get; set; }

    private Slider healthSlider;

    public void Start()
    {
        Funds = Manager.Instance.player.Funds;
        healthSlider = GameObject.FindWithTag("HealthSlider").GetComponent<Slider>();
        healthSlider.maxValue = 100;
        healthSlider.value = Health;
    }
    public PlayerStatus(int health, int reputation)
    {
        Health = health;
        Reputation = reputation;
    }

    public void UpdateUI()
    {
        GameObject healthSlider = GameObject.FindWithTag("HealthSlider");
        healthSlider.GetComponent<Slider>().value = Health;
    }
}


public class PlayerPocket
{
    public int Money { get; set; }
    public int Ammo { get; set; }

    public PlayerPocket(int money, int ammo)
    {
        Money = money;
        Ammo = ammo;
    }
}
