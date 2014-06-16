namespace MSGorilla.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFavourateTopic : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FavouriteTopic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Userid = c.String(),
                        TopicID = c.Int(nullable: false),
                        UnreadMsgCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FavouriteTopic");
        }
    }
}
