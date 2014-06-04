/* common function */
function encodeEmail(code) {
    code = code.replace('@', '-');
    code = code.replace('.', '_');
    return code;
}

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

    $("#loading_message").html(msg);
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
            var isFollowimg = data.IsFollowing;

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

            LoadUserFollowBtn(user, isFollowimg);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function LoadUserFollowBtn(user, isFollowing) {
    var btn = $("#btn_user_follow_" + encodeEmail(user));
    if (btn.length == 0) {
        return;
    }

    if (isFollowing == 0) {
        SetFollowBtn(user, true);
    } else if (isFollowing == 1) {
        SetUnfollowBtn(user, true);
    } else {  // -1: myself
        SetEditProfileBtn(user);
    }
}

function SetEditProfileBtn(user) {
    var btn = $("#btn_user_follow_" + encodeEmail(user));
    if (btn.length == 0) {
        return;
    }

    btn.text("Edit profile");
    btn.attr("class", "btn btn-info");
    btn.attr("href", "/account/manage");
}

function SetUnfollowBtn(user, enabled) {
    var btn = $("#btn_user_follow_" + encodeEmail(user));
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
    var btn = $("#btn_user_follow_" + encodeEmail(user));
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger");
    btn.text("Unfollow");
}

function UnfollowBtnMouseOut(user) {
    var btn = $("#btn_user_follow_" + encodeEmail(user));
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success");
    btn.text("Following");
}

