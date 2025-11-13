namespace Domain.Enums
{
    /// <summary>
    /// Defines communication/organizational hierarchy levels
    /// Lower numbers = higher hierarchy level
    /// </summary>
    public enum CommunicationLevel
    {
        Executive = 1,          // CEO, President, C-Level
        SeniorManagement = 2,   // VP, SVP, EVP
        MiddleManagement = 3,   // Directors, Senior Managers
        Management = 4,         // Managers, Team Leads
        Supervisor = 5,         // Supervisors
        Staff = 6,              // Individual Contributors
        Entry = 7               // Junior, Interns
    }
}
