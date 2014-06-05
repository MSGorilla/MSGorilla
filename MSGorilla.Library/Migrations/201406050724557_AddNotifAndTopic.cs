namespace MSGorilla.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotifAndTopic : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationCount",
                c => new
                    {
                        Userid = c.String(nullable: false, maxLength: 128),
                        UnreadHomelineMsgCount = c.Int(nullable: false),
                        UnreadOwnerlineMsgCount = c.Int(nullable: false),
                        UnreadAtlineMsgCount = c.Int(nullable: false),
                        UnreadReplyCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Userid);
            
            CreateTable(
                "dbo.Topic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        MsgCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Topic");
            DropTable("dbo.NotificationCount");
        }
    }
}
