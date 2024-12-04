using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Globalization;

public class DataManager : MonoBehaviour
{
	private Dictionary<int, ProductDataItem> DesignItems = new Dictionary<int, ProductDataItem>();
	private List<CartItem> CartItems = new List<CartItem>();
	private ProductDataItem originalProduct;
	private ProductDataItem selectedProduct;
	private int selectedSlotIndex;
	
	private List<ProductDataItem> favorites = new List<ProductDataItem>();
	
	public delegate void CartUpdatedDelegate(int quantity);
	public static event CartUpdatedDelegate OnCartInventoryChanged;
	
	public void Initialize () 
	{
		CartItems.Clear();
		//TODO read favorites from PlayerPrefs for now (should eventually be API)
	}
	
	public Dictionary<int, ProductDataItem> GetDesignData () 
	{
		return DesignItems;
	}
	
	public void SetDesignData (Dictionary<int, ProductDataItem> data) 
	{
		DesignItems.Clear();
		DesignItems = data;
	}
	
	public void RemoveDesignItem (int itemKey) 
	{
		DesignItems.Remove(itemKey);
	}
	
	public void SetSelectedProduct (ProductDataItem productDataItem) 
	{
		selectedProduct = productDataItem;
	}
	
	public ProductDataItem GetSelectedProduct () 
	{
		return selectedProduct;
	}
	
	public void SetOriginalProduct (ProductDataItem productDataItem, int slotIndex) 
	{
		originalProduct = productDataItem;
		selectedSlotIndex = slotIndex;
	}
	
	public void SwapOriginalProduct () 
	{
		originalProduct = selectedProduct;
		DesignItems[selectedSlotIndex] = selectedProduct;
	}

	public void AddItemToCart (ProductDataItem item, int quantity) 
	{
		bool alreadyInCart = false;
		foreach (CartItem cartItem in CartItems) 
		{
			if (cartItem.productData.productID == item.productID) 
			{
				cartItem.Increase(quantity);
				alreadyInCart = true;
				break;
			}
		}
		if (!alreadyInCart) 
		{
			CartItems.Add(new CartItem(item, quantity));
		}
		CartInventoryUpdated();
	}
	
	public void RemoveItemFromCart (ProductDataItem item) 
	{
		foreach (CartItem cartItem in CartItems) 
		{
			if (cartItem.productData.productID == item.productID) 
			{
				CartItems.Remove(cartItem);
				CartInventoryUpdated();
				break;
			}
		}
	}
	
	public void DecreaseCartCountForItem (ProductDataItem item) 
	{
		foreach (CartItem cartItem in CartItems) 
		{
			if (cartItem.productData.productID == item.productID) 
			{
				cartItem.Decrease();
				if (cartItem.quantity <= 0) 
				{
					CartItems.Remove(cartItem);
				}
				CartInventoryUpdated();
				break;
			}
		}
	}
	
	private void CartInventoryUpdated () 
	{
		 //Update UI progress bar here
		if (OnCartInventoryChanged != null)
		{
			OnCartInventoryChanged(GetCartItemCount());
		}
	}
	
	public int GetCartItemCount () 
	{
		int count = 0;
		foreach (CartItem cartItem in CartItems)  
		{
			count += cartItem.quantity;
		}
		return count;
	}
	
	public float GetSubtotal () 
	{
		float subtotal = 0;
		foreach (CartItem cartItem in CartItems)  
		{
			subtotal += cartItem.productData.price * cartItem.quantity;
		}
		return subtotal;
	}
	
	public List<CartItem> GetCart () 
	{
		return CartItems;
	}
	
	public void AddToFavorites (ProductDataItem item) 
	{
		foreach (ProductDataItem dataItem in favorites) 
		{
			if (dataItem.productID == item.productID) 
			{
				//already favorite, just exit
				return;
			}
		}
		Logger.Log("Saving to favorites: " + item.productName);
		favorites.Add(item);
	}
	
	public List<string> GetAllCountries()
    {
        SortedSet<string> countries= new SortedSet<string>();
        foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            var regionInfo = new RegionInfo(cultureInfo.Name);
            if (!countries.Contains(regionInfo.EnglishName))
            {
                countries.Add(regionInfo.EnglishName);
            }
        }
        return countries.ToList();
    }
	
	public List<string> GetAllStates()
	{
		List<string> states = new List<string>();
		states.Add("AL, Alabama");
		states.Add("AK, Alaska");
		states.Add("AZ, Arizona");
		states.Add("AR, Arkansas");
		states.Add("CA, California");
		states.Add("CO, Colorado");
		states.Add("CT, Connecticut");
		states.Add("DE, Delaware");
		states.Add("DC, District of Columbia");
		states.Add("FL, Florida");
		states.Add("GA, Georgia");
		states.Add("HI, Hawaii");
		states.Add("ID, Idaho");
		states.Add("IL, Illinois");
		states.Add("IN, Indiana");
		states.Add("IA, Iowa");
		states.Add("KS, Kansas");
		states.Add("KY, Kentucky");
		states.Add("LA, Louisiana");
		states.Add("ME, Maine");
		states.Add("MD, Maryland");
		states.Add("MA, Massachusetts");
		states.Add("MI, Michigan");
		states.Add("MN, Minnesota");
		states.Add("MS, Mississippi");
		states.Add("MO, Missouri");
		states.Add("MT, Montana");
		states.Add("NE, Nebraska");
		states.Add("NV, Nevada");
		states.Add("NH, New Hampshire");
		states.Add("NJ, New Jersey");
		states.Add("NM, New Mexico");
		states.Add("NY, New York");
		states.Add("NC, North Carolina");
		states.Add("ND, North Dakota");
		states.Add("OH, Ohio");
		states.Add("OK, Oklahoma");
		states.Add("OR, Oregon");
		states.Add("PA, Pennsylvania");
		states.Add("RI, Rhode Island");
		states.Add("SC, South Carolina");
		states.Add("SD, South Dakota");
		states.Add("TN, Tennessee");
		states.Add("TX, Texas");
		states.Add("UT, Utah");
		states.Add("VT, Vermont");
		states.Add("VA, Virginia");
		states.Add("WA, Washington");
		states.Add("WV, West Virginia");
		states.Add("WI, Wisconsin");
		states.Add("WY, Wyoming");

		return states;
	}
}
