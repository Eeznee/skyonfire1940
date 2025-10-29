using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing.Extension;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

namespace Samples.Purchasing.Core.BuyingConsumables
{
    public class MyBoutiqueV5 : MonoBehaviour
    {
        /*
        public Button spitfire;
        public Button bf110;
        public Button workshop;

        private IExtensionProvider extensions;

        public string spitfireId = "aircrafts.spitfire_mki_cannons";
        public string bf110Id = "aircrafts.bf110_c6";
        public string workshopId = "pack.workshop";
        const string k_Environment = "production";
        async void InitializeIAP()
        {
            StoreController controller = UnityIAPServices.StoreController();

            controller.OnPurchasePending += OnPurchasePending;

            await controller.Connect();

            controller.OnProductsFetched += OnProductsFetched;
            controller.OnPurchasesFetched += OnPurchasesFetched;

            var initialProductsToFetch = new List<ProductDefinition>
                {
                    new(spitfireId, ProductType.NonConsumable),
                    new(bf110Id, ProductType.Consumable),
                    new(workshopId, ProductType.Consumable)
                };

            controller.FetchProducts(initialProductsToFetch);

            UpdateButtons();
        }
        void OnProductsFetched(List<Product> products)
        {
            // Handle fetched products  
            UnityIAPServices.StoreController().FetchPurchases();
}
        void OnPurchasesFetched(Orders orders)
        {
            // Process purchases, e.g. check for entitlements from completed orders  
        }
        public void OnPurchasePending(PendingOrder p)
        {
            foreach (CartItem cartItem in p.CartOrdered.Items())
            {
                PurchaseProduct(cartItem.Product);
            }
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
        void Awake()
        {
            var options = new InitializationOptions().SetEnvironmentName(k_Environment);
            UnityServices.InitializeAsync(options);
        }

        public void Restore()
        {
            UnityIAPServices.StoreController().RestoreTransactions(OnRestore);
        }
        void OnRestore(bool success, string callback)
        {
            Debug.Log(success ? "Restore Successful" : "Restore Failed");
        }


        /// <summary>
        /// Called when a purchase fails.
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (failureReason == PurchaseFailureReason.DuplicateTransaction)
            {
                PurchaseProduct(product);
            }
        }
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            if (failureDescription.reason == PurchaseFailureReason.DuplicateTransaction)
            {
                PurchaseProduct(product);
            }
        }

        private void UpdateButtons()
        {
            if (HasProduct(spitfireId)) DisableButton(spitfire);
            if (HasProduct(bf110Id)) DisableButton(bf110);
            if (HasProduct(workshopId)) DisableButton(workshop);


            Product spitfireProduct = UnityIAPServices.StoreController().GetProductById(spitfireId);
            spitfire.GetComponentInChildren<Text>().text = "Purchase for " + spitfireProduct.PriceString();

            Product bf110Product = UnityIAPServices.StoreController().GetProductById(bf110Id);
            bf110.GetComponentInChildren<Text>().text = "Purchase for " + bf110Product.PriceString();

            Product workshopProduct = UnityIAPServices.StoreController().GetProductById(workshopId);
            workshop.GetComponentInChildren<Text>().text = "Purchase for " + workshopProduct.PriceString();
        }

        private void DisableButton(Button button)
        {
            button.interactable = false;
            button.GetComponentInChildren<Text>().text = "Owned !";
        }
        bool HasProduct(string id)
        {
            Product noAdsProduct = UnityIAPServices.StoreController().GetProductById(id);
            return noAdsProduct != null && !string.IsNullOrEmpty(noAdsProduct.receipt);
        }

        public void BuySpitfire()
        {
            UnityIAPServices.StoreController().PurchaseProduct(spitfireId);
        }
        public void BuyBf110()
        {
            UnityIAPServices.StoreController().PurchaseProduct(bf110Id);
        }
        public void BuyWorkshop()
        {
            UnityIAPServices.StoreController().PurchaseProduct(workshopId);
        }
    }
    public static class BoutiqueExtensions
    {
        public static string PriceString(this Product product)
        {
            return product.metadata.localizedPriceString + " " + product.metadata.isoCurrencyCode;
        }
        */
    }

}
