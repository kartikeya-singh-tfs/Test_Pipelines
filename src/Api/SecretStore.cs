namespace ThermoFisher.Opal.Api;

/// <summary>
/// Placeholder implementation of secret store.
/// This is a temporary solution until secrets management (#17) is implemented.
/// </summary>
internal class SecretStore : ISecretStore
{
    /// <summary>
    /// Create a DB password for the given module user
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string CreateDbPassword(string userName)
    {
        // TODO: create strong password and store it in password store
        return userName;
    }

    /// <summary>
    /// Retrieve the database password for the global admin account (opal).
    /// </summary>
    /// <returns></returns>
    public string GetDbAdminPassword()
    {
        // TODO: use secure password store instead of this plaintext file
        return File.ReadAllLines("../../.pg_password")[0];
        // no error handling for this temporary hack
    }

    /// <summary>
    /// Get the DB password for the given module user
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string GetDbPassword(string userName)
    {
        // TODO: retrieve from password store
        return userName;
    }
}
