﻿/* common function */
function encodeHtml(code) {
    code = code.replace(/&/mg, '&#38;');
    code = code.replace(/</mg, '&#60;');
    code = code.replace(/>/mg, '&#62;');
    code = code.replace(/\"/mg, '&#34;');
    code = code.replace(/\t/g, '  ');
    code = code.replace(/\r?\n/g, '<br>');
    code = code.replace(/<br><br>/g, '<br>');
    code = code.replace(/ /g, '&nbsp;');
    return code;
}

function isNullOrEmpty(strVal) {
    if (strVal == '' || strVal == null || strVal == undefined) {
        return true;
    } else {
        return false;
    }
}

function DateFormat(datestring) {
    var date = new Date(datestring);
    var format = date.toUTCString();
    return format;
}

function Time2Now(datestring) {
    var date = new Date(datestring);
    var now = new Date();
    var diff = (now - date)/1000;

    var sec = 1;
    var min = 60 * sec;
    var hour = 60 * min;
    var day = 24 * hour;
    if (diff / day > 1) {
        return date.toDateString();
    }
    else if (diff / hour > 1) {
        return Math.ceil(diff / hour) + "h";
    }
    else if (diff / min > 1) {
        return Math.ceil(diff / min) + "m";
    }
    else if (diff / sec > 1) {
        return Math.ceil(diff / sec) + "s";
    }
}

/* show message function */
function ShowError(msg) {
    var msghtml = "<div class='alert alert-dismissable alert-warning'><button class='close' type='button' data-dismiss='alert'>×</button><p>" + msg + "</p></div>";
    $("#notifications").append(msghtml);
}


/* user infor function */
function LoadMyInfo() {
    var apiurl = "/api/account/me";

    $.ajax({
        type: "get",
        url: apiurl,
        success: function (data) {
            var userid = data.Userid;
            var username = data.DisplayName;
            var picurl = data.PortraitUrl;
            var desp = data.Description;
            var postscount = data.MessageCount;
            var followingcount = data.FollowingsCount;
            var followerscount = data.FollowersCount;

            $("#my_id").html("@" + userid);
            $("#my_name").html(username);
            $("my_name").attr("href", "/profile");
            if (!isNullOrEmpty(picurl)) {
                $("#my_pic").attr("src", picurl);
            }
            $("#my_posts").html(postscount);
            $("#my_following").html(followingcount);
            $("#my_followers").html(followerscount);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function LoadUserInfo(user) {
    var apiurl = "";
    if (isNullOrEmpty(user)) {
        apiurl = "/api/account/me";
    }
    else {
        apiurl = "/api/account/user?userid=" + user;
        LoadUserFollowBtn(user);
    }

    $.ajax({
        type: "get",
        url: apiurl,
        success: function (data) {
            var userid = data.Userid;
            var username = data.DisplayName;
            var picurl = data.PortraitUrl;
            var desp = data.Description;
            var postscount = data.MessageCount;
            var followingcount = data.FollowingsCount;
            var followerscount = data.FollowersCount;

            $("#user_id").html("@" + userid);
            $("#user_name").html(username);
            $("#user_name").attr("href", "/profile/index?user=" + userid);
            if (!isNullOrEmpty(picurl)) {
                $("#user_pic").attr("src", picurl);
            }
            $("#user_desp").html(desp);
            $("#user_posts").html(postscount);
            $("#user_following").html(followingcount);
            $("#user_followers").html(followerscount);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function LoadUserFollowBtn(user) {
    var btn = $("#btn_user_follow");
    if (btn == undefined) {
        return;
    }

    $.get("/api/account/isfollowing", "followingUserID=" + user, function (data) {
        if (data == false) {
            SetFollowBtn(user);
        } else {
            SetUnfollowBtn(user);
        }
    });
}

function SetUnfollowBtn(user) {
    var btn = $("#btn_user_follow");
    if (btn == undefined) {
        return;
    }

    btn.text("Following");
    btn.attr("class", "btn btn-success");
    btn.attr("onclick", "Unfollow('" + user + "');");
    btn.attr("onmouseover", "UnfollowBtnMouseOver();")
    btn.attr("onmouseout", "UnfollowBtnMouseOut();")
}

function UnfollowBtnMouseOver() {
    var btn = $("#btn_user_follow");
    if (btn == undefined) {
        return;
    }

    btn.attr("class", "btn btn-danger");
    btn.text("Unfollow");
}

function UnfollowBtnMouseOut() {
    var btn = $("#btn_user_follow");
    if (btn == undefined) {
        return;
    }

    btn.attr("class", "btn btn-success");
    btn.text("Following");
}

function SetFollowBtn(user) {
    var btn = $("#btn_user_follow");
    if (btn == undefined) {
        return;
    }

    btn.text("Follow");
    btn.attr("class", "btn btn-primary");
    btn.attr("onclick", "Follow('" + user + "');");
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
}

function Follow(user) {
    $.ajax({
        type: "get",
        url: "/api/account/follow",
        data: "userid=" + user,
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadUserInfo(user);
                LoadMyInfo();
            }
            else {
                ShowError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function Unfollow(user) {
    $.ajax({
        type: "get",
        url: "/api/account/unfollow",
        data: "userid=" + user,
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadUserInfo(user);
                LoadMyInfo();
            }
            else {
                ShowError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

/* post function */
function PostMessage() {
    var message = $("#postmessage").val().trim();

    if (message.length === 0) {
        return;
    }

    $.ajax({
        type: "post",
        url: "/api/message/postmessage?" + "message=" + message,
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                $("#postmessage").val("");
                alert(msg);
                LoadFeeds("homeline");
            }
            else {
                ShowError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function PostReply(user, mid) {
    var message = $("#replymessage_" + mid).val().trim();

    if (message.length === 0) {
        return;
    }

    $.ajax({
        type: "post",
        url: "/api/reply/postreply?" + "to=" + user + "&message=" + message + "&messageUser=" + user + "&messageID=" + mid,
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                $("#replymessage_" + mid).val("");
                LoadReplies(mid);
            }
            else {
                ShowError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}


// feed function
function LoadFeeds(category) {
    var apiurl = "";
    if (isNullOrEmpty(category))
        apiurl = "/api/message/userline";
    else if (category == "homeline")
        apiurl = "/api/message/homeline";
    else
        apiurl = "/api/message/userline?userid=" + category;

    $.ajax({
        type: "get",
        url: apiurl,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                ShowError("No feed found.");
            }
            else {
                // create feed list
                $("#feedlist").empty();
                $.each(data, function (index, item) {
                    $("#feedlist").append(CreateFeed(item));
                    $.getJSON("/api/account/User", "userid=" + item.User, function (data) {
                        var mid = item.ID;
                        var username = data.DisplayName;
                        var picurl = data.PortraitUrl;
                        if (isNullOrEmpty(picurl)) {
                            picurl = "/Content/Images/default_avatar.jpg";
                        }
                        $("#user_pic_" + mid).attr("src", picurl);
                        $("#username_" + mid).text(username);
                    });
                })

            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function CreateFeed(postData) {
    var output = "";
    var user = postData.User;
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var posttime = postData.PostTime;

    output += "<ul class='list-group'>";
    output += CreateMessage(user, mid, sid, eid, msg, posttime);
    output += "</ul>";

    return output;
}

function CreateMessage(user, mid, sid, eid, msg, posttime) {
    var output = "";
    var showevents = false;

    if (!isNullOrEmpty(eid) && eid != "none") {
        showevents = true;
    }

    if (showevents) {
        output += "  <div id='event_newer_" + mid + "'></div>";
    }

    output += "<div>";
    output += "  <li class='list-group-item'>";
    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='user_pic_" + mid + "' src='/Content/Images/default_avatar.jpg' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'><a id='username_" + mid + "' class='list-group-item-heading bold' href='/profile/index?user=" + user + "'>" + user + "</a>";
    output += "        &nbsp;<span class='badge'>-&nbsp;" + Time2Now(posttime) + "</span></div>";
    output += "        <div class='newpost-input'><p>" + encodeHtml(msg) + "</p></div>";
    output += "        <div class='newpost-footer'>";
    if (showevents) {
        output += "      <button id='btn_expandevent' class='btn btn-link' type='button' onclick='ShowEvents(\"" + mid + "\", \"" + eid + "\");'>Events</button>";
    }
    output += "          <button id='btn_reply' class='btn btn-link' type='button' onclick='ShowReplies(\"" + user + "\", \"" + mid + "\");'>Comments</button>";
    output += "        </div>";
    output += "      </div>";
    output += "    </div>";
    output += "    <div id='reply_" + mid + "'></div>";
    output += "    <input type='hidden' id='isshowreplies_" + mid + "' value='false'/>";
    if (showevents) {
        output += "    <input type='hidden' id='isshowevents_" + mid + "' value='false'/>";
    }
    output += "  </li>";
    output += "</div>";

    if (showevents) {
        output += "  <div id='event_older_" + mid + "'></div>";
    }

    return output;
}


// reply function
function ShowReplies(user, mid) {
    var show = $("#isshowreplies_" + mid);
    var replydiv = $("#reply_" + mid);

    if (show.val() == "false") {
        // show replies
        show.val("true");

        var html = "";
        html = "<div class='replies-container well'>";
        // add reply box
        html += "  <div class='input-group'>";
        //html += "     <span class='input-group-addon'>Comment</span>";
        html += "     <input class='form-control' type='text' id='replymessage_" + mid + "'>";
        html += "     <span class='input-group-btn'>";
        html += "       <button class='btn btn-primary' type='button' id='btn_reply' onclick='PostReply(\"" + user + "\", \"" + mid + "\");'>Reply</button>";
        html += "     </span>";
        html += "  </div>";
        // add replies
        html += "  <div class='replies-feeds'>";
        html += "    <ul id='replylist_" + mid + "' class='list-group'></ul>";
        html += "  </div>";
        html += "</div>";

        replydiv.html(html);

        // show reply
        LoadReplies(mid);
    }
    else {
        // clear replies
        show.val("false");
        replydiv.html("");
    }
}

function LoadReplies(mid) {
    $.ajax({
        type: "get",
        url: "/api/message/getmessagereply",
        data: "msgID=" + mid,
        success: function (data) {
            // create reply list
            $("#replylist_" + mid).empty();
            var i = 0;
            $.each(data, function (index, item) {
                $("#replylist_" + mid).append(CreateReply(item, ++i));
                var rid = mid + "_" + i;
                $.getJSON("/api/account/User", "userid=" + item.FromUser, function (data) {
                    var username = data.DisplayName;
                    var picurl = data.PortraitUrl;
                    if (isNullOrEmpty(picurl)) {
                        picurl = "/Content/Images/default_avatar.jpg111";
                    }
                    $("#reply_user_pic_" + rid).attr("src", picurl);
                    $("#reply_username_" + rid).text(username);
                });
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function CreateReply(replyData, no) {
    var output = "";
    var user = replyData.FromUser;
    var msg = replyData.Message;
    var posttime = replyData.PostTime;
    var rid = replyData.MessageID + "_" + no;

    output = "<li class='list-group-item'>";
    output += "  <div>"
    output += "    <div class='reply-pic'>";
    output += "      <img class='img-rounded' id='reply_user_pic" + rid + "' src='/Content/Images/default_avatar.jpg' width='50' height='50' />";
    output += "    </div>";
    output += "    <div class='reply-content'>";
    output += "      <div class='reply-input'>";
    output += "        <a id='reply_username_" + rid + "' href='/profile/index?user=" + user + "'>" + user + "</a>";
    output += "        &nbsp;<span class='badge'>-&nbsp;" + Time2Now(posttime) + "</span>&nbsp;";
    output += "        " + encodeHtml(msg);
    output += "      </div>";
    output += "  </div>";
    output += "</li>";
    return output;
}

// event function
function ShowEvents(mid, eid) {
    var show = $("#isshowevents_" + mid);
    var newerdiv = $("#event_newer_" + mid);
    var olderdiv = $("#event_older_" + mid);

    if (show == undefined) {
        return;
    }

    if (show.val() == "false") {
        // show events
        show.val("true");
        // show events
        LoadEvents(mid,eid);
    }
    else {
        // clear events
        show.val("false");
        newerdiv.html("");
        olderdiv.html("");
    }
}

function LoadEvents(mid, eid) {
    var newerdiv = $("#event_newer_" + mid);
    var olderdiv = $("#event_older_" + mid);

    newerdiv.empty();
    olderdiv.empty();

    $.ajax({
        type: "get",
        url: "/api/message/eventline",
        data: "eventID=" + eid,
        success: function (data) {
            var eventdiv = newerdiv;
            // create events list
            $.each(data, function (index, item) {
                if (item.ID == mid) {
                    eventdiv = olderdiv;
                    return true;
                }
                eventdiv.append(CreateEvent(item));
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function CreateEvent(postData) {
    var output = "";
    var user = postData.User;
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var posttime = postData.PostTime;

    output += "  <li class='list-group-item quote'>";
    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='user_pic_" + mid + "_" + eid + "' src='/Content/Images/default_avatar.jpg' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'><a id='username_" + mid + "_" + eid + "' class='list-group-item-heading bold' href='/profile/index?user=" + user + "'>" + user + "</a>";
    output += "        &nbsp;<span class='badge'>-&nbsp;" + Time2Now(posttime) + "</span></div>";
    output += "        <div class='newpost-input'><p>" + encodeHtml(msg) + "</p></div>";
    //output += "        <div class='newpost-footer'>";
    //output += "          <button id='btn_reply' class='btn btn-link' type='button' onclick='ShowReplies(\"" + user + "\", \"" + mid + "\");'>Comments</button>";
    //output += "        </div>";
    output += "      </div>";
    output += "    </div>";
    //output += "    <div id='reply_" + mid + "'></div>";
    //output += "    <input type='hidden' id='isshowreplies_" + mid + "' value='false'/>";
    output += "  </li>";

    return output;
}
