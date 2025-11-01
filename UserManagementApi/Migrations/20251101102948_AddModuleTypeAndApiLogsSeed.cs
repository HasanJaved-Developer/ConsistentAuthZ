using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleTypeAndApiLogsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add column as nullable first
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Modules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true
            );

            // 2) Backfill existing rows
            migrationBuilder.Sql(@"
                UPDATE Modules SET Type = 'WebApp' WHERE Type IS NULL;
            ");

            // 3) Make it required (non-nullable) after backfill
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Modules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // 4) Seed Category -> Module -> Function (idempotent)
            migrationBuilder.Sql(@"
                DECLARE @catId INT;

                -- Ensure Administration category exists
                SELECT @catId = Id FROM Categories WHERE Name = 'Administration';

                -- Ensure ApiLogs module exists (Area='', Controller='api', Action='errorlogs')
                IF NOT EXISTS (
                    SELECT 1 FROM Modules 
                    WHERE Name = 'ApiLogs' AND Controller = 'api' AND Action = 'errorlogs'
                )
                BEGIN
                    INSERT INTO Modules ([Name],[Area],[Controller],[Action],[CategoryId],[Type])
                    VALUES ('ApiLogs','', 'api','errorlogs', @catId, 'Api');
                END

                DECLARE @modId INT;
                SELECT @modId = Id FROM Modules 
                WHERE Name='ApiLogs' AND Controller='api' AND Action='errorlogs';

                -- Ensure function Api.Logs.View exists
                IF NOT EXISTS (SELECT 1 FROM Functions WHERE [Code] = 'Api.Logs.View')
                BEGIN
                    INSERT INTO Functions ([Code],[DisplayName],[ModuleId])
                    VALUES ('Api.Logs.View','Api View Logs', @modId);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove function
            migrationBuilder.Sql(@"
                DELETE F 
                FROM Functions F
                WHERE F.[Code] = 'Api.Logs.View';
            ");

            // Remove module (only the one we added)
            migrationBuilder.Sql(@"
                DELETE M
                FROM Modules M
                WHERE M.[Name] = 'ApiLogs' AND M.[Controller] = 'api' AND M.[Action] = 'errorlogs';
            ");
            
            // Drop the Type column
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Modules"
            );
        }

    }
}
