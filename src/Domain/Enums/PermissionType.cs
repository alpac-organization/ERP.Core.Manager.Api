namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Defines the different types of permissions that can be granted
    /// to a user or role within the system. These permissions control
    /// what operations can be performed on a specific resource.
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// Allows the user to view or retrieve information
        /// without making any modifications.
        /// </summary>
        Read,

        /// <summary>
        /// Allows the user to create new records or resources
        /// within the system.
        /// </summary>
        Create,

        /// <summary>
        /// Allows the user to modify or update existing
        /// records or resources.
        /// </summary>
        Update,

        /// <summary>
        /// Allows the user to remove or delete existing
        /// records or resources from the system.
        /// </summary>
        Delete
    }
}