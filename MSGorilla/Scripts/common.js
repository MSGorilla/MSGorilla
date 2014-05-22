/* common function */
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


/* notification function */
function ShowError(msg) {
    var msghtml = "<div class='alert alert-dismissable alert-warning'><button class='close' type='button' data-dismiss='alert'>×</button><p>" + msg + "</p></div>";
    $("#notifications").append(msghtml);
}


/* user infor function */
function LoadUserInfo(user) {
    var apiurl = "";
    if (isNullOrEmpty(user))
        apiurl = "/api/account/me";
    else
        apiurl = "/api/account/user?userid=" + user;

    $.ajax({
        type: "get",
        url: apiurl,
        success: function (data) {
            var userid = data.Userid;
            var username = data.DisplayName;
            var picurl = data.PortraitUrl;
            var desp = data.Description;
            var followingcount = data.FollowingsCount;
            var followerscount = data.FollowersCount;

            $("#user_id").html("@" + userid);
            $("#user_name").html(username);
            $("#user_name").attr("href", "/profile/index?user=" + username);
            if (!isNullOrEmpty(picurl)) {
                $("#user_pic").attr("src", picurl);
            }
            $("#user_following").html(followingcount);
            $("#user_followers").html(followerscount);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}


/* post page function */
function PostMessage(eventID, schemaID) {
    var message = $("#postmessage").val().trim();

    if (message.length === 0) {
        return;
    }

    $.ajax({
        type: "post",
        url: "/api/message/postmessage?" + "eventID=" + eventID + "&schemaID=" + schemaID + "&message=" + message,
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
                alert(msg);
                LoadReplies(user, mid);
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

function LoadFeeds(category) {
    var apiurl = "";
    if (isNullOrEmpty(category))
        apiurl = "/api/message/userline";
    if (category == "homeline")
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
                    $("#feedlist").append(createFeed(item));
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function createFeed(postData) {
    var output = "";
    var user = postData.User;
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var posttime = postData.PostTime;
    var username = user;
    var picurl = "";

    $.getJSON("/api/account/User", "userid=" + user, function (data) {
        username = data.DisplayName;
        picurl = data.PortraitUrl;
    });

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }
    output = " <li class='list-group-item'>";
    output += "  <div>"
    output += "    <div class='feed-pic'>";
    output += "      <img class='img-rounded' id='user_pic' src='" + picurl + "' width='100' height='100' />";
    output += "    </div>";
    output += "    <div class='feed-content'>";
    output += "      <div class='newpost-header'><a class='list-group-item-heading lead' href='/profile/index?user=" + user + "'>" + username + "</a>";
    output += "      <small>(" + posttime + ")</small></div>";
    output += "      <div class='newpost-input'><p>" + encodeHtml(msg) + "</p></div>";
    output += "      <div class='newpost-footer'><button id='btn_reply' class='btn btn-link' type='button' onclick='ShowReplies(\"" + user + "\", \"" + mid + "\");'>Comment</button></div>";
    output += "    </div>";
    output += "    <div class='clearfix'></div>";
    output += "  </div>";
    output += "  <div id='reply_" + mid + "'></div>";
    output += "  <input type='hidden' id='hd_" + mid + "' value='false'/>";
    output += "</li>";
    return output;
}

function ShowReplies(user, mid) {
    var show = $("#hd_" + mid);
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
        LoadReplies(user, mid);
    }
    else {
        // clear replies
        show.val("false");
        replydiv.html("");
    }
}

function LoadReplies(user, mid) {
    $.ajax({
        type: "get",
        url: "/api/message/getmessagereply",
        data: "msgID=" + mid,
        success: function (data) {
            // create reply list
            $("#replylist_" + mid).empty();
            $.each(data, function (index, item) {
                $("#replylist_" + mid).append(createReply(item));
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowError(textStatus + ": " + errorThrown);
        }
    });
}

function createReply(replyData) {
    var output = "";
    var user = replyData.FromUser;
    var msg = replyData.Message;
    var posttime = replyData.PostTime;
    var username = user;
    var picurl = "";

    $.getJSON("/api/account/User", "userid=" + user, function (data) {
        username = data.DisplayName;
        picurl = data.PortraitUrl;
    });

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }
    output = "<li class='list-group-item'>";
    output += "  <div>"
    output += "    <div class='reply-pic'>";
    output += "      <img class='img-rounded' id='user_pic' src='" + picurl + "' width='50' height='50' />";
    output += "    </div>";
    output += "    <div class='reply-content'>";
    output += "      <div class='reply-input'>";
    output += "        <a href='/profile/index?user=" + user + "'>" + username + "</a>&nbsp;:&nbsp;" + encodeHtml(msg);
    output += "        <small>(" + posttime + ")</small>";
    output += "      </div>";
    output += "  </div>";
    output += "</li>";
    return output;
}

