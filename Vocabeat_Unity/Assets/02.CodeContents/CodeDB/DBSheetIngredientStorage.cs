using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IngredientData
{
    public delegate void ValueChangedHandler(IngredientData ingredientData, int currentValue, int prevValue);

    public event ValueChangedHandler OnValueChagned;
    public event ValueChangedHandler OnValueMax;
    public event ValueChangedHandler OnValueMin;
    
    public int TID;
    public int Amount;

    public IngredientData(int tid, int amount)
    {
        TID = tid;
        Amount = amount;
    }

    public void AddAmount(int amount)
    {
        int prevAmount = Amount;
        Amount = Mathf.Clamp(Amount + amount, 0, int.MaxValue); // Max값이 정해져 있다면 바뀔지도

        TryInvokeValueChangedEvent(Amount, prevAmount);
    }

    public void RemoveAmount(int amount)
    {
        int prevAmount = Amount;
        Amount = Mathf.Clamp(Amount - amount, 0, int.MaxValue); 

        TryInvokeValueChangedEvent(Amount, prevAmount);
    }

    private void TryInvokeValueChangedEvent(int currentValue, int prevValue)
    {
        if(currentValue != prevValue)
        {
            OnValueChagned?.Invoke(this, currentValue, prevValue);
            if (currentValue == 0)
                OnValueMin?.Invoke(this, currentValue, prevValue);
            else if (currentValue == int.MaxValue)
                OnValueMax?.Invoke(this, currentValue, prevValue);
        }
    }
}

public class DBSheetIngredientStorage
{   
    
}