function SetFollowBtn(user, enabled) {
    var btn = $("#btn_user_follow_" + encodeEmail(user));
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
function LoadFeeds(category, id) {
    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(category))
        apiurl = "/api/message/userline";
    else if (category == "homeline")
        apiurl = "/api/message/homeline";
    else if (category == "atline")
        apiurl = "/api/message/atline";
    else if (category == "ownerline")
        apiurl = "/api/message/ownerline";
    else if (category == "publicsquareline")
        apiurl = "/api/message/publicsquareline";
    else if (category == "userline") {
        apiurl = "/api/message/userline";
        if (isNullOrEmpty(id)) {
            ShowError("Illegal operation.");
            return;
        }
        apidata = "userid=" + id;
    }
    else if (category == "topicline") {
        apiurl = "/api/message/topicline";
        if (isNullOrEmpty(id)) {
            ShowError("Illegal operation.");
            return;
        }
        apidata = "topicid=" + id;
    }
    else {
        ShowError("Illegal operation.");
        return;
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
                $("#lbl_seemore").html("");
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
    var user = postData.User.Userid;
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var posttime = postData.PostTime;
    var username = postData.User.DisplayName;
    var picurl = postData.User.PortraitUrl;
    var userdesp = postData.User.Description;
    var showevents = false;

    //alert(user);
    //alert(username);

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    if (!isNullOrEmpty(eid) && eid != "none") {
        showevents = true;
    }

    //output += "<ul class='list-group'>";
    
    output += "  <li class='list-group-item'>";
    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='user_pic_" + mid + "' src='" + picurl + "' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'><a id='username_" + mid + "' class='list-group-item-heading bold' href='/profile/index?user=" + user + "'>" + username + "</a>";
    output += "        &nbsp;<span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span></div>";
    output += "        <div class='newpost-input'>" + encodeHtml(msg) + "</div>";
    output += "        <div class='newpost-footer'>";
    
    if (showevents) {
        output += "      <button id='btn_expandevent' class='btn btn-link' type='button' onclick='ShowEvents(\"" + mid + "\", \"" + eid + "\");'>View events</button>";
    }
    output += "          <button id='btn_showreply' class='btn btn-link' type='button' onclick='ShowReplies(\"" + user + "\", \"" + mid + "\");'>Reply</button>";
    output += "        </div>";
    output += "      </div>";
    output += "    </div>";
    output += "    <div id='reply_" + mid + "'></div>";
    output += "    <input type='hidden' id='isshowreplies_" + mid + "' value='false'/>";
    output += "  </li>";

    //output += "</ul>";

    
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
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function CreateReply(replyData) {
    var output = "";
    var user = replyData.FromUser.Userid;
    var msg = replyData.Message;
    var posttime = replyData.PostTime;
    var rid = replyData.ReplyID;
    var username = replyData.FromUser.DisplayName;
    var picurl = replyData.FromUser.PortraitUrl;
    var userdesp = replyData.FromUser.Description;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    output = "<li class='list-group-item'>";
    output += "  <div>"
    output += "    <div class='reply-pic'>";
    output += "      <img class='img-rounded' id='reply_user_pic_" + rid + "' src='" + picurl + "' width='50' height='50' />";
    output += "    </div>";
    output += "    <div class='reply-content'>";
    output += "      <div class='reply-input'>";
    output += "        <a id='reply_username_" + rid + "' href='/profile/index?user=" + user + "'>" + username + "</a>";
    output += "        &nbsp;<span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>&nbsp;";
    output += "        " + encodeHtml(msg);
    output += "      </div>";
    output += "  </div>";
    output += "</li>";
    return output;
}


// event function
function ShowEvents(mid, eid) {
    var events_div = $("#events_div");
    if (events_div.length == 0) {
        return;
    }

    $.ajax({
        type: "get",
        url: "/api/message/eventline",
        data: "eventID=" + eid,
        success: function (data) {
            events_div.empty();
            // create events list
            $.each(data, function (index, item) {
                if (item.ID == mid)
                    events_div.append(CreateEvent(item, true));
                else
                    events_div.append(CreateEvent(item, false));
            })

            $("#EventsModal").modal();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function CreateEvent(postData, isHeighlight) {
    var output = "";
    var user = postData.User.Userid;
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var posttime = postData.PostTime;
    var username = postData.User.DisplayName;
    var picurl = postData.User.PortraitUrl;
    var userdesp = postData.User.Description;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    if (isHeighlight) {
        output += "  <li class='list-group-item'>";
    } else {
        output += "  <li class='list-group-item quote'>";
    }
    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='event_user_pic_" + eid + "' src='" + picurl + "' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'><a id='event_username_" + eid + "' class='list-group-item-heading bold' href='/profile/index?user=" + user + "'>" + username + "</a>";
    output += "        &nbsp;<span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span></div>";
    output += "        <div class='newpost-input'>" + encodeHtml(msg) + "</div>";
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


// search user function
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
                    LoadUserFollowBtn(item.Userid, item.IsFollowing);
                })

            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function LoadUsers(category, user) {
    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(category))
        apiurl = "/api/account/user";
    else if (category == "following")
        apiurl = "/api/account/followings";
    else if (category == "followers")
        apiurl = "/api/account/followers";
    else {
        ShowError("Illegal operation.");
        return;
    }

    if (!isNullOrEmpty(user)) {
        apidata = "userid=" + user;
    }

    $.ajax({
        type: "get",
        url: apiurl,
        data: apidata,
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
                    LoadUserFollowBtn(item.Userid, item.IsFollowing);
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
    var isFollowing = data.IsFollowing;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    output = "  <li class='userlist-group-item'>"
           + "    <div class='user-card-info'>"
           + "      <div class='ma-btm-10'>"
           + "        <img class='img-rounded' id='user_pic_" + encodeEmail(userid) + "' src='" + picurl + "' width='100' height='100' />"
           + "      </div>"
           + "      <div>"
           + "        <a class='' id='user_name_" + encodeEmail(userid) + "' href='/profile/index?user=" + userid + "'>" + username + "</a>"
           + "      </div>"
           + "      <div>"
           + "        <span id='user_id_" + encodeEmail(userid) + "'>@" + userid + "</span>"
           + "      </div>"
           + "    </div>"
           + "    <div class='user-card-postinfo'>"
           + "      <div class='btn-group btn-group-justified'>"
           + "        <a class='btn btn-default' id='btn_user_follow_" + encodeEmail(userid) + "'>&nbsp;</a>"
           + "      </div>"
           + "    </div>"
           + "  </li>";

    return output;
}


