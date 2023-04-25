using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
public class MyBoutique : MonoBehaviour, IStoreListener
{
    public Button spitfire;
    public Button bf110;
    public Button workshop;

    private IStoreController controller;
    private IExtensionProvider extensions;

    public string spitfireId = "aircrafts.spitfire_mki_cannons";
    public string bf110Id = "aircrafts.bf110_c6";
    public string workshopId = "pack.workshop";
    const string k_Environment = "production";

    void IStoreListener.OnInitializeFailed(InitializationFailureReason error,string s)
    {
        
    }

    void Awake()
    {
        var options = new InitializationOptions().SetEnvironmentName(k_Environment);
        UnityServices.InitializeAsync(options);
    }
    void Start()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct("aircrafts.bf110_c6", ProductType.NonConsumable);
        builder.AddProduct("aircrafts.spitfire_mki_cannons", ProductType.NonConsumable);
        builder.AddProduct("pack.workshop", ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
        UpdateButtons();
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;
    }
    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }
    public void Restore()
    {
        extensions.GetExtension<IAppleExtensions>().RestoreTransactions(OnRestore);
    }
    void OnRestore(bool success,string callback)
    {
        var restoreMessage = "";
        if (success)
        {
            // This does not mean anything was restored,
            // merely that the restoration process succeeded.
            restoreMessage = "Restore Successful";
        }
        else
        {
            // Restoration failed.
            restoreMessage = "Restore Failed";
        }

        Debug.Log(restoreMessage);
    }
    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        //Retrieve the purchased product
        var product = e.purchasedProduct;

        //Add the purchased product to the players inventory
        PurchaseProduct(product);
        return PurchaseProcessingResult.Complete;
    }
    private void PurchaseProduct(Product p)
    {
        if (p.definition.id == spitfireId)
        {
            PlayerPrefs.SetInt("spitfire_mki_cannons", 1);
        }
        else if (p.definition.id == bf110Id)
        {
            PlayerPrefs.SetInt("bf_110_c6", 1);
        }
        else if (p.definition.id == workshopId)
        {
            PlayerPrefs.SetInt("workshop", 1);
        }
        UpdateButtons();
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        if (p == PurchaseFailureReason.DuplicateTransaction)
        {
            PurchaseProduct(i);
        }
    }

    private void UpdateButtons()
    {
        if (HasProduct(spitfireId)) DisableButton(spitfire);
        if (HasProduct(bf110Id)) DisableButton(bf110);
        if (HasProduct(workshopId)) DisableButton(workshop);
    }
    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponentInChildren<Text>().text = "Owned !";
    }
    bool HasProduct(string id)
    {
        Product noAdsProduct = controller.products.WithID(id);
        return noAdsProduct != null && noAdsProduct.hasReceipt;
    }

    public void BuySpitfire()
    {
        controller.InitiatePurchase(spitfireId);
    }
    public void BuyBf110()
    {
        controller.InitiatePurchase(bf110Id);
    }
    public void BuyWorkshop()
    {
        controller.InitiatePurchase(workshopId);
    }
}
