namespace MedConnect.Core.Abstractions;

public interface IRefreshTokenGenerator
{
    string Generate();

    string ComputeHash(string token);
}