public interface IPlaceTarget
{
    string Prompt { get; }
    bool CanPlace(IPickupable item);
    void Place(IPickupable item);
}
