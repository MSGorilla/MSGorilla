namespace MSGorilla.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMessageCountPostedByUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "MessageCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProfile", "MessageCount");
        }
    }
}
