namespace NovaLearn.Domain.Audit;

/// <summary>Which part of the platform an audited action touched.</summary>
public enum AuditCategory
{
    UserManagement,
    Courses,
    Departments,
    Finance,
    Settings,
    Security
}
