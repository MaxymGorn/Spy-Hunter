namespace WpfApp15.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAzureMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Files",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        path = c.String(),
                        DateTime = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        email = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.InfoPcs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ComputerName = c.String(),
                        ComputerCpu = c.String(),
                        ComputerGpu = c.String(),
                        ComputerRamAmount_MB = c.String(),
                        ComputerAntivirus = c.String(),
                        ComputerOs = c.String(),
                        Country = c.String(),
                        RegionName = c.String(),
                        City = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.LogTexts",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Desciption = c.String(),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LogTexts", "UserId", "dbo.Users");
            DropForeignKey("dbo.InfoPcs", "UserId", "dbo.Users");
            DropForeignKey("dbo.Files", "UserId", "dbo.Users");
            DropIndex("dbo.LogTexts", new[] { "UserId" });
            DropIndex("dbo.InfoPcs", new[] { "UserId" });
            DropIndex("dbo.Files", new[] { "UserId" });
            DropTable("dbo.LogTexts");
            DropTable("dbo.InfoPcs");
            DropTable("dbo.Users");
            DropTable("dbo.Files");
        }
    }
}
