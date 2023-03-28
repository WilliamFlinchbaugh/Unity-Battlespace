using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour {   
    private float health;
    private BattleEnvController envController;
    public Team team;

    public void Start() {
        envController = GetComponentInParent<BattleEnvController>();
        health = envController.BaseHealth;
    }
    public bool Hit() {
        health--;
        if (health <= 0f) {
            return true;
        }
        return false;
    }
}
