using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SmoothieMachine : MonoBehaviour
{
    public IngredientData ingredientData;
    public GameObject progressBar;
    private readonly float TimeIncrementWhenNewItemAdded = 2f;
    private readonly float MachineTopItemXSpace = 0.2f;
    private readonly Vector3 MachineToItemOffset = new(0f, 1f, -0.1f);
    private readonly List<Ingredient> ingredients = new();
    private readonly List<GameObject> topItems = new();
    private float productCountdown = 0f;
    private float productCountdownMax;

    private void Update()
    {
        if (productCountdown > 0f)
        {
            Vector3 scale = progressBar.transform.localScale;
            scale.x = productCountdown / productCountdownMax;
            progressBar.transform.localScale = scale;
            progressBar.SetActive(true);
            productCountdown -= Time.deltaTime;
            if (productCountdown <= 0f)
            {
                ProduceProduct();
                productCountdownMax = 0f;
            }
        }
        else
        {
            productCountdown = 0f;
            progressBar.SetActive(false);
        }
    }

    public bool AddIngredient(Item item)
    {
        if (item == null || item.type != Item.Type.Ingredient)
        {
            return false;
        }
        var ingredient = item.ingredients[0];
        if (ingredient.type == Ingredient.Type.Liquid
            && ingredients.Any(info => info.type == Ingredient.Type.Liquid))
        {
            Debug.Log("Ignored because there is already a liquid base: " + ingredient.prefab.name);
            return false;
        }
        else
        {
            Debug.Log("Added ingredient: " + ingredient.prefab.name);
            ingredients.Add(ingredient);
            AddTopItem(ingredient);
            UpdateProductCountdown();
            return true;
        }
    }

    private void AddTopItem(Ingredient ingredient)
    {
        float offset = -topItems.Count * MachineTopItemXSpace;
        foreach (var item in topItems)
        {
            item.transform.localPosition = Util.ChangeX(item.transform.localPosition, offset);
            offset += MachineTopItemXSpace * 2;
        }
        var newItem = Instantiate(ingredient.prefab);
        newItem.transform.SetParent(transform);
        newItem.transform.localPosition = new(offset, MachineToItemOffset.y, MachineToItemOffset.z);
        topItems.Add(newItem);
    }

    private void UpdateProductCountdown()
    {
        productCountdown += TimeIncrementWhenNewItemAdded;
        productCountdownMax += TimeIncrementWhenNewItemAdded;
    }

    private void ProduceProduct()
    {
        ClearTopItems();
        var smoothie = ingredientData.GetSmoothieObj(ingredients);
        smoothie.transform.SetParent(transform);
        smoothie.transform.localPosition = MachineToItemOffset;
        topItems.Add(smoothie);
    }

    public Item GetProduct()
    {
        if (topItems.Count > 0 && productCountdown <= 0f)
        {
            var item = new Item
            {
                obj = topItems[0],
                type = Item.Type.Smoothie,
                ingredients = new List<Ingredient>(ingredients)
            };
            topItems.Clear();
            ingredients.Clear();
            return item;
        }
        return null;
    }

    private void ClearTopItems()
    {
        foreach (var item in topItems)
        {
            Destroy(item);
        }
        topItems.Clear();
    }
}
