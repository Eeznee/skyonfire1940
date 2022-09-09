using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class Boutique : MonoBehaviour
{
    public IStoreController controller;
    public AircraftCard[] cards;
    public string[] purchaseIds;
    public string workshop;
    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == "pack.workshop")
        {
            BuyWorkshop();
            return;
        }
        for (int i = 0; i < purchaseIds.Length; i++)
        {
            if (purchaseIds[i]== product.definition.id)
            {
                BuyAircraft(cards[i]);
                return;
            }
        }
    }
    public void BuyAircraft(AircraftCard card)
    {
        PlayerPrefs.SetInt(card.fileName, 1);
    }
    public void BuyWorkshop()
    {
        PlayerPrefs.SetInt("workshop", 1);
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        if (reason == PurchaseFailureReason.DuplicateTransaction) { OnPurchaseComplete(product); }
    }
}
