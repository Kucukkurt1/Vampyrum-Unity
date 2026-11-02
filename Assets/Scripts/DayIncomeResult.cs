using System.Collections.Generic;

public class DayIncomeResult
{
    public int TotalHostingCost;
    public int TotalRefunds;
    public int TotalIncome => TotalRefunds;
    public int TotalNetChange => TotalRefunds - TotalHostingCost;
    public readonly List<string> Lines = new List<string>();
}
