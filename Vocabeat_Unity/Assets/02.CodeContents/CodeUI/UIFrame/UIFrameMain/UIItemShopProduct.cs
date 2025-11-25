using System;
using TMPro;
using UnityEngine;

public enum EProductType
{
    Key,
    SongPack,
}

[Serializable]
public class ShopProductData
{
    public EProductType ProductType;
    public int Price;
    public int Amount;
}

public class UIItemShopProduct : MonoBehaviour
{
    [SerializeField] private ShopProductData _shopData;

    [SerializeField] private TMP_Text _textAmount;
    [SerializeField] private TMP_Text _textPrice;

    [SerializeField] private UIWidgetMainShop _widgetShop;
    [SerializeField] private UIWidgetPurchasePopup _purchasePopup;

    private void Awake()
    {
        if (_shopData == null)
            return;

        if (_textAmount != null)
            _textAmount.text = $"{_shopData.Amount:N0}";
        _textPrice.text = $"{_shopData.Price:N0} ₩";
    }

    public void OnPurchase()
    {
        if (_shopData.ProductType == EProductType.Key)
            ManagerRhythm.Instance.AddMusicKey(_shopData.Amount);
        else if (_shopData.ProductType == EProductType.SongPack)
            ManagerUnlock.Instance.UnlockAllSongs();

        _widgetShop.RefreshKey();
        _purchasePopup.DoUIWidgetShow();
    }
}
