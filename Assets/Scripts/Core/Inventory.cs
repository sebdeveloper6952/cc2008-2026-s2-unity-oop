using System.Collections.Generic;
using UnityEngine;

// What the engineer is carrying.
public class Inventory : MonoBehaviour
{
    private readonly List<Item> items = new List<Item>();

    public IReadOnlyList<Item> Items => items;
    public int Count => items.Count;

    public void Add(Item item)
    {
        items.Add(item);
        Hud.Instance.Toast("+ " + item);
    }

    public float TotalCharge()
    {
        float total = 0f;
        foreach (Item item in items)
        {
            total += item.Charge;
        }
        return total;
    }

    // Hands over everything and ends up empty.
    public List<Item> TakeAll()
    {
        List<Item> all = new List<Item>(items);
        items.Clear();
        return all;
    }
}
