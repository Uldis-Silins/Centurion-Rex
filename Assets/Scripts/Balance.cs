using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Currently using this for notes. 
/// Would be cool if could change values here and they are updated elswhere in the game
/// </summary>
/// 

public class Unit 
{
    float cost;
    float hp;
    float attack;
    float trainingTime;
    public Unit(float unitCost, float unitHp, float unitAttack, float trainingTime)
    {
        cost = unitCost;
        hp = unitHp;
        attack = unitAttack;
        this.trainingTime = trainingTime;
    }
}
public class Balance : MonoBehaviour
{
    
    void UnitStats()
    {
        ///  ===== ROMANS =======
        /* Soldier
         * - cost efficient 
         * - average hp
         * - strong attack
         */
        Unit playerLegionnaire = new Unit(
            unitCost: 20,
            unitHp: 20,
            unitAttack: 14,
            trainingTime: 2.5f);
          Unit aiLegionnaire = new Unit(
            unitCost: 20,
            unitHp: 21,
            unitAttack: 14,
            trainingTime: 2.5f);

        /* Archer
         * - expensive
         * - low hp
         * - low attack
         * - range advantage
         */
        Unit playerRaptor = new Unit(
            unitCost: 50,
            unitHp: 15,
            unitAttack: 6,
            trainingTime: 4);
        Unit aiRaptor = new Unit(
            unitCost: 50,
            unitHp: 20,
            unitAttack: 6,
            trainingTime: 4);

        /* Cavalry
         * - time efficient 
         * - high hp
         * - medium attack
         */
        Unit playerTrex = new Unit(
            unitCost: 80,
            unitHp: 70,
            unitAttack: 15,
            trainingTime: 5);
        Unit aiTrex = new Unit(
           unitCost: 80,
           unitHp: 70,
           unitAttack: 15,
           trainingTime: 5);

        ///  ===== SHARKS =======
        /* Soldier
         * - cost efficient 
         * - average hp
         * - strong attack
         * - faster than legion
         */
        Unit playerSamurai = new Unit(
            unitCost: 21,
            unitHp: 18,
            unitAttack: 15,
            trainingTime: 2.5f);
          Unit aiSamurai = new Unit(
            unitCost: 21,
            unitHp: 18,
            unitAttack: 15,
            trainingTime: 2.5f);
        // maxAccel 31

        /* Archer
         * - expensive
         * - low hp (lower than raptor)
         * - low attack (lower than raptor)
         * - range advantage (bigger than raptor)
         */
        Unit playerNamazu = new Unit(
            unitCost: 50,
            unitHp: 12,
            unitAttack: 5,
            trainingTime: 4);
            // range 12
        Unit aiNamazu = new Unit(
            unitCost: 50,
            unitHp: 12,
            unitAttack: 5,
            trainingTime: 4);

        /* Cavalry
         * - time efficient 
         * - medium hp
         * - high attack
         */
        Unit playerShark = new Unit(
            unitCost: 75,
            unitHp: 55,
            unitAttack: 20,
            trainingTime: 5);
        Unit aiShark = new Unit(
           unitCost: 75,
           unitHp: 55,
           unitAttack: 20,
           trainingTime: 5);



    }
}
