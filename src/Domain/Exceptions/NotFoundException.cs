namespace PickupOrderSystem.Domain.Exceptions;

public class NotFoundException(string message = "Recurso não encontrado.") : Exception(message);
