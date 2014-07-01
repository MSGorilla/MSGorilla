/* common function */
function isValidForUserid(c) {
    if ((c >= 'a' && c <= 'z')
        || (c >= '0' && c <= '9')
        || (c >= 'A' && c <= 'Z')
        || c == '-'
        )
        return true;
    else
        return false;
}

function isValidForTopic(c) {
    if ((c >= 'a' && c <= 'z')
        || (c >= '0' && c <= '9')
        || (c >= 'A' && c <= 'Z')
        //|| c == ' '
        || c == '-'
        || c == '.'
        || c == ','
        || c == ':'
        || c == '&'
        || c == '*'
        || c == '+'
        )
        return true;
    else
        return false;
}

function encodeEmail(code) {
    code = code.replace('@', '_');
    code = code.replace('.', '_');
    return code;
}

function ScrollTo(itemname) {
    var scroll_offset = $("#" + itemname).offset();
    $("body,html").animate({
        scrollTop: scroll_offset.top - 60
    }, "slow");
}

function Txt2Html(code) {
    code = code.replace(/&/mg, '&#38;');
    code = code.replace(/</mg, '&#60;');
    code = code.replace(/>/mg, '&#62;');
    code = code.replace(/\"/mg, '&#34;');
    code = code.replace(/\t/g, '  ');
    code = code.replace(/\r?\n/g, '<br/>');
    code = code.replace(/ /g, '&nbsp;');

    return code;
}

function Html2Txt(code) {
    code = code.replace('&nbsp;', ' ');
    code = code.replace('<br/>', '\n');
    code = code.replace('<br>', '\n');
    code = code.replace('</br>', '');
    code = code.replace('&#34;', '"');
    code = code.replace('&#62;', '>');
    code = code.replace('&#60;', '<');
    code = code.replace('&#38;', '&');

    return code;
}

function encodeHtml(code, atusers, topics) {
    var code = Txt2Html(code);

    // autolink http[s]
    var strRegex = "http[s]?://(([0-9a-z]+(\\-)?)*[0-9a-z]+(\\.))*[0-9a-z]+(:[0-9]{1,5})?(/([\\w\\-/\\+\\?%#=\\.:{}]|(&#38;))*)?";
    var linkre = new RegExp(strRegex, "gi");

    code = code.replace(linkre, function (s) {
        return ('<a href="' + (Html2Txt(s)) + '">' + s + '</a>');
    });

    // autolink @user
    if (!isNullOrEmpty(atusers)) {
        for (var key in atusers) {
            var user = atusers[key];
            var htmluser = Txt2Html(user);
            code = code.replace("@" + htmluser, ('<a href="/profile/index?user=' + encodeURIComponent(user) + '">@' + htmluser + '</a>'));
        }

        //strRegex = "@([0-9a-z\\-]+)(\\s|$)";
        //var atre = new RegExp(strRegex, "gi");

        //code = code.replace(atre, function (s1, s2, s3) {
        //    if ($.inArray(s2, atusers) >= 0) {
        //        return encodeURIComponent('<a href="/profile/index?user=' + s2 + '">@' + s2 + '</a>') + s3;
        //    }
        //    else {
        //        return s1;
        //    }
        //});
    }

    // autolink #topic#
    if (!isNullOrEmpty(topics)) {
        for (var key in topics) {
            var topic = topics[key];
            var htmltopic = Txt2Html(topic);

            code = code.replace("#" + htmltopic + "#", ('<a href="/topic/index?topic=' + encodeURIComponent(topic) + '">#' + htmltopic + '#</a>'));
        }

        //strRegex = "#(([\\w \\-]+(#{2,})?)*[\\w \\-]+)#(\\s|$)";
        //var topicre = new RegExp(strRegex, "gi");

        //code = code.replace(topicre, function (s1, s2, s3, s4,s5,s6,s7) {
        //    if ($.inArray(s2, topics) >= 0) {
        //        return encodeURIComponent('<a href="/topic/index?topic=' + s2 + '">#' + s2 + '#</a>') + s5;
        //    }
        //    else {
        //        return s1;
        //    }
        //});
    }

    return code;
}

function encodeTxt(code) {
    code = encodeURIComponent(code);
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
    else if (diff / hour > 2) {
        return Math.floor(diff / hour) + " hrs";
    }
    else if (diff / hour > 1) {
        return "1 hr";
    }
    else if (diff / min > 2) {
        return Math.floor(diff / min) + " mins";
    }
    else if (diff / min > 1) {
        return "1 min";
    }
        //else if (diff / sec > 1) {
        //    return Math.floor(diff / sec) + "secs";
        //}
    else {
        return "Just now";
    }
}


/* show message function */
function ShowError(msg, boxno) {
    var msghtml = "<div class='alert alert-dismissable alert-warning'><button class='close' type='button' data-dismiss='alert'>×</button><p>" + msg + "</p></div>";
    $("#notifications").append(msghtml);

    if (isNullOrEmpty(boxno)) {
        $("#loading_message").html(msg);
    } else {
        $("#loading_message_" + boxno).html(msg);
    }
}

function ShowAjaxError(status, error, code, msg, boxno) {
    if (code == 2004) // access denied. need login again.
        location.href = "/Error/?message=" + encodeURIComponent(msg) + "&returnUrl=" + encodeURIComponent("/Account/Logoff");

    var errormsg = "[" + status + "]" + " " + error + ": " + (isNullOrEmpty(code) ? "" : code) + " " + msg;
    ShowError(errormsg, boxno);
}


/* user infor function */
function LoadMyInfo() {
    var apiurl = "/api/account/me";

    $.ajax({
        type: "GET",
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadUserInfo(user) {
    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(user)) {
        apiurl = "/api/account/me";
    }
    else {
        apiurl = "/api/account/user";
        apidata = "userid=" + encodeURIComponent(user);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
        type: "GET",
        url: "/api/account/follow",
        data: "userid=" + encodeURIComponent(user),
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function Unfollow(user) {
    SetFollowBtn(user, false);
    $.ajax({
        type: "GET",
        url: "/api/account/unfollow",
        data: "userid=" + encodeURIComponent(user),
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

/* post function */
function PostMessage() {
    var message = encodeTxt($("#postmessage").val().trim());
    if (message.length === 0) {
        return;
    }

    $("#btn_post").button('loading');
    $.ajax({
        type: "POST",
        url: "/api/message/postmessage",
        data: "message=" + message, // + "&richMessage=" + message,
        dataType: "json",
        success: function (data) {
            $("#postmessage").val("");
            // insert the new posted message
            $("#feedlist").prepend(CreateFeed(data));
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    }).always(function () {
        $("#btn_post").button('reset');
    });
}

function PostReply(user, mid) {
    var replybox = $("#replymessage_" + mid);
    var message = encodeTxt(replybox.val().trim());
    var touser = replybox.attr("replyto");

    if (message.length === 0) {
        return;
    }

    if (isNullOrEmpty(touser)) {
        touser = user;
    }

    $("#btn_reply_" + mid).button('loading');
    $.ajax({
        type: "POST",
        url: "/api/reply/postreply",
        data: "to=" + encodeURIComponent(touser) + "&message=" + message + "&messageUser=" + encodeURIComponent(user) + "&messageID=" + encodeURIComponent(mid),
        dataType: "json",
        success: function (data) {
            $("#replymessage_" + mid).val("");
            $("#replylist_" + mid).prepend(CreateReply(data));
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    }).always(function () {
        $("#btn_reply_" + mid).button('reset');
    });
}


// feed function
function LoadMessage(user, mid) {
    var apiurl = "";
    var apidata = "";

    if (isNullOrEmpty(user) || isNullOrEmpty(mid)) {
        ShowError("Illegal operation.");
        return;
    }

    apiurl = "/api/message/getdisplaymessage";
    apidata = "msgUser=" + encodeURIComponent(user) + "&msgID=" + encodeURIComponent(mid);

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        data: apidata,
        success: function (data) {
            if (isNullOrEmpty(data)) {
                ShowError("No content.");
            }
            else {
                $("#feedlist").empty();
                // create feed 
                $("#feedlist").append(CreateFeed(data));
                $("#reply_" + mid).collapse('show');
                ShowReplies(mid);
            }

        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function FilterFeeds(category, id, filter) {
    if (isLoadFeeds) return;    // if it is loading, skip this time.

    var filters = [
        "all",
        "latest24hours",
        "latest3days",
        "latest7days",
        "latest1month"
    ];

    for (var f in filters) {
        if (filters[f] == filter) {
            $("#feedfilter_" + filter).addClass("active");
            $("#feedfilter_title").text($("#feedfilter_" + filter).text());
            LoadFeeds(category, id, filter);
        } else {
            $("#feedfilter_" + filters[f]).removeClass("active");
        }
    }
}

var isLoadFeeds = false;
function LoadFeeds(category, id, filter) {
    if (isLoadFeeds) return;    // if it is loading, skip this time.
    isLoadFeeds = true;

    var apiurl = "";
    var apidata = "";
    var token = "";
    var count = 0;

    if (category == "homeline") {
        apiurl = "/api/message/homeline";
        apidata = "userid=" + encodeURIComponent(id);
    }
    else if (category == "atline") {
        apiurl = "/api/message/atline";
        apidata = "userid=" + encodeURIComponent(id);
    }
    else if (category == "ownerline") {
        apiurl = "/api/message/ownerline";
        apidata = "userid=" + encodeURIComponent(id);
    }
    else if (category == "replyline") {
        apiurl = "/api/reply/getmyreply";
    }
    else if (category == "publicsquareline")
        apiurl = "/api/message/publicsquareline";
    else if (category == "userline") {
        apiurl = "/api/message/userline";
        apidata = "userid=" + encodeURIComponent(id);
    }
    else if (category == "topicline") {
        apiurl = "/api/message/topicline";
        if (isNullOrEmpty(id)) {
            ShowError("Illegal operation.");
            isLoadFeeds = false;
            return;
        }
        apidata = "topic=" + encodeURIComponent(id);
    }
    else if (category == "message") {
        apiurl = "/api/message/topicline";
        if (isNullOrEmpty(id)) {
            ShowError("Illegal operation.");
            isLoadFeeds = false;
            return;
        }
        apidata = "topic=" + encodeURIComponent(id);
    }
    else {
        ShowError("Illegal operation.");
        isLoadFeeds = false;
        return;
    }


    if (isNullOrEmpty(filter)) { // page load or load more
        token = $("#hd_token").val();
        filter = $("#hd_filter").val();
    }
    else {  // click on filter btn, clear current feeds and add loading gif
        $("#feedlist").empty();
        $("#feedlist").append("<li id='loading_message' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

        $("#hd_token").val(token);
        $("#hd_filter").val(filter);
    }

    if (!isNullOrEmpty(filter)) {   // set filter value
        if (isNullOrEmpty(apidata)) {
            apidata = "filter=" + filter;
        }
        else {
            apidata += "&filter=" + filter;
        }
    }

    if (isNullOrEmpty(token)) { // first time load, not load more
        count = GetNotificationCount(category);
        $("#hd_newcount").val(count);
    } else {    // load more
        if (token == "nomore") {
            // already no more feeds, don't load any more
            isLoadFeeds = false;
            return;
        }

        count = $("#hd_newcount").val();
        if (isNullOrEmpty(apidata)) {
            apidata = "token=" + token;
        }
        else {
            apidata += "&token=" + token;
        }
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        data: apidata,
        success: function (data) {
            var nexttoken = null;
            if (isNullOrEmpty(data)) {
                ShowError("No content.");
            }
            else {
                nexttoken = data.continuationToken
                if (category == "replyline")
                    data = data.reply;
                else
                    data = data.message
                //ShowError(data);
                if (data.length == 0) {
                    ShowError("No content.");
                }
                else {
                    // create feed list
                    if (isNullOrEmpty(token)) {
                        // clear feeds at the first time 
                        $("#feedlist").empty();
                    }
                    $.each(data, function (index, item) {
                        var isNew = false;
                        if (count > 0) {
                            isNew = true;
                            count--;
                        }
                        if (category == "replyline")
                            $("#feedlist").append(CreateReply(item, true, isNew));
                        else
                            $("#feedlist").append(CreateFeed(item, isNew));
                    })
                }
            }

            if (isNullOrEmpty(nexttoken)) {
                $("#lbl_seemore").html("");
                $("#hd_token").val("nomore");
                $("#hd_newcount").val(0);
            }
            else {
                $("#lbl_seemore").html("<span class='spinner-loading'></span> Loading more...");
                $("#hd_token").val(nexttoken);
                $("#hd_newcount").val(count);
            }

            UpdateNotificationCount();
            LoadMyFavoriteTopics();
            isLoadFeeds = false;
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
            $("#lbl_seemore").html("");
            $("#hd_token").val("nomore");
            isLoadFeeds = false;
        }
    });
}

var ImportenceTags = [
    "", // 0
    "<span class='label label-info'>Info</span>",
    "<span class='label label-primary'>Primary</span>",
    "<span class='label label-success'>Success</span>",
    "<span class='label label-warning'>Warning</span>",
    "<span class='label label-danger'>Danger</span>",
    "<span class='label label-default'>Default</span>",
];

function CreateFeed(postData, isNew) {
    var output = "";
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var richmsgid = postData.RichMessageID;
    var posttime = postData.PostTime;
    var user = postData.User.Userid;
    var username = postData.User.DisplayName;
    var picurl = postData.User.PortraitUrl;
    var userdesp = postData.User.Description;
    var owners = postData.Owner;
    var atusers = postData.AtUser;
    var topics = postData.TopicName;
    var attach = postData.Attachment;
    var importence = postData.Importance;
    var showevents = false;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    if (!isNullOrEmpty(eid) && eid != "none") {
        showevents = true;
    }

    //output += "<ul class='list-group'>";
    if (isNew == true) {
        output += "  <li id='feed_" + mid + "' class='list-group-item new-notification'>";
    }
    else {
        output += "  <li id='feed_" + mid + "' class='list-group-item'>";
    }

    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='user_pic_" + mid + "' src='" + picurl + "' width='100' height='100' />";
    output += "      </div>";

    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'>";
    output += "          <a id='username_" + mid + "' class='fullname' href='/profile/index?user=" + encodeURIComponent(user) + "'>" + username + "</a>&nbsp;";
    output += "          <span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>";
    output += "        </div>";

    output += "        <div class='newpost-input'>";
    // importance
    if (importence > 2) {
        output += ImportenceTags[importence] + "&nbsp;";
    }

    // owners
    if (!isNullOrEmpty(owners)) {
        output += "      <span class=''>Owned by: </span>";
        for (var key in owners) {
            output += "  <a href='/profile/index?user=" + encodeURIComponent(owners[key]) + "'>@" + owners[key] + "</a>&nbsp;";
        }
    }
    output += "        </div>";

    // message
    output += "        <div class='newpost-input'>" + encodeHtml(msg, atusers, topics) + "</div>";

    // richmsg
    if (!isNullOrEmpty(richmsgid)) {
        output += "    <div id='rich_message_" + mid + "' class='newpost-input panel-collapse collapse out clickable' data-toggle='collapse' data-parent='#feed_" + mid + "' href='#rich_message_" + mid + "' onclick='ShowRichMsg(\"" + richmsgid + "\", \"" + mid + "\");'></div>";
    }

    // attachment
    if (!isNullOrEmpty(attach)) {
        output += "    <div class='newpost-input'><span class=''>Attachments: </span>";
        for (var key in attach) {
            output += "  <a class='btn btn-link btn-xs' onclick='ShowAttach(\"" + attach[key].AttachmentID + "\", \"" + attach[key].Filetype + "\", \"" + mid + "\");' >" + attach[key].Filename + " (" + attach[key].Filesize + ")</a>&nbsp;";
        }
        output += "    </div>";
        output += "    <div id='attachment_" + mid + "' class='newpost-input' style='display:none;'></div>";
    }

    // footer btns
    output += "        <div class='newpost-footer'>";

    // event button
    //if (showevents) {
    //    output += "      <button id='btn_expandevent' class='btn btn-link btn-sm' type='button' onclick='ShowEvents(\"" + mid + "\", \"" + eid + "\");'>Related threads</button>";
    //}

    // btns
    // output += "       <a id='btn_open_msg_" + mid + "' class='btn btn-link btn-sm' target='_blank' href='/Message/Index?user=" + encodeURIComponent(user) + "&msgID=" + encodeURIComponent(mid) + "'>Open</a>";
    if (!isNullOrEmpty(richmsgid)) {
        output += "      <button id='btn_showrichmsg_" + mid + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + mid + "' href='#rich_message_" + mid + "' onclick='ShowRichMsg(\"" + richmsgid + "\", \"" + mid + "\");'>More</button>";
    }
    output += "          <button id='btn_showreply_" + mid + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + mid + "' href='#reply_" + mid + "' onclick='ShowReplies(\"" + mid + "\");'>Replies</button>";

    output += "        </div>";
    output += "      </div>";
    output += "    </div>";

    // reply
    output += "    <div id='reply_" + mid + "' class='panel-collapse collapse out '>";
    // add reply box
    output += "      <div class='replies-container'>";
    output += "        <div class='input-group'>";
    //output += "         <span class='input-group-addon'>Comment</span>";
    output += "          <input class='form-control' type='text' id='replymessage_" + mid + "' replyto='" + user + "' placeholder='Reply to @" + user + "'>";
    output += "          <span class='input-group-btn'>";
    output += "            <button class='btn btn-primary' type='button' id='btn_reply_" + mid + "' data-loading-text='Replying...' onclick='PostReply(\"" + user + "\", \"" + mid + "\");'>Reply</button>";
    output += "          </span>";
    output += "        </div>";
    // add replies
    output += "        <div class='replies-feeds'>";
    output += "          <ul id='replylist_" + mid + "' class='list-group'></ul>";
    output += "        </div>";
    output += "      </div>";
    output += "    </div>";

    output += "  </li>";

    //output += "</ul>";

    return output;
}

function ShowAttach(aid, type, mid) {
    var attadiv = $("#attachment_" + mid);
    var apiurl = "/api/attachment/download?attachmentid=" + aid;

    attadiv.empty();
    switch (type) {
        default:
            window.open(apiurl);
            break;
    }
}

function ShowRichMsg(rmid, mid) {
    var showbtn = $("#btn_showrichmsg_" + mid);
    var richmsgdiv = $("#rich_message_" + mid);
    var show = richmsgdiv.hasClass("in");

    if (show == false) {
        if (isNullOrEmpty(richmsgdiv.html())) {
            richmsgdiv.append("<div class='txtaln-c'><span class='spinner-loading'></span> Loading...</div>");
            // load rich once
            $.ajax({
                type: "GET",
                url: "/api/message/getrichmessage",
                data: "richMsgID=" + encodeURIComponent(rmid),
                success: function (data) {
                    richmsgdiv.empty();
                    // create rich message
                    richmsgdiv.append(data);

                    // show rich
                    showbtn.text("Less");
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    richmsgdiv.empty();
                    ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
                }
            });
        }
        else {
            // show rich
            showbtn.text("Less");
        }
    }
    else {
        // clear rich
        showbtn.text("More");
    }

    ScrollTo("feed_" + mid);
}


// reply function
function ShowReplies(mid) {
    var showbtn = $("#btn_showreply_" + mid);
    var replydiv = $("#reply_" + mid);
    var show = replydiv.hasClass("in");

    if (show == false) {
        // show replies
        showbtn.text("Collapse");

        // show reply
        LoadReplies(mid);
    }
    else {
        // clear replies
        showbtn.text("Replies");
    }
}

function LoadReplies(mid) {
    var replydiv = $("#replylist_" + mid);

    replydiv.empty();
    replydiv.append("<li class='center-block txtaln-c'><span class='spinner-loading'></span> Loading...</li>");
    $.ajax({
        type: "GET",
        url: "/api/message/getmessagereply",
        data: "msgID=" + encodeURIComponent(mid),
        success: function (data) {
            // create reply list
            replydiv.empty();
            $.each(data, function (index, item) {
                replydiv.append(CreateReply(item, false));
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            replydiv.empty();
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function CreateReply(replyData, isReplyFeed, isNew) {
    var output = "";
    var user = replyData.FromUser.Userid;
    var username = replyData.FromUser.DisplayName;
    var picurl = replyData.FromUser.PortraitUrl;
    var userdesp = replyData.FromUser.Description;
    var msg = replyData.Message;
    var posttime = replyData.PostTime;
    var rid = replyData.ReplyID;
    var mid = replyData.MessageID;
    var touser = replyData.ToUser.Userid;
    var tousername = replyData.ToUser.DisplayName;
    var msguser = replyData.MessageUser.Userid;
    var msgusername = replyData.MessageUser.DisplayName;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    if (isReplyFeed) {
        if (isNew == true) {
            output += "<li class='list-group-item clickable new-notification' onclick='ShowMessage(event, \"" + mid + "\", \"" + msguser + "\");'>";
        }
        else {
            output += "<li class='list-group-item clickable' onclick='ShowMessage(event, \"" + mid + "\", \"" + msguser + "\");'>";
        }
    } else {
        output += "<li class='list-group-item clickable' onclick='SetReplyTo(event, \"" + mid + "\", \"" + user + "\");'>";
    }

    output += "  <div>"
    output += "    <div class='reply-pic'>";
    output += "      <img class='img-rounded' id='reply_user_pic_" + rid + "' src='" + picurl + "' width='50' height='50' />";
    output += "    </div>";
    output += "    <div class='reply-content'>";
    output += "      <div class='reply-header'>"
    output += "        <a id='reply_username_" + rid + "' class='fullname' href='/profile/index?user=" + encodeURIComponent(user) + "'>" + username + "</a>";
    output += "        &nbsp;<span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>";
    output += "      </div>";
    output += "      <div class='reply-input'>";
    output += "        <a id='reply_tousername_" + rid + "' href='/profile/index?user=" + encodeURIComponent(touser) + "'>@" + touser + "</a>&nbsp;";
    output += "        " + encodeHtml(msg);
    output += "      </div>";
    output += "    </div>";
    output += "  </div>";
    output += "</li>";

    return output;
}

function SetReplyTo(evt, mid, user) {
    var sender = null;
    if (window.event) {
        sender = window.event.target;
        event.cancelBubble = true;
    } else if (evt) {
        sender = evt.target;
        evt.stopPropagation();
    }
    if (!isNullOrEmpty(sender.href)) {  // link clicked
        return;
    }

    var replybox = $("#replymessage_" + mid);

    if (replybox.length == 0) {
        return;
    }

    replybox.attr("replyto", user);
    replybox.attr("placeholder", "Reply to @" + user);
}

function ShowMessage(evt, mid, user) {
    var sender = null;
    if (window.event) {
        sender = window.event.target;
        event.cancelBubble = true;
    } else if (evt) {
        sender = evt.target;
        evt.stopPropagation();
    }
    if (!isNullOrEmpty(sender.href)) {  // link clicked
        return;
    }

    var message_div = $("#message_div");
    if (message_div.length == 0) {
        return;
    }

    $("#MessageModal").modal();
    message_div.empty();
    message_div.append("<li id='loading_message' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    $.ajax({
        type: "GET",
        url: "/api/message/getdisplaymessage",
        data: "msgUser=" + encodeURIComponent(user) + "&msgID=" + encodeURIComponent(mid),
        success: function (data) {
            message_div.empty();
            // create message
            message_div.append(CreateFeed(data));
            $("#reply_" + mid).collapse('show');
            ShowReplies(mid);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}


// event function
function ShowEvents(mid, eid) {
    var events_div = $("#events_div");
    if (events_div.length == 0) {
        return;
    }

    $.ajax({
        type: "GET",
        url: "/api/message/eventline",
        data: "eventID=" + encodeURIComponent(eid),
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function CreateEvent(postData, isHeighlight) {
    var output = "";
    var mid = postData.ID;
    var sid = postData.SchemaID;
    var eid = postData.EventID;
    var msg = postData.MessageContent;
    var richmsg = postData.RichMessage;
    var posttime = postData.PostTime;
    var user = postData.User.Userid;
    var username = postData.User.DisplayName;
    var picurl = postData.User.PortraitUrl;
    var userdesp = postData.User.Description;
    var owners = postData.Owner;
    var atusers = postData.AtUser;
    var topics = postData.TopicName;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    if (isHeighlight) {
        output += "  <li class='list-group-item link-line'>";
    } else {
        output += "  <li class='list-group-item link-line quote'>";
    }
    output += "    <div>"
    output += "      <div class='feed-pic'>";
    output += "        <img class='img-rounded' id='event_user_pic_" + eid + "' src='" + picurl + "' width='100' height='100' />";
    output += "      </div>";
    output += "      <div class='feed-content'>";
    output += "        <div class='newpost-header'>";
    output += "          <a id='event_username_" + eid + "' class='fullname' href='/profile/index?user=" + encodeURIComponent(user) + "'>" + username + "</a>&nbsp;";
    output += "          <span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>";
    output += "        </div>";

    if (!isNullOrEmpty(owners)) {
        output += "    <div class='newpost-input'><span class=''>Owned by: </span>";
        for (var key in owners) {
            output += "  <a href='/profile/index?user=" + encodeURIComponent(owners[key]) + "'>@" + owners[key] + "</a>&nbsp;";
        }
        output += "    </div>";
    }

    output += "        <div class='newpost-input'>" + encodeHtml(msg) + "</div>";

    if (!isNullOrEmpty(richmsg)) {
        output += "        <div class='newpost-input'>" + richmsg + "</div>";
    }

    output += "      </div>";
    output += "    </div>";
    output += "  </li>";

    return output;
}


// search function
function SearchTopic(keyword) {
    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(keyword))
        apiurl = "/api/topic/getalltopic";
    else {
        apiurl = "/api/topic/searchtopic";
        apidata = "keyword=" + encodeURIComponent(keyword);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                ShowError("No content.");
            }
            else {
                $("#topiclist").empty();
                $.each(data, function (index, item) {
                    var topicid = item.Id;
                    var topicname = item.Name;
                    var topicdesp = item.Description;
                    var topiccount = item.MsgCount;

                    $("#topiclist").append(CreateTopic(topicid, topicname, topicdesp, topiccount));
                    LoadTopicLikeBtn("btn_topic_like_" + topicid, topicid);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function SearchUser(keyword) {
    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(keyword))
        apiurl = "/api/account/user";
    else {
        apiurl = "/api/account/searchuser";
        apidata = "keyword=" + encodeURIComponent(keyword);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                ShowError("No content.", "1");
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message, "1");
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
        apidata = "userid=" + encodeURIComponent(user);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                ShowError("No content.");
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
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
           + "        <a class='' id='user_name_" + encodeEmail(userid) + "' href='/profile/index?user=" + encodeURIComponent(userid) + "'>" + username + "</a>"
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


// notification count function
function GetNotificationCount(category) {
    var count = 0;

    if (category != "atline"
        && category != "ownerline"
        && category != "replyline"
        && category != "homeline"
        ) {
        return count;
    }

    var apiurl = "/api/account/getnotificationcount";
    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        async: false,
        success: function (data) {
            var homelineCount = data.UnreadHomelineMsgCount;
            var ownerlineCount = data.UnreadOwnerlineMsgCount;
            var atlineCount = data.UnreadAtlineMsgCount;
            var replyCount = data.UnreadReplyCount;
            var userid = data.Userid;
            var notificationCount = ownerlineCount + replyCount + atlineCount;

            switch (category) {
                case "atline":
                    count = atlineCount;
                    break;
                case "ownerline":
                    count = ownerlineCount;
                    break;
                case "replyline":
                    count = replyCount;
                    break;
                case "homeline":
                    count = homelineCount;
                    break;
                default:
                    break;
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });

    return count;
}

function UpdateNotificationCount() {
    var apiurl = "/api/account/getnotificationcount";

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        success: function (data) {
            var homelineCount = data.UnreadHomelineMsgCount;
            var ownerlineCount = data.UnreadOwnerlineMsgCount;
            var atlineCount = data.UnreadAtlineMsgCount;
            var replyCount = data.UnreadReplyCount;
            var userid = data.Userid;
            var notificationCount = ownerlineCount + replyCount + atlineCount;

            // shortcut panel count
            $("#shortcut_homeline_count").html(homelineCount);
            $("#shortcut_reply_count").html(replyCount);
            $("#shortcut_atline_count").html(atlineCount);
            $("#shortcut_ownerline_count").html(ownerlineCount);
            $("#shortcut_notification_count").html(notificationCount);

            // homepage count
            $("#home_reply_count").html(replyCount);
            $("#home_atline_count").html(atlineCount);
            $("#home_ownerline_count").html(ownerlineCount);

            // nav bar count
            if (homelineCount > 0)
                $("#nav_home_count").html(homelineCount);
            else
                $("#nav_home_count").html("");

            if (notificationCount > 0)
                $("#nav_notification_count").html(notificationCount);
            else
                $("#nav_notification_count").html("");

            // chrome desktop notification
            notify(homelineCount, atlineCount, ownerlineCount, replyCount);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function SetNotificationCount() {
    UpdateNotificationCount();
    var timer = $.timer(UpdateNotificationCount, 30000, true);
}


// topic function
function LoadHotTopics() {
    var apiurl = "/api/topic/hottopics";

    $.ajax({
        type: "GET",
        url: apiurl,
        data: "count=10",
        dataType: "json",
        success: function (data) {
            // create hot topic
            $("#topic_collapse").empty();
            $.each(data, function (index, item) {
                var output = "";
                var topicid = item.Id;
                var topicname = item.Name;
                var topicdesp = item.Description;
                var topiccount = item.MsgCount;

                output += "<li class='sub-list-group-item'>";
                output += "  <a class='btn btn-default like-btn' id='shortcut_btn_topic_like_" + topicid + "' style='display:none'>&nbsp;</a>";
                //output += "  <span class='badge'>" + topiccount + "</span>";
                output += "  <a href='/topic/index?topic=" + encodeURIComponent(topicname) + "'>#" + topicname + "#</a>";
                output += "</li>";
                $("#topic_collapse").append(output);
                LoadTopicLikeBtn("shortcut_btn_topic_like_" + topicid, topicid);
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadUserFavoriteTopics(user) {
    var apiurl = "/api/topic/getuserfavouritetopic";
    var apidata = "userid=" + encodeURIComponent(user);

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                ShowError("No content.");
            }
            else {
                $("#topiclist").empty();
                $.each(data, function (index, item) {
                    var topicid = item.topicID;
                    var topicname = item.topicName;
                    var topicdesp = item.topicDescription;
                    var topiccount = item.topicMsgCount;

                    $("#topiclist").append(CreateTopic(topicid, topicname, topicdesp, topiccount));
                    LoadTopicLikeBtn("btn_topic_like_" + topicid, topicid);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadMyFavoriteTopics() {
    var apiurl = "/api/topic/getmyfavouritetopic";

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        success: function (data) {
            // create hot topic
            $("#favorite_collapse").empty();
            var navlist = $("#home_nav_list");
            navlist.empty();
            $.each(data, function (index, item) {
                var output = "";
                var userid = item.userid;
                var topicid = item.topicID;
                var unreadcount = item.UnreadMsgCount;
                var topicname = item.topicName;
                var topicdesp = item.topicDescription;
                var topiccount = item.topicMsgCount;

                // shortcut panel
                output += "<li class='sub-list-group-item'>";
                output += "  <span class='badge'>" + unreadcount + "</span>";
                output += "  <a href='/topic/index?topic=" + encodeURIComponent(topicname) + "'>#" + topicname + "#</a>";
                output += "</li>";
                $("#favorite_collapse").append(output);

                // homepage 
                if (unreadcount > 0 && navlist.length > 0) {
                    output = "";
                    //output += "<li>";
                    output += "  <a class='btn btn-link btn-xs' href='/topic/index?topic=" + encodeURIComponent(topicname) + "'>#" + topicname + "# <span class='badge'>" + unreadcount + "</span></a> ";
                    //output += "</li>";

                    navlist.append(output);
                }
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            //ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadTopicLikeBtn(btnid, topicid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    var apiurl = "/api/topic/isfavouritetopic";
    var apidata = "topicID=" + encodeURIComponent(topicid);

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data == true) {
                SetUnlikeBtn(btnid, topicid, true);
            }
            else if (data == false) {
                SetLikeBtn(btnid, topicid, true);
            }
            else {
                // do nothing.
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            //ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function SetUnlikeBtn(btnid, topicid, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Liked");
    btn.attr("class", "btn btn-success like-btn");
    if (enabled) {
        btn.attr("onclick", "Unlike('" + btnid + "', '" + topicid + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "UnlikeBtnMouseOver('" + btnid + "');")
    btn.attr("onmouseout", "UnlikeBtnMouseOut('" + btnid + "');")
    btn.show();
}

function UnlikeBtnMouseOver(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger like-btn");
    btn.text("Unlike");
}

function UnlikeBtnMouseOut(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success like-btn");
    btn.text("Liked");
}

function SetLikeBtn(btnid, topicid, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Like");
    btn.attr("class", "btn btn-primary like-btn");
    if (enabled) {
        btn.attr("onclick", "Like('" + btnid + "', '" + topicid + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
    btn.show();
}

function Like(btnid, topicid) {
    SetUnlikeBtn(btnid, topicid, false);
    $.ajax({
        type: "GET",
        url: "/api/topic/addfavouritetopic",
        data: "topicID=" + encodeURIComponent(topicid),
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadMyFavoriteTopics();
                SetUnlikeBtn(btnid, topicid, true);
            }
            else {
                ShowError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function Unlike(btnid, topicid) {
    SetLikeBtn(btnid, topicid, false);
    $.ajax({
        type: "GET",
        url: "/api/topic/removefavouritetopic",
        data: "topicID=" + encodeURIComponent(topicid),
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadMyFavoriteTopics();
                SetLikeBtn(btnid, topicid, true);
            }
            else {
                ShowError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            ShowAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function CreateTopic(topicid, topicname, topicdesp, topiccount) {
    var output = "";

    output += "<li class='list-group-item'>";
    output += "  <a class='btn btn-default like-btn' id='btn_topic_like_" + topicid + "' style='display:none'>&nbsp;</a>";
    output += "  <span class='badge'>" + topiccount + "</span>";
    output += "  <a class='fullname' href='/topic/index?topic=" + encodeURIComponent(topicname) + "'>#" + topicname + "#</a>";
    output += "  <span class='username' >" + (isNullOrEmpty(topicdesp) ? "" : topicdesp) + "</span>&nbsp;";
    output += "</li>";

    return output;
}


function notify(homelineCount, atlineCount, ownerlineCount, replyCount) {
    try {
        if (isNullOrEmpty(chrome)) {
            return;
        }

        //chrome.notification.getPermissionLevel(function (level) {
        //    if (level == "granted") {
        var notifitems = [];

        if (atlineCount > 0) {
            notifitems.push({
                title: "Mentions: ", message: atlineCount
            });
        }
        if (ownerlineCount > 0) {
            notifitems.push({
                title: "Owned : ", message: ownerlineCount
            });
        }
        if (replyCount > 0) {
            notifitems.push({
                title: "Replies : ", message: replyCount
            });
        }

        var opt = {
            type: "list",
            title: "Notifications",
            message: "You have new unread messages.",
            iconUrl: "/Content/Images/default_avatar.jpg",
            items: notifitems
        };

        if (notifitems.length > 0) {
            chrome.notifications.create('chrome_notification', opt, function (id) {
            });
        }
        //    }
        //    else if (level == "denied") {
        //        return;
        //    }
        //});
    }
    catch (e) {
    }
}

//function notify() {
//    alert(window.webkitNotifications);
//    if (window.webkitNotifications) {
//        if (window.webkitNotifications.checkPermission() == 0) {
//            var notification_test = window.webkitNotifications.createNotification("http://images.cnblogs.com/cnblogs_com/flyingzl/268702/r_1.jpg", '标题', '内容' + new Date().getTime());
//            notification_test.display = function () { }
//            notification_test.onerror = function () { }
//            notification_test.onclose = function () { }
//            notification_test.onclick = function () { this.cancel(); }

//            notification_test.replaceId = 'Meteoric';

//            notification_test.show();

//            var tempPopup = window.webkitNotifications.createHTMLNotification(["http://www.baidu.com/", "http://www.soso.com"][Math.random() >= 0.5 ? 0 : 1]);
//            tempPopup.replaceId = "Meteoric_cry";
//            tempPopup.show();
//        } else {
//            window.webkitNotifications.requestPermission(notify);
//        }
//    }
//}