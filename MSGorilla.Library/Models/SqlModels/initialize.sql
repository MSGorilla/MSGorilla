
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/20/2014 19:31:41
-- Generated from EDMX file: D:\Projects\MSGorilla\MSGorilla.Library\Models\SqlModels\MSGorillaDbModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [MSGorillaEntities];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Category_0]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Category] DROP CONSTRAINT [FK_Category_0];
GO
IF OBJECT_ID(N'[dbo].[FK_Category_1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Category] DROP CONSTRAINT [FK_Category_1];
GO
IF OBJECT_ID(N'[dbo].[FK_Chart_DataSet_0]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Chart_DataSet] DROP CONSTRAINT [FK_Chart_DataSet_0];
GO
IF OBJECT_ID(N'[dbo].[FK_Chart_DataSet_1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Chart_DataSet] DROP CONSTRAINT [FK_Chart_DataSet_1];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_Subscription_dbo_UserProfile_Userid]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Subscription] DROP CONSTRAINT [FK_dbo_Subscription_dbo_UserProfile_Userid];
GO
IF OBJECT_ID(N'[dbo].[FK_FAVOURITETOPIC_TOPICID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FavouriteTopic] DROP CONSTRAINT [FK_FAVOURITETOPIC_TOPICID];
GO
IF OBJECT_ID(N'[dbo].[FK_FAVOURITETOPIC_USERID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FavouriteTopic] DROP CONSTRAINT [FK_FAVOURITETOPIC_USERID];
GO
IF OBJECT_ID(N'[dbo].[FK_MatricChart_0]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MetricChart] DROP CONSTRAINT [FK_MatricChart_0];
GO
IF OBJECT_ID(N'[dbo].[FK_MEMBERSHIP_GROUPID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Membership] DROP CONSTRAINT [FK_MEMBERSHIP_GROUPID];
GO
IF OBJECT_ID(N'[dbo].[FK_MEMBERSHIP_MEMBERID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Membership] DROP CONSTRAINT [FK_MEMBERSHIP_MEMBERID];
GO
IF OBJECT_ID(N'[dbo].[FK_MetricDataSet_0]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MetricDataSet] DROP CONSTRAINT [FK_MetricDataSet_0];
GO
IF OBJECT_ID(N'[dbo].[FK_METRICDATASET_GROUPID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MetricDataSet] DROP CONSTRAINT [FK_METRICDATASET_GROUPID];
GO
IF OBJECT_ID(N'[dbo].[FK_NOTIFCOUNT_USERID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NotificationCount] DROP CONSTRAINT [FK_NOTIFCOUNT_USERID];
GO
IF OBJECT_ID(N'[dbo].[FK_TOPIC_GROUPID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Topic] DROP CONSTRAINT [FK_TOPIC_GROUPID];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_DefaultGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile] DROP CONSTRAINT [FK_UserProfile_DefaultGroup];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[AWException]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AWException];
GO
IF OBJECT_ID(N'[dbo].[Category]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Category];
GO
IF OBJECT_ID(N'[dbo].[Chart_DataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Chart_DataSet];
GO
IF OBJECT_ID(N'[dbo].[FavouriteTopic]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FavouriteTopic];
GO
IF OBJECT_ID(N'[dbo].[Group]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Group];
GO
IF OBJECT_ID(N'[dbo].[Membership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Membership];
GO
IF OBJECT_ID(N'[dbo].[MetricChart]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MetricChart];
GO
IF OBJECT_ID(N'[dbo].[MetricDataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MetricDataSet];
GO
IF OBJECT_ID(N'[dbo].[NotificationCount]', 'U') IS NOT NULL
    DROP TABLE [dbo].[NotificationCount];
GO
IF OBJECT_ID(N'[dbo].[Schema]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Schema];
GO
IF OBJECT_ID(N'[dbo].[Subscription]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Subscription];
GO
IF OBJECT_ID(N'[dbo].[Topic]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Topic];
GO
IF OBJECT_ID(N'[dbo].[UserProfile]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfile];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FavouriteTopic'
CREATE TABLE [dbo].[FavouriteTopic] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Userid] nvarchar(128)  NOT NULL,
    [TopicID] int  NOT NULL,
    [UnreadMsgCount] int  NOT NULL
);
GO

-- Creating table 'Group'
CREATE TABLE [dbo].[Group] (
    [GroupID] nvarchar(128)  NOT NULL,
    [DisplayName] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [IsOpen] bit  NOT NULL
);
GO

-- Creating table 'Membership'
CREATE TABLE [dbo].[Membership] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [GroupID] nvarchar(128)  NOT NULL,
    [MemberID] nvarchar(128)  NOT NULL,
    [Role] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'MetricDataSet'
CREATE TABLE [dbo].[MetricDataSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(256)  NULL,
    [GroupID] nvarchar(128)  NOT NULL,
    [Creater] nvarchar(128)  NOT NULL,
    [RecordCount] int  NOT NULL,
    [Category] nvarchar(128)  NULL,
    [Counter] nvarchar(128)  NULL,
    [Instance] nvarchar(128)  NULL
);
GO

-- Creating table 'NotificationCount'
CREATE TABLE [dbo].[NotificationCount] (
    [Userid] nvarchar(128)  NOT NULL,
    [UnreadHomelineMsgCount] int  NOT NULL,
    [UnreadOwnerlineMsgCount] int  NOT NULL,
    [UnreadAtlineMsgCount] int  NOT NULL,
    [UnreadReplyCount] int  NOT NULL,
    [UnreadImportantMsgCount] int  NOT NULL
);
GO

-- Creating table 'Schema'
CREATE TABLE [dbo].[Schema] (
    [SchemaID] nvarchar(128)  NOT NULL,
    [SchemaContent] nvarchar(max)  NULL
);
GO

-- Creating table 'Subscription'
CREATE TABLE [dbo].[Subscription] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Userid] nvarchar(128)  NULL,
    [FollowingUserid] nvarchar(max)  NULL,
    [FollowingUserDisplayName] nvarchar(max)  NULL
);
GO

-- Creating table 'Topic'
CREATE TABLE [dbo].[Topic] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(128)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [MsgCount] int  NOT NULL,
    [GroupID] nvarchar(128)  NULL
);
GO

-- Creating table 'UserProfile'
CREATE TABLE [dbo].[UserProfile] (
    [Userid] nvarchar(128)  NOT NULL,
    [DisplayName] nvarchar(max)  NOT NULL,
    [PortraitUrl] nvarchar(max)  NULL,
    [Description] nvarchar(max)  NULL,
    [FollowingsCount] int  NOT NULL,
    [FollowersCount] int  NOT NULL,
    [Password] nvarchar(128)  NULL,
    [MessageCount] int  NOT NULL,
    [IsRobot] bit  NOT NULL,
    [DefaultGroup] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'Chart_DataSet'
CREATE TABLE [dbo].[Chart_DataSet] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Legend] nvarchar(128)  NOT NULL,
    [Type] nvarchar(128)  NOT NULL,
    [MetricDataSetID] int  NOT NULL,
    [MetricChartName] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'MetricChart'
CREATE TABLE [dbo].[MetricChart] (
    [Name] nvarchar(128)  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [SubTitle] nvarchar(max)  NULL,
    [GroupID] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'Category'
CREATE TABLE [dbo].[Category] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(128)  NOT NULL,
    [GroupID] nvarchar(128)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [Creater] nvarchar(128)  NOT NULL,
    [CreateTimestamp] datetime  NOT NULL
);
GO

-- Creating table 'AWException'
CREATE TABLE [dbo].[AWException] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Timestamp] datetimeoffset  NOT NULL,
    [Table] nvarchar(128)  NOT NULL,
    [Function] nvarchar(128)  NOT NULL,
    [ExceptionType] nvarchar(128)  NOT NULL,
    [ExceptionMsg] nvarchar(max)  NULL,
    [StackTrace] nvarchar(max)  NULL,
    [HttpStatusCode] int  NULL,
    [ServiceRequestID] nvarchar(64)  NULL,
    [PartitionKey] nvarchar(128)  NULL,
    [RowKey] nvarchar(128)  NULL,
    [LastResultXml] nvarchar(max)  NULL,
    [RequestStartTime] datetimeoffset  NULL,
    [RequestResults] nvarchar(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'FavouriteTopic'
ALTER TABLE [dbo].[FavouriteTopic]
ADD CONSTRAINT [PK_FavouriteTopic]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [GroupID] in table 'Group'
ALTER TABLE [dbo].[Group]
ADD CONSTRAINT [PK_Group]
    PRIMARY KEY CLUSTERED ([GroupID] ASC);
GO

-- Creating primary key on [Id] in table 'Membership'
ALTER TABLE [dbo].[Membership]
ADD CONSTRAINT [PK_Membership]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MetricDataSet'
ALTER TABLE [dbo].[MetricDataSet]
ADD CONSTRAINT [PK_MetricDataSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Userid] in table 'NotificationCount'
ALTER TABLE [dbo].[NotificationCount]
ADD CONSTRAINT [PK_NotificationCount]
    PRIMARY KEY CLUSTERED ([Userid] ASC);
GO

-- Creating primary key on [SchemaID] in table 'Schema'
ALTER TABLE [dbo].[Schema]
ADD CONSTRAINT [PK_Schema]
    PRIMARY KEY CLUSTERED ([SchemaID] ASC);
GO

-- Creating primary key on [Id] in table 'Subscription'
ALTER TABLE [dbo].[Subscription]
ADD CONSTRAINT [PK_Subscription]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Topic'
ALTER TABLE [dbo].[Topic]
ADD CONSTRAINT [PK_Topic]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Userid] in table 'UserProfile'
ALTER TABLE [dbo].[UserProfile]
ADD CONSTRAINT [PK_UserProfile]
    PRIMARY KEY CLUSTERED ([Userid] ASC);
GO

-- Creating primary key on [ID] in table 'Chart_DataSet'
ALTER TABLE [dbo].[Chart_DataSet]
ADD CONSTRAINT [PK_Chart_DataSet]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Name] in table 'MetricChart'
ALTER TABLE [dbo].[MetricChart]
ADD CONSTRAINT [PK_MetricChart]
    PRIMARY KEY CLUSTERED ([Name] ASC);
GO

-- Creating primary key on [ID] in table 'Category'
ALTER TABLE [dbo].[Category]
ADD CONSTRAINT [PK_Category]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'AWException'
ALTER TABLE [dbo].[AWException]
ADD CONSTRAINT [PK_AWException]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [TopicID] in table 'FavouriteTopic'
ALTER TABLE [dbo].[FavouriteTopic]
ADD CONSTRAINT [FK_FAVOURITETOPIC_TOPICID]
    FOREIGN KEY ([TopicID])
    REFERENCES [dbo].[Topic]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FAVOURITETOPIC_TOPICID'
CREATE INDEX [IX_FK_FAVOURITETOPIC_TOPICID]
ON [dbo].[FavouriteTopic]
    ([TopicID]);
GO

-- Creating foreign key on [Userid] in table 'FavouriteTopic'
ALTER TABLE [dbo].[FavouriteTopic]
ADD CONSTRAINT [FK_FAVOURITETOPIC_USERID]
    FOREIGN KEY ([Userid])
    REFERENCES [dbo].[UserProfile]
        ([Userid])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FAVOURITETOPIC_USERID'
CREATE INDEX [IX_FK_FAVOURITETOPIC_USERID]
ON [dbo].[FavouriteTopic]
    ([Userid]);
GO

-- Creating foreign key on [GroupID] in table 'Membership'
ALTER TABLE [dbo].[Membership]
ADD CONSTRAINT [FK_MEMBERSHIP_GROUPID]
    FOREIGN KEY ([GroupID])
    REFERENCES [dbo].[Group]
        ([GroupID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MEMBERSHIP_GROUPID'
CREATE INDEX [IX_FK_MEMBERSHIP_GROUPID]
ON [dbo].[Membership]
    ([GroupID]);
GO

-- Creating foreign key on [GroupID] in table 'MetricDataSet'
ALTER TABLE [dbo].[MetricDataSet]
ADD CONSTRAINT [FK_METRICDATASET_GROUPID]
    FOREIGN KEY ([GroupID])
    REFERENCES [dbo].[Group]
        ([GroupID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_METRICDATASET_GROUPID'
CREATE INDEX [IX_FK_METRICDATASET_GROUPID]
ON [dbo].[MetricDataSet]
    ([GroupID]);
GO

-- Creating foreign key on [GroupID] in table 'Topic'
ALTER TABLE [dbo].[Topic]
ADD CONSTRAINT [FK_TOPIC_GROUPID]
    FOREIGN KEY ([GroupID])
    REFERENCES [dbo].[Group]
        ([GroupID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TOPIC_GROUPID'
CREATE INDEX [IX_FK_TOPIC_GROUPID]
ON [dbo].[Topic]
    ([GroupID]);
GO

-- Creating foreign key on [MemberID] in table 'Membership'
ALTER TABLE [dbo].[Membership]
ADD CONSTRAINT [FK_MEMBERSHIP_MEMBERID]
    FOREIGN KEY ([MemberID])
    REFERENCES [dbo].[UserProfile]
        ([Userid])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MEMBERSHIP_MEMBERID'
CREATE INDEX [IX_FK_MEMBERSHIP_MEMBERID]
ON [dbo].[Membership]
    ([MemberID]);
GO

-- Creating foreign key on [Userid] in table 'NotificationCount'
ALTER TABLE [dbo].[NotificationCount]
ADD CONSTRAINT [FK_NOTIFCOUNT_USERID]
    FOREIGN KEY ([Userid])
    REFERENCES [dbo].[UserProfile]
        ([Userid])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Userid] in table 'Subscription'
ALTER TABLE [dbo].[Subscription]
ADD CONSTRAINT [FK_dbo_Subscription_dbo_UserProfile_Userid]
    FOREIGN KEY ([Userid])
    REFERENCES [dbo].[UserProfile]
        ([Userid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_Subscription_dbo_UserProfile_Userid'
CREATE INDEX [IX_FK_dbo_Subscription_dbo_UserProfile_Userid]
ON [dbo].[Subscription]
    ([Userid]);
GO

-- Creating foreign key on [DefaultGroup] in table 'UserProfile'
ALTER TABLE [dbo].[UserProfile]
ADD CONSTRAINT [FK_UserProfile_DefaultGroup]
    FOREIGN KEY ([DefaultGroup])
    REFERENCES [dbo].[Group]
        ([GroupID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfile_DefaultGroup'
CREATE INDEX [IX_FK_UserProfile_DefaultGroup]
ON [dbo].[UserProfile]
    ([DefaultGroup]);
GO

-- Creating foreign key on [Creater] in table 'MetricDataSet'
ALTER TABLE [dbo].[MetricDataSet]
ADD CONSTRAINT [FK_MetricDataSet_0]
    FOREIGN KEY ([Creater])
    REFERENCES [dbo].[UserProfile]
        ([Userid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MetricDataSet_0'
CREATE INDEX [IX_FK_MetricDataSet_0]
ON [dbo].[MetricDataSet]
    ([Creater]);
GO

-- Creating foreign key on [MetricDataSetID] in table 'Chart_DataSet'
ALTER TABLE [dbo].[Chart_DataSet]
ADD CONSTRAINT [FK_Chart_DataSet_0]
    FOREIGN KEY ([MetricDataSetID])
    REFERENCES [dbo].[MetricDataSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Chart_DataSet_0'
CREATE INDEX [IX_FK_Chart_DataSet_0]
ON [dbo].[Chart_DataSet]
    ([MetricDataSetID]);
GO

-- Creating foreign key on [MetricChartName] in table 'Chart_DataSet'
ALTER TABLE [dbo].[Chart_DataSet]
ADD CONSTRAINT [FK_Chart_DataSet_1]
    FOREIGN KEY ([MetricChartName])
    REFERENCES [dbo].[MetricChart]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Chart_DataSet_1'
CREATE INDEX [IX_FK_Chart_DataSet_1]
ON [dbo].[Chart_DataSet]
    ([MetricChartName]);
GO

-- Creating foreign key on [GroupID] in table 'MetricChart'
ALTER TABLE [dbo].[MetricChart]
ADD CONSTRAINT [FK_MatricChart_0]
    FOREIGN KEY ([GroupID])
    REFERENCES [dbo].[Group]
        ([GroupID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MatricChart_0'
CREATE INDEX [IX_FK_MatricChart_0]
ON [dbo].[MetricChart]
    ([GroupID]);
GO

-- Creating foreign key on [Creater] in table 'Category'
ALTER TABLE [dbo].[Category]
ADD CONSTRAINT [FK_Category_0]
    FOREIGN KEY ([Creater])
    REFERENCES [dbo].[UserProfile]
        ([Userid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Category_0'
CREATE INDEX [IX_FK_Category_0]
ON [dbo].[Category]
    ([Creater]);
GO

-- Creating foreign key on [GroupID] in table 'Category'
ALTER TABLE [dbo].[Category]
ADD CONSTRAINT [FK_Category_1]
    FOREIGN KEY ([GroupID])
    REFERENCES [dbo].[Group]
        ([GroupID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Category_1'
CREATE INDEX [IX_FK_Category_1]
ON [dbo].[Category]
    ([GroupID]);
GO

-- Initial Data --
insert into [dbo].[Group] values('default', 'AllUser', 'default group for all user', 1);
insert into [dbo].[UserProfile] values(
    'admin', 
    'Admin', 
    '/Content/Images/default_avatar.jpg', 
    'default admin', 
    0,
    0,
    'f6fdffe48c908deb0f4c3bd36c032e72',
    0,
    0,
    'default');
insert into [dbo].[Membership](GroupID, MemberID, Role) values(
    'default', 'admin', 'admin');
-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------