namespace MSGorilla.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Schema",
                c => new
                    {
                        SchemaID = c.String(nullable: false, maxLength: 128),
                        SchemaContent = c.String(),
                    })
                .PrimaryKey(t => t.SchemaID);
            
            CreateTable(
                "dbo.Subscription",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Userid = c.String(maxLength: 128),
                        FollowingUserid = c.String(),
                        FollowingUserDisplayName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfile", t => t.Userid)
                .Index(t => t.Userid);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        Userid = c.String(nullable: false, maxLength: 128),
                        DisplayName = c.String(nullable: false),
                        PortraitUrl = c.String(),
                        Description = c.String(),
                        FollowingsCount = c.Int(nullable: false),
                        FollowersCount = c.Int(nullable: false),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Userid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Subscription", "Userid", "dbo.UserProfile");
            DropIndex("dbo.Subscription", new[] { "Userid" });
            DropTable("dbo.UserProfile");
            DropTable("dbo.Subscription");
            DropTable("dbo.Schema");
        }
    }
}
