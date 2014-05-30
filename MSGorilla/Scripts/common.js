/* common function */
function ScrollTo(itemname) {
    var scroll_offset = $("#" + itemname).offset();
    $("body,html").animate({
        scrollTop: scroll_offset.top
    }, 0);
}

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
    if (strVal == null || strVal == undefined || strVal == '') {
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
    var diff = (now - date) / 1000;

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
    else {
        return "1s";
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

            LoadUserFollowBtn(user);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function LoadUserFollowBtn(user) {
    var btn = $("#btn_user_follow_" + user);
    if (btn.length == 0) {
        return;
    }

    $.get("/api/account/isfollowing", "followingUserID=" + user, function (data) {
        if (data == 0) {
            SetFollowBtn(user, true);
        } else if(data == 1) {
            SetUnfollowBtn(user, true);
        }
        else {  // -1: myself
            SetEditProfileBtn(user);
        }
    });
}

function SetEditProfileBtn(user) {
    var btn = $("#btn_user_follow_" + user);
    if (btn.length == 0) {
        return;
    }

    btn.text("Edit profile");
    btn.attr("class", "btn btn-info");
    btn.attr("href", "/account/manage");
}

function SetUnfollowBtn(user, enabled) {
    var btn = $("#btn_user_follow_" + user);
    if (btn.length == 0) {
        return;
    }

    btn.text("Following");
    btn.attr("class", "btn btn-success");
    if (enabled) {
        btn.attr("onclick", "Unfollow('" + user + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "UnfollowBtnMouseOver('" + user + "');")
    btn.attr("onmouseout", "UnfollowBtnMouseOut('" + user + "');")
}

function UnfollowBtnMouseOver(user) {
    var btn = $("#btn_user_follow_" + user);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger");
    btn.text("Unfollow");
}

function UnfollowBtnMouseOut(user) {
    var btn = $("#btn_user_follow_" + user);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success");
    btn.text("Following");
}

function SetFollowBtn(user, enabled) {
    var btn = $("#btn_user_follow_" + user);
    if (btn.length == 0) {
        return;
    }

    btn.text("Follow");
    btn.attr("class", "btn btn-primary");
    if (enabled) {
        btn.attr("onclick", "Follow('" + user + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
}

function Follow(user) {
    SetUnfollowBtn(user, false);
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
    SetFollowBtn(user, false);
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

    $("#btn_post").button('loading');
    $.ajax({
        type: "post",
        url: "/api/message/postmessage?" + "message=" + message,
        success: function (data) {
            $("#postmessage").val("");
            // insert the new posted message
            $("#feedlist").prepend(CreateFeed(data));
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    }).always(function () {
        $("#btn_post").button('reset');
    });
}

function PostReply(user, mid) {
    var message = $("#replymessage_" + mid).val().trim();

    if (message.length === 0) {
        return;
    }

    $("#btn_reply_" + mid).button('loading');
    $.ajax({
        type: "post",
        url: "/api/reply/postreply?" + "to=" + user + "&message=" + message + "&messageUser=" + user + "&messageID=" + mid,
        success: function (data) {
            $("#replymessage_" + mid).val("");
            $("#replylist_" + mid).prepend(CreateReply(data));
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    }).always(function () {
        $("#btn_reply_" + mid).button('reset');
    });
}


// feed function
function LoadFeeds(category) {
    var apiurl = "";
    var apidata = ""
    if (isNullOrEmpty(category))
        apiurl = "/api/message/userline";
    else if (category == "homeline")
        apiurl = "/api/message/homeline";
    else if(category == "ownerline")
        apiurl = "api/message/ownerline";
    else if (category == "publicsquareline")
        apiurl = "api/message/publicsquareline";
    else {
        apiurl = "/api/message/userline";
        apidata = "userid=" + category;
    }

    var token = $("#hd_token").val();
    if (!isNullOrEmpty(token)) {
        if (token == "nomore") {
            // already no more feeds, don't load any more
            return;
        }

        if (isNullOrEmpty(apidata)) {
            apidata = "token=" + token;
        }
        else {
            apidata += "&token=" + token;
        }
    }

    $.ajax({
        type: "get",
        url: apiurl,
        dataType: "json",
        data: apidata,
        success: function (data) {
            nexttoken = data.continuationToken
            data = data.message
            //ShowError(data);
            if (data.length == 0) {
                ShowError("No feed found.");
            }
            else {
                // create feed list
                if (isNullOrEmpty(token)) {
                    // clear feeds at the first time 
                    $("#feedlist").empty();
                }
                $.each(data, function (index, item) {
                    $("#feedlist").append(CreateFeed(item));
                })

            }

            if (isNullOrEmpty(nexttoken)) {
                $("#lbl_seemore").html("No more feeds");
                $("#hd_token").val("nomore");
            }
            else {
                $("#lbl_seemore").html("Loading more...");
                $("#hd_token").val(nexttoken);
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
    var username = postData.DisplayName;
    var picurl = postData.PortraitUrl;
    var userdesp = postData.Description;
    var showevents = false;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    if (!isNullOrEmpty(eid) && eid != "none") {
        showevents = true;
    }

    output += "<ul class='list-group'>";

    if (showevents) {
        output += "  <div id='event_newer_" + mid + "'></div>";
    }

    output += "<div>";
    output += "  <li class='list-group-item'>";
    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='user_pic_" + mid + "' src='" + picurl + "' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'><a id='username_" + mid + "' class='list-group-item-heading bold' href='/profile/index?user=" + user + "'>" + username + "</a>";
    output += "        &nbsp;<span class='badge'>-&nbsp;" + Time2Now(posttime) + "</span></div>";
    output += "        <div class='newpost-input'><p>" + encodeHtml(msg) + "</p></div>";
    output += "        <div class='newpost-footer'>";
    if (showevents) {
        output += "      <button id='btn_expandevent' class='btn btn-link' type='button' onclick='ShowEvents(\"" + mid + "\", \"" + eid + "\");'>Events</button>";
    }
    output += "          <button id='btn_showreply' class='btn btn-link' type='button' onclick='ShowReplies(\"" + user + "\", \"" + mid + "\");'>Comments</button>";
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

    output += "</ul>";

    return output;
}

function CreateMessage(user, mid, sid, eid, msg, posttime) {
    var output = "";

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
        html += "       <button class='btn btn-primary' type='button' id='btn_reply_" + mid + "' data-loading-text='Replying...' onclick='PostReply(\"" + user + "\", \"" + mid + "\");'>Reply</button>";
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
            $.each(data, function (index, item) {
                $("#replylist_" + mid).append(CreateReply(item));
                var rid = item.ReplyID;
                $.getJSON("/api/account/User", "userid=" + item.FromUser, function (data) {
                    var username = data.DisplayName;
                    var picurl = data.PortraitUrl;
                    if (isNullOrEmpty(picurl)) {
                        picurl = "/Content/Images/default_avatar.jpg";
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

function CreateReply(replyData) {
    var output = "";
    var user = replyData.FromUser;
    var msg = replyData.Message;
    var posttime = replyData.PostTime;
    var rid = replyData.ReplyID;

    output = "<li class='list-group-item'>";
    output += "  <div>"
    output += "    <div class='reply-pic'>";
    output += "      <img class='img-rounded' id='reply_user_pic_" + rid + "' src='/Content/Images/default_avatar.jpg' width='50' height='50' />";
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
    $("#events").empty();
    GetEvents(eid);
    $("#EventsModal").modal();


    //var show = $("#isshowevents_" + mid);
    //var newerdiv = $("#event_newer_" + mid);
    //var olderdiv = $("#event_older_" + mid);

    //if (show.length == 0) {
    //    return;
    //}

    //if (show.val() == "false") {
    //    // show events
    //    show.val("true");
    //    // show events
    //    LoadEvents(mid, eid);
    //}
    //else {
    //    // clear events
    //    show.val("false");
    //    newerdiv.html("");
    //    olderdiv.html("");
    //}
}

function GetEvents(eid) {
    var newerdiv = $("#events");
    $.ajax({
        type: "get",
        url: "/api/message/eventline",
        data: "eventID=" + eid,
        success: function (data) {
            var eventdiv = newerdiv;
            // create events list
            $.each(data, function (index, item) {

                eventdiv.append(CreateEvent(item));
                var id = item.EventID;
                $.getJSON("/api/account/User", "userid=" + item.User, function (data) {
                    var username = data.DisplayName;
                    var picurl = data.PortraitUrl;
                    if (isNullOrEmpty(picurl)) {
                        picurl = "/Content/Images/default_avatar.jpg";
                    }
                    $("#event_user_pic_" + id).attr("src", picurl);
                    $("#event_username_" + id).text(username);
                });
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
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
                var id = item.EventID;
                $.getJSON("/api/account/User", "userid=" + item.User, function (data) {
                    var username = data.DisplayName;
                    var picurl = data.PortraitUrl;
                    if (isNullOrEmpty(picurl)) {
                        picurl = "/Content/Images/default_avatar.jpg";
                    }
                    $("#event_user_pic_" + id).attr("src", picurl);
                    $("#event_username_" + id).text(username);
                });
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
    output += "        <img class='img-rounded' id='event_user_pic_" + eid + "' src='/Content/Images/default_avatar.jpg' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'><a id='event_username_" + eid + "' class='list-group-item-heading bold' href='/profile/index?user=" + user + "'>" + user + "</a>";
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


// search function
function SearchUser(keyword) {
    var apiurl = "";
    if (isNullOrEmpty(keyword))
        apiurl = "/api/account/user";
    else
        apiurl = "/api/account/searchuser?keyword=" + keyword;

    $.ajax({
        type: "get",
        url: apiurl,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                ShowError("No user found.");
            }
            else {
                // create user list
                $("#userlist").empty();
                $.each(data, function (index, item) {
                    $("#userlist").append(CreateUserCard(item));
                    LoadUserFollowBtn(item.Userid);
                })

            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function CreateUserCard(data) {
    var output = "";
    var userid = data.Userid;
    var username = data.DisplayName;
    var picurl = data.PortraitUrl;
    var desp = data.Description;
    var postscount = data.MessageCount;
    var followingcount = data.FollowingsCount;
    var followerscount = data.FollowersCount;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    output = "  <div class='user-card'>"
           + "    <div class='user-card-info '>"
           + "      <div class='ma-btm-10'>"
           + "        <img class='img-rounded' id='user_pic_" + userid + "' src='" + picurl + "' width='100' height='100' />"
           + "      </div>"
           + "      <div>"
           + "        <a class='' id='user_name_" + userid + "' href='/profile/index?user=" + userid + "'>" + username + "</a>"
           + "      </div>"
           + "      <div>"
           + "        <span id='user_id_" + userid + "'>" + userid + "</span>"
           + "      </div>"
           + "    </div>"
           + "    <div class='user-card-postinfo'>"
           + "      <div class='btn-group btn-group-justified'>"
           + "        <a class='btn btn-primary' id='btn_user_follow_" + userid + "'>Follow</a>"
           + "      </div>"
           + "    </div>"
           + "  </div>";

    return output;
}