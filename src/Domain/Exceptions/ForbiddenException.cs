namespace PickupOrderSystem.Domain.Exceptions;

public class ForbiddenException(string message = "Acesso negado.") : Exception(message);
