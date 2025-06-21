using System;
using UnityEngine;

namespace MUSOAR
{
    public class PlayerSuitEnergy : MonoBehaviour
    {
        [SerializeField] private float maxSuitEnergy = 100f;
        [SerializeField] private float energyRegenRate = 20f;
        [SerializeField] private float regenDelay = 2f;

        private float currentSuitEnergy;
        private float lastEnergySpendTime;

        private void Awake()
        {
            currentSuitEnergy = maxSuitEnergy;
            lastEnergySpendTime = -regenDelay;
        }

        private void Update()
        {
            HandleEnergyRegeneration();
        }

        private void HandleEnergyRegeneration()
        {
            if (currentSuitEnergy < maxSuitEnergy && Time.time - lastEnergySpendTime >= regenDelay)
            {
                IncreaseSuitEnergy(energyRegenRate);
            }
        }

        private void DecreaseSuitEnergy(float decreaseAmount)
        {
            currentSuitEnergy = Mathf.Max(0f, currentSuitEnergy - decreaseAmount * Time.deltaTime);
            GlobalEventManager.OnPlayerStaminaChange.Invoke(currentSuitEnergy / maxSuitEnergy);
            lastEnergySpendTime = Time.time;
        }

        private void IncreaseSuitEnergy(float increaseAmount)
        {
            currentSuitEnergy = Mathf.Min(maxSuitEnergy, currentSuitEnergy + increaseAmount * Time.deltaTime);
            GlobalEventManager.OnPlayerStaminaChange.Invoke(currentSuitEnergy / maxSuitEnergy);
        }

        private void SetSuitEnergy(float setAmount)
        {
            currentSuitEnergy = Mathf.Clamp(setAmount, 0f, maxSuitEnergy);
            Debug.Log("SetSuitEnergy: " + currentSuitEnergy);
            GlobalEventManager.OnPlayerStaminaChange.Invoke(currentSuitEnergy / maxSuitEnergy);
        }

        public bool TrySpendEnergy(float amount)
        {
            if (currentSuitEnergy >= amount * Time.deltaTime)
            {
                DecreaseSuitEnergy(amount);
                return true;
            }
            return false;
        }

        public bool TrySpendEnergyInstant(float amount)
        {
            if (currentSuitEnergy >= amount)
            {
                currentSuitEnergy -= amount;
                GlobalEventManager.OnPlayerStaminaChange.Invoke(currentSuitEnergy / maxSuitEnergy);
                lastEnergySpendTime = Time.time;
                return true;
            }
            return false;
        }
    }
}
