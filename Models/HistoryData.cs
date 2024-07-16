using System;
using System.Collections.Generic;
using System.Collections.Specialized;

public class IntegratedOutput
{
    public string detailed_prediction { get; set; }
    public string general_prediction { get; set; }
}

public class QuantitativeOutput
{
    public Dictionary<DateTime, double> Data { get; set; }
}

public class HistoryData
{
    public string Id { get; set; }
    public string ticker { get; set; }
    public string time { get; set; }
    public string actual_result { get; set; }
    public double closing_price { get; set; }
    public DateTime Date { get; set; }
    public IntegratedOutput integrated_output { get; set; }
    public object newsOutput { get; set; } // Adjust the type if you know what it should be
    public List<object> quantitativeOutput { get; set; } // Adjust the type if you know what it should be
    public string real_result { get; set; } // New property
}
