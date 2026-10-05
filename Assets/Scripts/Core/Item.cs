// Something the engineer carries. For now, fuel cells.
public class Item
{
    public string Name { get; }
    public float Charge { get; }          // charge in %, from 0 to 100

    public Item(string name, float charge)
    {
        Name = name;
        Charge = charge;
    }

    public override string ToString()
    {
        return Name + " (" + Charge.ToString("0") + "%)";
    }
}
