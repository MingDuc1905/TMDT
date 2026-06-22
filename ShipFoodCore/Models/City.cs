namespace ShipFood.Models;

public class City
{
    public City()
    {
        nameCity = "Đà Nẵng";
        districts = new List<District>();
        string[] names = { "Hải Châu", "Thanh Khê", "Sơn Trà", "Ngũ Hành Sơn", "Liên Chiểu", "Cẩm Lệ", "Hòa Vang" };
        foreach (string n in names)
        {
            districts.Add(new District(n));
        }
    }

    public string nameCity { get; set; } = "Đà Nẵng";
    public List<District> districts { get; set; } = new();
}
