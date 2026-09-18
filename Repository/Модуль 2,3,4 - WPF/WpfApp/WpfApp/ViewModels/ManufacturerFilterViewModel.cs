using WpfApp.Models;

namespace WpfApp.ViewModels;

public class ManufacturerFilterViewModel
{
    public Manufacturer? Manufacturer { get; }

    public string Name =>
        Manufacturer == null
            ? "Все производители"
            : Manufacturer.Name;

    public ManufacturerFilterViewModel(Manufacturer? manufacturer)
    {
        Manufacturer = manufacturer;
    }
}