//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Data Access Application Block
//===============================================================================

using System;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace EntLibContrib.Data.PostgreSql
{
	/// <summary>
	/// Represents the process to build an instance of <see cref="NpgsqlDatabase"/> described by configuration information.
	/// </summary>
	/// <author>Oleg Ufaev</author>
	public class NpgsqlDatabaseAssembler : IDatabaseAssembler
	{
		#region Public Methods
		/// <summary>
		/// Builds an instance of <see cref="NpgsqlDatabase"/>, based on the provided connection string.
		/// </summary>
		/// <param name="name">The name for the new database instance.</param>
		/// <param name="connectionStringSettings">The connection string for the new database instance.</param>
		/// <param name="configurationSource">The source for any additional configuration information.</param>
		/// <returns>The new postgres database instance.</returns>
		public Database Assemble(string name, ConnectionStringSettings connectionStringSettings, IConfigurationSource configurationSource)
		{
			return new NpgsqlDatabase(connectionStringSettings.ConnectionString);
		}
		#endregion
	}
}
