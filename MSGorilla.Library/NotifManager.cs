using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.DAL;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library
{
    public class NotifManager
    {
        public NotificationCount FindUserNotif(string userid, MSGorillaContext _gorillaCtx)
        {
            NotificationCount notifCount = _gorillaCtx.NotifCounts.Find(userid);
            if (notifCount == null)
            {
                notifCount = new NotificationCount();
                notifCount.Userid = userid;
                notifCount.UnreadAtlineMsgCount = notifCount.UnreadHomelineMsgCount = 0;
                notifCount.UnreadOwnerlineMsgCount = notifCount.UnreadReplyCount = 0;

                notifCount = _gorillaCtx.NotifCounts.Add(notifCount);
                _gorillaCtx.SaveChanges();
            }
            return notifCount;
        }

        public NotificationCount FindUserNotif(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notifCount = _gorillaCtx.NotifCounts.Find(userid);
                if (notifCount == null)
                {
                    notifCount = new NotificationCount();
                    notifCount.Userid = userid;
                    notifCount.UnreadAtlineMsgCount = notifCount.UnreadHomelineMsgCount = 0;
                    notifCount.UnreadOwnerlineMsgCount = notifCount.UnreadReplyCount = 0;

                    notifCount = _gorillaCtx.NotifCounts.Add(notifCount);
                    _gorillaCtx.SaveChanges();
                }
                return notifCount;
            }            
        }

        public void incrementAtlineNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadAtlineMsgCount++;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void incrementHomelineNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadHomelineMsgCount++;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void incrementOwnerlineNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadOwnerlineMsgCount++;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void incrementReplyNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadReplyCount++;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void incrementImportantMsgCount(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadImportantMsgCount++;
                _gorillaCtx.SaveChanges();
            }
        }

        public void clearAtlineNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadAtlineMsgCount = 0;
                _gorillaCtx.SaveChanges();
            }            
        }
        public void clearHomelineNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadHomelineMsgCount = 0;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void clearOwnerlineNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadOwnerlineMsgCount = 0;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void clearReplyNotifCount(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadReplyCount = 0;
                _gorillaCtx.SaveChanges();
            }            
        }

        public void clearImportantMsgCount(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadImportantMsgCount = 0;
                _gorillaCtx.SaveChanges();
            }
        }

        public void clearAll(string userid)
        {
            using(var _gorillaCtx = new MSGorillaContext())
            {
                NotificationCount notif = FindUserNotif(userid, _gorillaCtx);
                notif.UnreadOwnerlineMsgCount = 0;
                notif.UnreadReplyCount = 0;
                notif.UnreadHomelineMsgCount = 0;
                notif.UnreadAtlineMsgCount = 0;
                _gorillaCtx.SaveChanges();
            }            
        }
    }        
}
