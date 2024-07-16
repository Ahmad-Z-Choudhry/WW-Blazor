namespace MyBlazorApp.Models
{
    public class MyDataModel
    {
        private string ticker;

        public string GetTicker()
        {
            return ticker;
        }

        public void SetTicker(string value)
        {
            ticker = value;
        }

        public string Name { get; set; }
        public double Price { get; set; }
        // Add other properties as needed
    }
}