using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;
using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;


namespace MSGorilla.Library
{
    public class StatisticsManager
    {
        private CloudTable _homeline;
        private CloudTable _userline;
        private CloudTable _eventline;
        private CloudTable _ownerline;
        private CloudTable _atline;
        private CloudTable _publicSquareLine;
        private CloudTable _topicline;
        private CloudTable _reply;

        private AccountManager _accManager;
        private AttachmentManager _attManager;
        private SchemaManager _schemaManager;
        private NotifManager _notifManager;
        private TopicManager _topicManager;
        private RichMsgManager _richMsgManager;


        public StatisticsManager()
        {
            _homeline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Homeline);
            _userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);
            _eventline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.EventLine);
            _publicSquareLine = AzureFactory.GetTable(AzureFactory.MSGorillaTable.PublicSquareLine);
            _topicline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.TopicLine);
            _ownerline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.OwnerLine);
            _atline = AzureFactory.GetTable(Azure.AzureFactory.MSGorillaTable.AtLine);
            _reply = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Reply);

            _accManager = new AccountManager();
            _attManager = new AttachmentManager();
            _schemaManager = new SchemaManager();
            _notifManager = new NotifManager();
            _topicManager = new TopicManager();
            _richMsgManager = new RichMsgManager();
        }

        public void CountTopics(DateTime countdate)
        {

        }
    }
}
