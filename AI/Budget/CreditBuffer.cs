
using Godot;
using System.Linq;

public class CreditBuffer
{
    public Vector2[] Buffer { get; private set; }
    private int _index;

    public CreditBuffer(int size)
    {
        Buffer = new Vector2[size];
        _index = 0;
    }

    public float GetCredit()
    {
        return Buffer.Sum(v => v.X) - Buffer.Sum(v => v.Y);
    }

    public void Add(float credit, float spent)
    {
        Buffer[_index] = new Vector2(credit, spent);
        _index = (_index + 1) % Buffer.Length;
    }

    public void AddSpendingToCurrent(float spent)
    {
        Buffer[_index] += new Vector2(0f, spent);
    }

    public void AddCreditToCurrent(float credit)
    {
        Buffer[_index] += new Vector2(credit, 0f);
    }
}