namespace MSGorilla.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_schema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Schema",
                c => new
                    {
                        SchemaID = c.Int(nullable: false, identity: true),
                        SchemaText = c.String(),
                    })
                .PrimaryKey(t => t.SchemaID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Schema");
        }
    }
}
