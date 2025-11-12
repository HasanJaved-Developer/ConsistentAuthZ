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

                -- Ensure ApiLogs module exists (Area='', Controller='ErrorLogs', Action='GetAllErrors')
                IF NOT EXISTS (
                    SELECT 1 FROM Modules 
                    WHERE Name = 'ApiLogs' AND Controller = 'ErrorLogs' AND Action = 'GetAllErrors'
                )
                BEGIN
                    INSERT INTO Modules ([Name],[Area],[Controller],[Action],[CategoryId],[Type])
                    VALUES ('ApiLogs','', 'ErrorLogs','GetAllErrors', @catId, 'Api');
                END

                DECLARE @modId INT;
                SELECT @modId = Id FROM Modules 
                WHERE Name='ApiLogs' AND Controller='ErrorLogs' AND Action='GetAllErrors';

                -- Ensure function Api.Logs.View exists
                IF NOT EXISTS (SELECT 1 FROM Functions WHERE [Code] = 'Api.Logs.View')
                BEGIN
                    INSERT INTO Functions ([Code],[DisplayName],[ModuleId])
                    VALUES ('Api.Logs.View','Api View Logs', @modId);
                END
            ");

    
            // add allan if missing (hash created here once & inlined)
            var hash = BCrypt.Net.BCrypt.HashPassword("allan");
            migrationBuilder.Sql($@"
                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserName]='allan')
                BEGIN
                    INSERT INTO [Users]([UserName],[Password]) VALUES ('allan','{hash}');
                END
            ");

            // link allan ↔ Operator if missing
            migrationBuilder.Sql(@"
                DECLARE @uid INT = (SELECT TOP 1 [Id] FROM [Users] WHERE [UserName]='allan');
                DECLARE @rid INT = (SELECT TOP 1 [Id] FROM [Roles] WHERE [Name]='Admin');

                IF @uid IS NOT NULL AND @rid IS NOT NULL
                AND NOT EXISTS(SELECT 1 FROM [UserRoles] WHERE [UserId]=@uid AND [RoleId]=@rid)
                BEGIN
                    INSERT INTO [UserRoles]([UserId],[RoleId]) VALUES (@uid,@rid);
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
                WHERE M.[Name] = 'ApiLogs' AND M.[Controller] = 'ErrorLogs' AND M.[Action] = 'GetAllErrors';
            ");
            
            // Drop the Type column
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Modules"
            );

            // Remove allan user and its roles
            migrationBuilder.Sql(@"
                DECLARE @uid INT = (SELECT TOP 1 [Id] FROM [Users] WHERE [UserName]='allan');
                IF @uid IS NOT NULL
                BEGIN
                    DELETE FROM [UserRoles] WHERE [UserId]=@uid;
                    DELETE FROM [Users] WHERE [Id]=@uid;
                END
            ");
        }

    }
}
