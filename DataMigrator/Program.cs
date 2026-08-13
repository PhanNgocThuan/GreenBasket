using System;
using System.Linq;
using System.Data;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string sourceConnStr = @"Server=(localdb)\MSSQLLocalDB;Database=GreenBasketDB;Trusted_Connection=True;TrustServerCertificate=True;";
        string destConnStr = "Server=localhost,1433;Database=GreenBasketDB;User Id=sa;Password=SuperSecret@123;TrustServerCertificate=True;";

        string[] tables = new[] {
            "Roles", "Users", "Farms", "DiscountCodes", "DeliverySlots", "Products", 
            "UserRoles", "UserClaims", "UserLogins", "UserTokens", "RoleClaims",
            "Addresses", "Carts", "Orders", "Batches", "CartItems", "OrderItems"
        };

        using var destConn = new SqlConnection(destConnStr);
        destConn.Open();
        
        // Disable constraints and clear tables
        using (var cmd = destConn.CreateCommand())
        {
            cmd.CommandText = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';";
            cmd.ExecuteNonQuery();
        }
        
        var reverseTables = tables.Reverse().ToArray();
        foreach (var table in reverseTables)
        {
            using var cmd = destConn.CreateCommand();
            cmd.CommandText = $"SET QUOTED_IDENTIFIER ON; DELETE FROM [{table}];";
            cmd.ExecuteNonQuery();
        }

        using var sourceConn = new SqlConnection(sourceConnStr);
        sourceConn.Open();

        foreach (var table in tables)
        {
            Console.WriteLine($"Copying {table}...");
            using var cmd = sourceConn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{table}]";
            using var reader = cmd.ExecuteReader();

            using var bulkCopy = new SqlBulkCopy(destConn, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls, null);
            bulkCopy.DestinationTableName = $"[{table}]";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string colName = reader.GetName(i);
                bulkCopy.ColumnMappings.Add(colName, colName);
            }
            try
            {
                bulkCopy.WriteToServer(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying {table}: {ex.Message}");
            }
        }

        // Re-enable constraints
        using (var cmd = destConn.CreateCommand())
        {
            cmd.CommandText = "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all';";
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("Migration complete!");
    }
}
