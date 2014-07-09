namespace MSGorilla.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_important_msg_count : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationCount", "UnreadImportantMsgCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationCount", "UnreadImportantMsgCount");
        }
    }
}
