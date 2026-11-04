namespace BuyMyHouse.Application.Common.Exceptions;

public class NotFoundException(string resource, string key)
    : Exception($"{resource} with id '{key}' was not found.");
