namespace ERP.Core.Manager.Api.Domain.Enums
{
    /// <summary>
    /// Defines the different types of catalogs available in the system.
    /// Catalog types are used to categorize configurable information
    /// related to the organizational and operational structure of a company.
    /// </summary>
    public enum CatalogType
    {
        /// <summary>
        /// Represents the catalog of company branches.
        /// Each entry corresponds to a physical or operational location
        /// where the company performs its business activities.
        /// </summary>
        Branches = 1,

        /// <summary>
        /// Represents the catalog related to the company's organizational structure.
        /// This includes work areas and the job positions associated with those areas.
        /// </summary>
        OrganizationalStructure = 2,
    }
}