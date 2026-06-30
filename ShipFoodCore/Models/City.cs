namespace ShipFood.Models;

public class City
{
    public City()
    {
        nameCity = "TP. Hồ Chí Minh";
        districts = new List<District>();
        string[] names = {
            "Quận 1", "Quận 3", "Quận 5", "Quận 7", "Quận 10",
            "Bình Thạnh", "Tân Bình", "Gò Vấp", "Phú Nhuận",
            "Thủ Đức", "Bình Dương", "Hóc Môn", "Củ Chi"
        };
        foreach (string n in names)
        {
            districts.Add(new District(n));
        }
    }

    public string nameCity { get; set; } = "TP. Hồ Chí Minh";
    public List<District> districts { get; set; } = new();
}
