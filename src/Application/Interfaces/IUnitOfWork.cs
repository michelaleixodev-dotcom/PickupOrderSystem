namespace PickupOrderSystem.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
