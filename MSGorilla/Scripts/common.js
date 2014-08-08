// ajax function
function AjaxCall(type, async, apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid) {
    if (isNullOrEmpty(apiurl)) {
        return;
    }
    if (isNullOrEmpty(apidata)) {
        apidata = "";
    }

    $.ajax({
        type: type,
        url: apiurl,
        data: apidata,
        dataType: "json",
        async: async,
        success: function (data) {
            try {
                if (!isNullOrEmpty(successCallback)) {
                    successCallback(data);
                }
            }
            catch (ex) {
                showError(ex);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            try {
                showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message, msgboxid);
                if (!isNullOrEmpty(errorCallback)) {
                    errorCallback(XMLHttpRequest, textStatus, errorThrown);
                }
            }
            catch (ex) {
                showError(ex);
            }
        }
    }).always(function () {
        try {
            if (!isNullOrEmpty(alwaysCallback)) {
                alwaysCallback();
            }
        }
        catch (ex) {
            showError(ex);
        }
    });
}

function AjaxGetAsync(apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid) {
    AjaxCall("GET", true, apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid);
}

function AjaxGetSync(apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid) {
    AjaxCall("GET", false, apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid);
}

function AjaxPostAsync(apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid) {
    AjaxCall("POST", true, apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid);
}

function AjaxPostSync(apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid) {
    AjaxCall("POST", false, apiurl, apidata, successCallback, errorCallback, alwaysCallback, msgboxid);
}


// common function
function scrollTo(itemid) {
    var scroll_offset = $("#" + itemid).offset();
    $("body,html").animate({
        scrollTop: scroll_offset.top - 60
    }, "slow");
}

function isNullOrEmpty(strVal) {
    if (strVal == null || strVal == undefined || strVal == '') {
        return true;
    } else {
        return false;
    }
}

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

function txt2Html(code) {
    code = code.replace(/&/mg, '&#38;');
    code = code.replace(/</mg, '&#60;');
    code = code.replace(/>/mg, '&#62;');
    code = code.replace(/\"/mg, '&#34;');
    code = code.replace(/\t/g, '  ');
    code = code.replace(/\r?\n/g, '<br/>');
    code = code.replace(/ /g, '&nbsp;');

    return code;
}

function html2Txt(code) {
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

function cutCount(count) {
    if (isNullOrEmpty(count)) {
        return 0;
    }

    if (count > 99) {
        return "99+";
    }

    return count;
}

function cutHtml(code, length, atusers, topics, group) {
    if (isNullOrEmpty(code)) {
        return "";
    }

    if (code.length <= length) {
        return encodeHtml(code, atusers, topics, group);
    }
    else {
        return txt2Html(code.substr(0, length - 3) + "...");
    }
}

function encodeId(code) {
    code = code.replace(/@/mg, '_');
    code = code.replace(/\./mg, '_');
    return code;
}

function encodeHtml(code, atusers, topics, group) {
    code = txt2Html(code);

    // autolink http[s]
    var strRegex = "http[s]?://(([0-9a-z]+(\\-)?)*[0-9a-z]+(\\.))*[0-9a-z]+(:[0-9]{1,5})?(/([\\w\\-/\\+\\?%#=\\.:{}]|(&#38;))*)?";
    //var strRegex = "(https?://)?([\\da-z\\.-]+)\\.([a-z\\.]{2,6})([/\\w \\.-]*)*/?";
    var linkre = new RegExp(strRegex, "gi");

    code = code.replace(linkre, function (s) {
        return ('<a href="' + (html2Txt(s)) + '">' + s + '</a>');
    });

    // autolink @user
    if (!isNullOrEmpty(atusers)) {
        for (var i = 0; i < atusers.length; i++) {
            var user = atusers[i];
            var htmluser = txt2Html(user);
            code = code.replace(new RegExp("@" + htmluser, "gm"), ('<a href="/profile/index?user=' + encodeTxt(user) + '">@' + htmluser + '</a>'));
        }

        //strRegex = "@([0-9a-z\\-]+)(\\s|$)";
        //var atre = new RegExp(strRegex, "gi");

        //code = code.replace(atre, function (s1, s2, s3) {
        //    if ($.inArray(s2, atusers) >= 0) {
        //        return encodeTxt('<a href="/profile/index?user=' + s2 + '">@' + s2 + '</a>') + s3;
        //    }
        //    else {
        //        return s1;
        //    }
        //});
    }

    // autolink #topic#
    if (!isNullOrEmpty(topics)) {
        for (var i = 0; i < topics.length; i++) {
            var topic = topics[i];
            var htmltopic = txt2Html(topic);
            code = code.replace(new RegExp("#" + htmltopic + "#", "gm"), ('<a href="/topic/index?topic=' + encodeTxt(topic) + "&group=" + encodeTxt(group) + '">#' + htmltopic + '#</a>'));
        }

        //strRegex = "#(([\\w \\-]+(#{2,})?)*[\\w \\-]+)#(\\s|$)";
        //var topicre = new RegExp(strRegex, "gi");

        //code = code.replace(topicre, function (s1, s2, s3, s4,s5,s6,s7) {
        //    if ($.inArray(s2, topics) >= 0) {
        //        return encodeTxt('<a href="/topic/index?topic=' + s2 + '">#' + s2 + '#</a>') + s5;
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


// chart template
var chart_axis_template = {
    tooltip: {
        trigger: 'axis'
    },
    toolbox: {
        show: true,
        y: 'center',
        orient: 'vertical',
        feature: {
            mark: {
                show: true,
                title: {
                    mark: 'New markline',
                    markUndo: 'Erase markline',
                    markClear: 'Clear all marklines'
                },
            },
            magicType: {
                show: true,
                title: {
                    line: 'Line view',
                    bar: 'Bar view',
                    stack: 'Stack view',
                    tiled: 'Tiled view'
                },
                type: ['line', 'bar', 'stack', 'tiled']
            },
            dataView: {
                show: true,
                title: 'Data view',
                readOnly: false
            },
            restore: {
                show: true,
                title: 'Restore'
            },
            saveAsImage: {
                show: true,
                title: 'Save as image',
                lang: ['Right click and save']
            }
        }
    },
    calculable: true,
    title: {
        text: '',
        subtext: '',
        x: 'center'
    },
    legend: {
        x: 'center',
        y: 'bottom',
        data: []
    },
    xAxis: [
        {
            type: 'category',
            data: []
        }
    ],
    yAxis: [
        {
            type: 'value',
        },
        {
            type: 'value',
        }
    ],
    series: []
};

var chart_scatter_template = {
    tooltip: {
        trigger: 'axis'
    },
    toolbox: {
        show: true,
        y: 'center',
        orient: 'vertical',
        feature: {
            mark: {
                show: true,
                title: {
                    mark: 'New markline',
                    markUndo: 'Erase markline',
                    markClear: 'Clear all marklines'
                },
            },
            dataZoom: {
                show: true,
                title: {
                    dataZoom: 'Zoom in',
                    dataZoomReset: 'Cancel zoom in'
                }
            },
            dataView: {
                show: true,
                title: 'Data view',
                readOnly: false
            },
            restore: {
                show: true,
                title: 'Restore'
            },
            saveAsImage: {
                show: true,
                title: 'Save as image',
                lang: ['Right click and save']
            }
        }
    },
    calculable: true,
    title: {
        text: '',
        subtext: '',
        x: 'center'
    },
    legend: {
        x: 'center',
        y: 'bottom',
        data: []
    },
    xAxis: [
        {
            type: 'value',
            power: 1,
            scale: true,
        }
    ],
    yAxis: [
        {
            type: 'value',
            power: 1,
            scale: true,
        }
    ],
    series: []
};

var chart_pie_template = {
    tooltip: {
        trigger: 'item',
        formatter: "{a} <br/>{b} : {c} ({d}%)"
    },
    toolbox: {
        show: true,
        y: 'center',
        orient: 'vertical',
        feature: {
            mark: {
                show: true,
                title: {
                    mark: 'New markline',
                    markUndo: 'Erase markline',
                    markClear: 'Clear all marklines'
                },
            },
            dataView: {
                show: true,
                title: 'Data view',
                readOnly: false
            },
            restore: {
                show: true,
                title: 'Restore'
            },
            saveAsImage: {
                show: true,
                title: 'Save as image',
                lang: ['Right click and save']
            }
        }
    },
    calculable: true,
    title: {
        text: '',
        subtext: '',
        x: 'center'
    },
    legend: {
        orient: 'vertical',
        x: 'left',
        y: 'center',
        data: []
    },
    series: []
};

function enhanceMessage(schema, mid, msg) {
    var msgdiv = $("#message_" + mid);
    if (msgdiv.length == 0 || isNullOrEmpty(msg)) {
        return;
    }

    try {
        switch (schema) {
            case "chart-free":
                var json = eval("(" + msg + ")");

                msgdiv.height(400);
                var chart = echarts.init(msgdiv[0]);
                chart.setOption(json);
                break;

            case "chart-axis-singleaxis":
                var json = eval("(" + msg + ")");

                var option = chart_axis_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.xAxis[0].data = json.xAxis;
                option.series = json.yAxis;

                msgdiv.height(400);
                var chart = echarts.init(msgdiv[0]);
                chart.setOption(option);
                break;

            case "chart-axis-doubleaxes":
                var json = eval("(" + msg + ")");

                var option = chart_axis_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.xAxis[0].data = json.xAxis;
                option.series = json.yAxis;
                for (var i = 0; i < json.yAxis2.length; i++) {
                    var y2 = json.yAxis2[i];
                    y2.yAxisIndex = 1;
                    option.series.push(y2);
                }

                msgdiv.height(400);
                var chart = echarts.init(msgdiv[0]);
                chart.setOption(option);
                break;

            case "chart-scatter":
                var json = eval("(" + msg + ")");

                var option = chart_scatter_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.series = json.data;

                msgdiv.height(400);
                var chart = echarts.init(msgdiv[0]);
                chart.setOption(option);
                break;

            case "chart-pie":
                var json = eval("(" + msg + ")");

                var option = chart_pie_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.series = json.data;

                msgdiv.height(400);
                var chart = echarts.init(msgdiv[0]);
                chart.setOption(option);
                break;

            default:
                break;
        }
    }
    catch (ex) { }
}


/* show message function */
function showMessage(msg, msgboxid) {
    if (isNullOrEmpty(msgboxid)) {
        $("#loading_message").html(msg);
    } else {
        $("#" + msgboxid).html(msg);
    }
}

function showError(msg, msgboxid) {
    var msghtml = "<div class='alert alert-dismissable alert-warning'><button class='close' type='button' data-dismiss='alert'>×</button><p>" + msg + "</p></div>";
    $("#alert_box").append(msghtml);

    if (isNullOrEmpty(msgboxid)) {
        $("#loading_message").html(msg);
    } else {
        $("#" + msgboxid).html(msg);
    }
}

function showAjaxError(status, error, code, msg, msgboxid) {
    if (code == 2004) // access denied. need login again.
        location.href = "/Error/?message=" + encodeTxt(msg) + "&returnUrl=" + encodeTxt("/Account/Logoff");

    var errormsg = "[" + status + "]" + " " + error + ": " + (isNullOrEmpty(code) ? "" : code) + " " + msg;
    showError(errormsg, msgboxid);
}


/* post function */
function PostMessage() {
    var msgbox = $("#postmessage");
    var message = encodeTxt(msgbox.val().trim());
    if (message.length === 0) {
        return;
    }
    var group = $("#postmessage_groups").val();
    var curgroup = $("#hd_current_group").val();

    var apiurl = "/api/message/postmessage";
    var apidata = "message=" + message + "&group=" + encodeTxt(group) + "&importance=2"; // + "&schemaID=" + "chart-pie"

    $("#btn_post").button('loading');
    AjaxPostAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to post message.");
            }
            else {
                msgbox.val("");

                if (group == curgroup) {
                    // insert the new posted message
                    $("#feedlist").prepend(createFeed(data));
                    // render feed
                    initUserPopover("user_pic_" + data.ID, data.User.Userid);
                    // enhanceMessage(data.SchemaID, data.ID, data.MessageContent);  // user can not post schema data from webpage.
                }
            }
        },
        null,
        function () {
            $("#btn_post").button('reset');
        }
    );
}

function PostReply(user, mid) {
    var replybox = $("#replymessage_" + mid);
    var message = encodeTxt(replybox.val().trim());
    var replyto = replybox.attr("replyto");
    var tousers = [];
    if (message.length === 0) {
        return;
    }

    if (isNullOrEmpty(replyto)) {
        tousers.push(user);
    }
    else {
        tousers = replyto.split(";");
    }

    var apiurl = "/api/reply/postreply";
    var apidata = "message=" + message + "&messageUser=" + encodeTxt(user) + "&messageID=" + encodeTxt(mid);
    for (var i = 0; i < tousers.length; i++) {
        if (!isNullOrEmpty(tousers[i]))
            apidata += "&to=" + encodeTxt(tousers[i]);
    }

    $("#btn_reply_" + mid).button('loading');
    AjaxPostAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to post reply.");
            }
            else {
                replybox.val("");
                $("#replylist_" + mid).prepend(createReply(data));
                // render feed
                initUserPopover("user_pic_" + data.ID, data.User.Userid);
            }
        },
        null,
        function () {
            $("#btn_reply_" + mid).button('reset');
        }
    );
}


/* user infor function */
function LoadMyInfo() {
    if ($("#my_id").length == 0) {
        return;
    }

    var apiurl = "/api/account/me";

    AjaxGetAsync(
        apiurl,
        "",
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to load my info.");
            }
            else {
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

                $("#my_id").html("@" + userid);
                $("#my_name").html(username);
                $("#my_pic").attr("src", picurl);
                $("#my_posts").html(postscount);
                $("#my_following").html(followingcount);
                $("#my_followers").html(followerscount);
            }
        }
    );
}

function LoadUserInfo(user) {
    if ($("#user_id").length == 0) {
        return;
    }

    if (isNullOrEmpty(user)) { // refresh user profile
        user = $("#user_id").html();
        if (!isNullOrEmpty(user)) {
            user = user.substr(1); // remove '@'
        }
    }

    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(user)) {
        apiurl = "/api/account/me";
    }
    else {
        apiurl = "/api/account/user";
        apidata = "userid=" + encodeTxt(user);
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to load user info.");
            }
            else {
                var userid = data.Userid;
                var username = data.DisplayName;
                var picurl = data.PortraitUrl;
                var desp = data.Description;
                var postscount = data.MessageCount;
                var followingcount = data.FollowingsCount;
                var followerscount = data.FollowersCount;
                var isFollowimg = data.IsFollowing;
                if (isNullOrEmpty(picurl)) {
                    picurl = "/Content/Images/default_avatar.jpg";
                }

                $("#user_id").html("@" + userid);
                $("#user_name").html(username);
                $("#user_name").attr("href", "/profile/index?user=" + userid);
                $("#user_pic").attr("src", picurl);
                $("#user_desp").html(desp);
                $("#user_posts").html(postscount);
                $("#user_following").html(followingcount);
                $("#user_followers").html(followerscount);

                setUserFollowBtn("btn_userprofile_follow_" + encodeId(userid), userid, isFollowimg);
            }
        }
    );
}

function setUserFollowBtn(btnid, user, isFollowing) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    if (isFollowing == 0) {
        setFollowBtn(btnid, user, true);
    } else if (isFollowing == 1) {
        setUnfollowBtn(btnid, user, true);
    } else {  // -1: myself
        setEditProfileBtn(btnid, user);
    }
}

function setEditProfileBtn(btnid, user) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Edit profile");
    btn.attr("class", "btn btn-info follow-btn");
    btn.attr("href", "/account/manage");
    btn.show();
}

function setUnfollowBtn(btnid, user, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Following");
    btn.attr("class", "btn btn-success follow-btn");
    if (enabled) {
        btn.attr("onclick", "unfollow('" + btnid + "', '" + user + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "unfollowBtnMouseOver('" + btnid + "');")
    btn.attr("onmouseout", "unfollowBtnMouseOut('" + btnid + "');")
    btn.show();
}

function unfollowBtnMouseOver(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger follow-btn");
    btn.text("- Unfollow");
}

function unfollowBtnMouseOut(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success follow-btn");
    btn.text("Following");
}

function setFollowBtn(btnid, user, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("+ Follow");
    btn.attr("class", "btn btn-primary follow-btn");
    if (enabled) {
        btn.attr("onclick", "follow('" + btnid + "', '" + user + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
    btn.show();
}

function follow(btnid, user) {
    setUnfollowBtn(btnid, user, false);

    var apiurl = "/api/account/follow";
    var apidata = "userid=" + encodeTxt(user);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to follow user.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    setUnfollowBtn(btnid, user, true);
                    // refresh user info 
                    LoadUserInfo();
                    LoadMyInfo();
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}

function unfollow(btnid, user) {
    setFollowBtn(btnid, user, false);

    var apiurl = "/api/account/unfollow";
    var apidata = "userid=" + encodeTxt(user);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to unfollow user.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    setFollowBtn(btnid, user, true);
                    // refresh user info 
                    LoadUserInfo();
                    LoadMyInfo();
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}

function LoadUsers(category, user) {
    var listdiv = $("#userlist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(category))
        apiurl = "/api/account/user";
    else if (category == "following")
        apiurl = "/api/account/followings";
    else if (category == "followers")
        apiurl = "/api/account/followers";
    else {
        showError("Illegal operation.");
        return;
    }
    if (!isNullOrEmpty(user)) {
        apidata = "userid=" + encodeTxt(user);
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.");
            }
            else {
                // clear list
                listdiv.empty();

                // create list
                $.each(data, function (index, item) {
                    listdiv.append(createUserCard(item));
                    setUserFollowBtn("btn_user_follow_" + encodeId(item.Userid), item.Userid, item.IsFollowing);
                })
            }
        }
    );
}

function createUserCard(data) {
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
           + "        <img class='img-rounded' id='user_pic_" + encodeId(userid) + "' src='" + picurl + "' width='100' height='100' />"
           + "      </div>"
           + "      <div>"
           + "        <a class='fullname' id='user_name_" + encodeId(userid) + "' href='/profile/index?user=" + encodeTxt(userid) + "'>" + username + "</a>"
           + "      </div>"
           + "      <div>"
           + "        <span class='username' id='user_id_" + encodeId(userid) + "'>@" + userid + "</span>"
           + "      </div>"
           + "    </div>"
           + "    <div class='user-card-postinfo'>"
           + "      <div class='btn-group btn-group-justified'>"
           + "        <a class='btn btn-default follow-btn' id='btn_user_follow_" + encodeId(userid) + "'>&nbsp;</a>"
           + "      </div>"
           + "    </div>"
           + "  </li>";

    return output;
}

function initUserPopover(eid, user) {
    var options = {
        toggle: 'popover',
        container: 'body',
        placement: 'right',
        html: true,
        trigger: 'focus',
        content: function (e) {
            var output = "";
            var apiurl = "/api/account/user";
            var apidata = "userid=" + encodeTxt(user);

            AjaxGetSync(
                apiurl,
                apidata,
                function (data) {
                    if (isNullOrEmpty(data)) {
                    }
                    else {
                        output = createUserPopover(data);
                    }
                }
            );

            return output;
        }
    };

    $("#" + eid).popover(options);
}

function createUserPopover(data) {
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

    output = "  <div>"
           + "    <div class='user-popover-info'>"
           + "      <div class='user-popover-pic'>"
           + "        <img class='img-rounded' src='" + picurl + "' width='100' height='100' />"
           + "      </div>"
           + "      <div class='user-popover-content'>"
           + "        <div>"
           + "          <a class='fullname' href='/profile/index?user=" + encodeTxt(userid) + "'>" + username + "</a>"
           + "        </div>"
           + "        <div>"
           + "          <span class='username'>@" + userid + "</span>"
           + "        </div>"
           + "        <div>"
           + "          <span class='word-break'>" + desp + "</span>"
           + "        </div>"
           + "      </div>"
           + "    </div>"
           + "    <div class='user-popover-postinfo'>"
           + "      <div class='btn-group btn-group-justified'>"
           + "        <a class='btn btn-link btn-xs' href='/Profile?user=" + encodeTxt(userid) + "'>POSTS <br /><span class='badge'>" + postscount + "</span></a>"
           + "        <a class='btn btn-link btn-xs' href='/Profile/Following?user=" + encodeTxt(userid) + "'>FLWING <br /><span class='badge'>" + followingcount + "</span></a>"
           + "        <a class='btn btn-link btn-xs' href='/Profile/Followers?user=" + encodeTxt(userid) + "'>FLWERS <br /><span class='badge'>" + followerscount + "</span></a>"
           + "      </div>"
           + "    </div>"
           + "    <div class='user-popover-footer'>";

    if (isFollowing == 0) {
        output += "<a class='btn btn-primary follow-btn' id='btn_userpopover_follow_" + encodeId(userid) + "' onclick='follow(\"btn_userpopover_follow_" + encodeId(userid) + "\", \"" + userid + "\");'>+ Follow</a>";
    } else if (isFollowing == 1) {
        output += "<a class='btn btn-success follow-btn' id='btn_userpopover_follow_" + encodeId(userid) + "' onclick='unfollow(\"btn_userpopover_follow_" + encodeId(userid) + "\", \"" + userid + "\");' onmouseover='unfollowBtnMouseOver(\"btn_userpopover_follow_" + encodeId(userid) + "\");' onmouseout='unfollowBtnMouseOut(\"btn_userpopover_follow_" + encodeId(userid) + "\");' >Following</a>";
    } else {  // -1: myself
        output += "<a class='btn btn-info follow-btn' id='btn_userpopover_follow_" + encodeId(userid) + "' href='/account/manage'>Edit profile</a>";
    }

    output += "    </div>"
           + "  </div>";

    return output;
}


// feed function
function LoadMessage(mid) {
    if (isNullOrEmpty(user) || isNullOrEmpty(mid)) {
        showError("Illegal operation.");
        return;
    }

    var listdiv = $("#feedlist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/getdisplaymessage";
    var apidata = "msgID=" + encodeTxt(mid);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showMessage("No content.");
            }
            else {
                // clear list
                listdiv.empty();

                // create feed 
                listdiv.append(createFeed(data, true));

                // render feed
                initUserPopover("user_pic_" + data.ID, data.User.Userid);
                enhanceMessage(data.SchemaID, data.ID, data.MessageContent);
                if ($("#rich_message_" + data.ID).length > 0) {
                    $("#rich_message_" + data.ID).collapse('show');
                    showRichMsg(data.RichMessageID, data.ID);
                }
                if ($("#reply_" + data.ID).length > 0) {
                    $("#reply_" + data.ID).collapse('show');
                    showReplies(data.ID);
                }
            }
        }
    );
}

var feed_filters = [
    "all",
    "latest24hours",
    "latest3days",
    "latest7days",
    "latest1month"
];

var isLoadFeeds = false;

function FilterFeeds(filter) {
    if (isLoadFeeds) return;    // if it is loading, skip this time.

    for (var i = 0; i < feed_filters.length; i++) {
        if (feed_filters[i] == filter) {
            $("#feedfilter_" + filter).addClass("active");
            $("#feedfilter_title").text($("#feedfilter_" + filter).text());
            $("#hd_filter").val(filter);
            LoadFeeds(true);
        } else {
            $("#feedfilter_" + feed_filters[i]).removeClass("active");
        }
    }
}

function LoadFeeds(isReload) {
    if (isLoadFeeds) return;    // if it is loading, skip this time.
    isLoadFeeds = true;

    var listdiv = $("#feedlist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";
    var category = "";
    var id = "";
    var group = "";
    var filter = "";
    var token = "";
    var count = 0;

    // get paras from page
    if ($("#hd_feed_category").length > 0) {
        category = $("#hd_feed_category").val();
    }
    if ($("#hd_feed_id").length > 0) {
        id = $("#hd_feed_id").val();
    }
    if ($("#hd_current_group").length > 0) {
        group = $("#hd_current_group").val();
    }
    if ($("#hd_filter").length > 0) {
        filter = $("#hd_filter").val();
    }
    if ($("#hd_token").length > 0) {
        token = $("#hd_token").val();
    }
    if ($("#hd_newcount").length > 0) {
        count = $("#hd_newcount").val();
    }

    if (isReload == true) { // reload
        token = "";
        $("#hd_token").val(token);
        count = getNotificationCount(category, id, group); // homeline not supported yet.
        $("#hd_newcount").val(count);

        listdiv.empty();
        listdiv.append("<li id='loading_message' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");
    }
    else { // load more
        if (token == "nomore") {
            // already no more feeds, don't load any more
            isLoadFeeds = false;
            return;
        }
    }

    // get
    if (category == "homeline") {
        apiurl = "/api/message/homeline";
        apidata = "userid=" + encodeTxt(id);
    }
    else if (category == "atline") {
        apiurl = "/api/message/atline";
        apidata = "userid=" + encodeTxt(id);
    }
    else if (category == "ownerline") {
        apiurl = "/api/message/ownerline";
        apidata = "userid=" + encodeTxt(id);
    }
    else if (category == "replyline") {
        apiurl = "/api/reply/getmyreply";
    }
    else if (category == "publicsquareline")
        apiurl = "/api/message/publicsquareline";
    else if (category == "userline") {
        apiurl = "/api/message/userline";
        apidata = "userid=" + encodeTxt(id);
    }
    else if (category == "topicline") {
        apiurl = "/api/message/topicline";
        if (isNullOrEmpty(id)) {
            showError("Illegal operation.");
            isLoadFeeds = false;
            return;
        }
        apidata = "topic=" + encodeTxt(id);
    }
    else if (category == "message") {
        apiurl = "/api/message/topicline";
        if (isNullOrEmpty(id)) {
            showError("Illegal operation.");
            isLoadFeeds = false;
            return;
        }
        apidata = "topic=" + encodeTxt(id);
    }
    else {
        showError("Illegal operation.");
        isLoadFeeds = false;
        return;
    }

    if (!isNullOrEmpty(group)) {   // set group value
        if (isNullOrEmpty(apidata)) {
            apidata = "group=" + group;
        }
        else {
            apidata += "&group=" + group;
        }
    }

    if (!isNullOrEmpty(filter)) {   // set filter value
        if (isNullOrEmpty(apidata)) {
            apidata = "filter=" + filter;
        }
        else {
            apidata += "&filter=" + filter;
        }
    }

    if (!isNullOrEmpty(token)) { // set token value
        if (isNullOrEmpty(apidata)) {
            apidata = "token=" + token;
        }
        else {
            apidata += "&token=" + token;
        }
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            var nexttoken = null;
            if (isNullOrEmpty(data) || data.message.length == 0) {
                showMessage("No content.");
            }
            else {
                nexttoken = data.continuationToken;
                data = data.message

                // clear feed list
                if (isNullOrEmpty(token)) {
                    // clear loading message at the first time 
                    listdiv.empty();
                }

                // create feed list
                $.each(data, function (index, item) {
                    listdiv.append(createFeed(item));

                    // render feed
                    if (count-- > 0) {
                        $("#feed_" + item.ID).addClass('new-notification');
                    }
                    initUserPopover("user_pic_" + item.ID, item.User.Userid);
                    enhanceMessage(item.SchemaID, item.ID, item.MessageContent);
                    // if msg is empty, show richmsg instead
                    if (isNullOrEmpty(item.MessageContent) && $("#rich_message_" + item.ID).length > 0) {
                        $("#rich_message_" + item.ID).collapse('show');
                        showRichMsg(item.RichMessageID, item.ID);
                    }
                })
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
        function (XMLHttpRequest, textStatus, errorThrown) {
            $("#lbl_seemore").html("");
            $("#hd_token").val("nomore");
            $("#hd_newcount").val(0);
            isLoadFeeds = false;
        }
    );
}

var importence_tags = [
    "<span class='label label-danger feed-importence'>Important</span>", // 0
    "<span class='label label-info feed-importence'>Info</span>",
    "<span class='label label-primary feed-importence'>Primary</span>",
    "<span class='label label-success feed-importence'>Success</span>",
    "<span class='label label-warning feed-importence'>Warning</span>",
    "<span class='label label-danger feed-importence'>Danger</span>",
    "<span class='label label-default feed-importence'>Default</span>",
];

function createFeed(data, hideOpenBtn) {
    var output = "";
    var showevents = false;
    var type = data.Type;
    var id = data.ID;
    var sid = data.SchemaID;
    var eid = data.EventID;
    var msg = data.MessageContent;
    var richmsgid = data.RichMessageID;
    var posttime = data.PostTime;
    var user = data.User.Userid;
    var username = data.User.DisplayName;
    var picurl = data.User.PortraitUrl;
    var userdesp = data.User.Description;
    var owners = data.Owner;
    var atusers = data.AtUser;
    var topics = data.TopicName;
    var attach = data.Attachment;
    var importence = data.Importance;
    var group = data.Group;

    if (type == "reply") {
        var mid = data.MessageID;
        var muser = data.MessageUser;
    } else {
        var mid = id;
        var muser = user;
    }
    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    //output += "<ul class='list-group'>";
    output += "  <li id='feed_" + id + "' class='list-group-item'>";
    output += "    <div>"

    // user pic
    output += "      <div class='feed-pic'>";
    output += "        <button class='btn btn-img' type='button' id='user_pic_" + id + "' href='javascript:void(0);'>";
    output += "          <img class='img-rounded' src='" + picurl + "' width='100' height='100'/>";
    output += "        </button>";
    output += "      </div>";

    // importance
    if (importence == 0) {
        output += importence_tags[importence] + "&nbsp;";
    }

    output += "      <div class='feed-content'>";

    // user name and post time
    output += "        <div class='newpost-header'>";
    output += "          <a id='username_" + id + "' class='fullname' href='/profile/index?user=" + encodeTxt(user) + "'>" + username + "</a>&nbsp;";
    output += "          <span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>";
    output += "        </div>";

    // owners
    if (!isNullOrEmpty(owners)) {
        output += "    <div class='newpost-input'>";
        output += "      <span class=''>Owned by: </span>";
        for (var i = 0; i < owners.length; i++) {
            output += "  <a href='/profile/index?user=" + encodeTxt(owners[i]) + "'>@" + owners[i] + "</a>&nbsp;";
        }
        output += "    </div>";
    }

    // message
    if (!isNullOrEmpty(msg)) {
        output += "    <div id='message_" + id + "' class='newpost-input'>" + encodeHtml(msg, atusers, topics, group) + "</div>";
    }

    // richmsg
    if (!isNullOrEmpty(richmsgid)) {
        output += "    <div id='rich_message_" + id + "' class='newpost-richinput panel-collapse collapse out'>";
        output += "      <hr/>";
        output += "      <div id='rich_message_inner_" + id + "' class='clickable' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='showRichMsg(\"" + richmsgid + "\", \"" + id + "\", event);'></div>";
        output += "    </div>";
    }

    // attachment
    if (!isNullOrEmpty(attach)) {
        output += "    <div class='newpost-input'><span class=''>Attachments: </span>";
        for (var i = 0; i < attach.length; i++) {
            output += "  <a class='btn btn-link btn-xs' onclick='showAttach(\"" + attach[i].AttachmentID + "\", \"" + attach[i].Filetype + "\", \"" + id + "\");' >" + attach[i].Filename + " (" + attach[i].Filesize + ")</a>&nbsp;";
        }
        output += "    </div>";
        output += "    <div id='attachment_" + id + "' class='newpost-input' style='display:none;'></div>";
    }

    // footer btns
    output += "        <div class='newpost-footer'>";
    // open btn
    if (hideOpenBtn != true) {
        output += "      <a id='btn_open_msg_" + id + "' class='btn btn-link btn-sm' target='_blank' href='/Message/Index?user=" + encodeTxt(muser) + "&msgID=" + encodeTxt(mid) + "'>Open</a>";
    }
    // richmsg btn
    if (!isNullOrEmpty(richmsgid)) {
        output += "      <button id='btn_showrichmsg_" + id + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='showRichMsg(\"" + richmsgid + "\", \"" + id + "\");'>More</button>";
    }
    // replies btn
    if (type == "message") {
        output += "      <button id='btn_showreply_" + id + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + id + "' href='#reply_" + id + "' onclick='showReplies(\"" + id + "\");'>Replies</button>";
    }
    output += "        </div>";

    output += "      </div>";
    output += "    </div>";

    if (type == "message") {
        // whom reply to
        var replyto = user + ";";   // default reply to poster.
        var replytostring = " @" + user;   // default reply to poster.
        if (!isNullOrEmpty(owners)) {   // reply to owners if owners exist.
            replyto = "";
            replytostring = "";
            for (var i = 0; i < owners.length; i++) {
                replyto += owners[i] + ";";
                replytostring += " @" + owners[i];
            }
        }

        // reply div
        output += "    <div id='reply_" + id + "' class='panel-collapse collapse out '>";
        output += "      <div class='replies-container'>";

        // reply box
        output += "        <div class='input-group'>";
        //output += "        <span class='input-group-addon'>Comment</span>";
        output += "          <input class='form-control' type='text' id='replymessage_" + id + "' replyto='" + replyto + "' placeholder='Reply to" + replytostring + "'>";
        output += "          <span class='input-group-btn'>";
        output += "            <button class='btn btn-primary' type='button' id='btn_reply_" + id + "' data-loading-text='Replying...' onclick='PostReply(\"" + user + "\", \"" + id + "\");'>Reply</button>";
        output += "          </span>";
        output += "        </div>";

        // replies
        output += "        <div class='replies-feeds'>";
        output += "          <ul id='replylist_" + id + "' class='list-group'></ul>";
        output += "          <ul id='replylist_older_" + id + "' class='hidden'></ul>";
        output += "          <div class='txtaln-c'><a id='show_more_replies_" + id + "' class='btn btn-link btn-xs' style='display:none' onclick='showMoreReplies(\"" + id + "\");'></a></div>";
        output += "        </div>";

        output += "      </div>";
        output += "    </div>";
    }

    output += "  </li>";
    //output += "</ul>";

    return output;
}

function showAttach(aid, type, mid) {
    var attadiv = $("#attachment_" + mid);
    if (attadiv.length == 0) {
        return;
    }

    var apiurl = "/api/attachment/download?attachmentid=" + aid;

    attadiv.empty();
    switch (type) {
        default:
            window.open(apiurl);
            break;
    }
}

function showRichMsg(rmid, mid, evt) {
    var richmsgdiv = $("#rich_message_" + mid);
    if (richmsgdiv.length == 0) {
        return;
    }

    var richmsginner = $("#rich_message_inner_" + mid);
    var showbtn = $("#btn_showrichmsg_" + mid);
    var show = richmsgdiv.hasClass("in");

    if (show == false) {
        if (isNullOrEmpty(richmsginner.html())) {
            // add loading pic
            richmsginner.append("<div class='txtaln-c'><span class='spinner-loading'></span> Loading...</div>");

            // load rich once
            var apiurl = "/api/message/getrichmessage";
            var apidata = "richMsgID=" + encodeTxt(rmid);

            AjaxGetAsync(
                apiurl,
                apidata,
                function (data) {
                    if (isNullOrEmpty(data)) {
                        data = "No content.";
                    }

                    // clear 
                    richmsginner.empty();
                    // create rich message
                    richmsginner.append(data);

                    // show rich
                    showbtn.text("Less");
                },
                null,
                null,
                "rich_message_inner_" + mid
            );
        }
        else {
            // show rich
            showbtn.text("Less");
        }
    }
    else {
        if (!isNullOrEmpty(evt)) {
            var e = window.event || evt;
            var ele = e.srcElement || e.target;

            while (ele) {
                if (!isNullOrEmpty(ele.href)) {
                    if (window.event) {
                        e.cancelBubble = true;
                    }
                    else {
                        //e.preventDefault();
                        e.stopPropagation();
                    }
                    return;
                }
                ele = ele.parentElement;
            }
        }
        // clear rich
        showbtn.text("More");
        scrollTo("feed_" + mid);
    }
}

function showMessageModel(mid, user) {
    if (isNullOrEmpty(user) || isNullOrEmpty(mid)) {
        return;
    }

    var message_div = $("#message_div");
    if (message_div.length == 0) {
        return;
    }

    message_div.empty();
    message_div.append("<li id='loading_message_messagemodel' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");
    $("#MessageModal").modal();

    var apiurl = "/api/message/getdisplaymessage";
    var apidata = "msgID=" + encodeTxt(mid);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showMessage("No content.", "loading_message_messagemodel");
            }
            else {
                // clear
                message_div.empty();

                // create message
                message_div.append(createFeed(data));

                // render feed
                initUserPopover("user_pic_" + data.ID, data.User.Userid);
                enhanceMessage(data.SchemaID, data.ID, data.MessageContent);
                if ($("#rich_message_" + data.ID).length > 0) {
                    $("#rich_message_" + data.ID).collapse('show');
                    showRichMsg(data.RichMessageID, data.ID);
                }
                if ($("#reply_" + data.ID).length > 0) {
                    $("#reply_" + data.ID).collapse('show');
                    showReplies(data.ID);
                }
            }
        },
        null,
        null,
        "loading_message_messagemodel"
    );
}


// reply function
function showReplies(mid) {
    var replydiv = $("#reply_" + mid);
    if (replydiv.length == 0) {
        return;
    }

    var showbtn = $("#btn_showreply_" + mid);
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
    if (replydiv.length == 0) {
        return;
    }
    var replyolderdiv = $("#replylist_older_" + mid);
    var showmorebtn = $("#show_more_replies_" + mid);

    replydiv.empty();
    replyolderdiv.empty();
    replydiv.append("<li class='center-block txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    var newcount = 2;
    var totlecount = 0;
    var apiurl = "/api/message/getmessagereply";
    var apidata = "msgID=" + encodeTxt(mid);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            // clear list
            replydiv.empty();

            if (isNullOrEmpty(data) || data.length == 0) {
            }
            else {
                // create reply list
                $.each(data, function (index, item) {
                    if (index < newcount) {
                        replydiv.append(createReply(item));
                    }
                    else {
                        replyolderdiv.append(createReply(item));
                    }
                    totlecount++;

                    // render feed
                    initUserPopover("user_pic_" + item.ID, item.User.Userid);
                    // if msg is empty, show richmsg instead
                    if (isNullOrEmpty(item.MessageContent) && $("#rich_message_" + item.ID).length > 0) {
                        $("#rich_message_" + item.ID).collapse('show');
                        showRichMsg(item.RichMessageID, item.ID);
                    }
                })

                if (totlecount > newcount) {
                    var morecount = totlecount - newcount;
                    if (morecount > 1)
                        showmorebtn.text("Show " + morecount + " older replies.");
                    else
                        showmorebtn.text("Show " + morecount + " older reply.");
                    showmorebtn.show();
                }
            }
        },
        function (XMLHttpRequest, textStatus, errorThrown) {
            replydiv.empty();
        }
    );
}

function showMoreReplies(mid) {
    var replydiv = $("#replylist_" + mid);
    if (replydiv.length == 0) {
        return;
    }
    var replyolderdiv = $("#replylist_older_" + mid);
    var showmorebtn = $("#show_more_replies_" + mid);

    // move replies from older div to reply div
    replydiv.append(replyolderdiv.children());
    showmorebtn.hide();
}

function createReply(data) {
    var output = "";
    var type = data.Type;
    var id = data.ID;
    var sid = data.SchemaID;
    var eid = data.EventID;
    var msg = data.MessageContent;
    var richmsgid = data.RichMessageID;
    var posttime = data.PostTime;
    var user = data.User.Userid;
    var username = data.User.DisplayName;
    var picurl = data.User.PortraitUrl;
    var userdesp = data.User.Description;
    var owners = data.Owner;
    var atusers = data.AtUser;
    var topics = data.TopicName;
    var attach = data.Attachment;
    var importence = data.Importance;
    var group = data.Group;
    var mid = data.MessageID;
    var muser = data.MessageUser;
    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    output += "<li id='feed_" + id + "' class='list-group-item'>";
    output += "  <div>"

    // user pic
    output += "    <div class='reply-pic'>";
    output += "      <button class='btn btn-img' type='button' id='user_pic_" + id + "' href='javascript:void(0);'>";
    output += "        <img class='img-rounded' src='" + picurl + "' width='50' height='50' />";
    output += "      </button>";
    output += "    </div>";

    // importance
    if (importence == 0) {
        output += importence_tags[importence] + "&nbsp;";
    }

    output += "    <div class='reply-content'>";

    // user name and post time
    output += "      <div class='reply-header'>"
    output += "        <a id='username_" + id + "' class='fullname' href='/profile/index?user=" + encodeTxt(user) + "'>" + username + "</a>&nbsp;";
    output += "        <span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>";
    output += "      </div>";

    // owners
    if (!isNullOrEmpty(owners)) {
        output += "  <div class='reply-input'>";
        output += "    <span class=''>Owned by: </span>";
        for (var i = 0; i < owners.length; i++) {
            output += "<a href='/profile/index?user=" + encodeTxt(owners[i]) + "'>@" + owners[i] + "</a>&nbsp;";
        }
        output += "  </div>";
    }

    // message
    if (!isNullOrEmpty(msg)) {
        output += "  <div id='message_" + id + "' class='reply-input'>" + encodeHtml(msg, atusers, topics, group) + "</div>";
    }

    // richmsg
    if (!isNullOrEmpty(richmsgid)) {
        output += "  <div id='rich_message_" + id + "' class='reply-richinput panel-collapse collapse out'>";
        output += "    <hr/>";
        output += "    <div id='rich_message_inner_" + id + "' class='clickable' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='showRichMsg(\"" + richmsgid + "\", \"" + id + "\", event);'></div>";
        output += "  </div>";
    }

    // attachment
    if (!isNullOrEmpty(attach)) {
        output += "  <div class='reply-input'><span class=''>Attachments: </span>";
        for (var i = 0; i < attach.length; i++) {
            output += "<a class='btn btn-link btn-xs' onclick='showAttach(\"" + attach[i].AttachmentID + "\", \"" + attach[i].Filetype + "\", \"" + id + "\");' >" + attach[i].Filename + " (" + attach[i].Filesize + ")</a>&nbsp;";
        }
        output += "  </div>";
        output += "  <div id='attachment_" + id + "' class='reply-input' style='display:none;'></div>";
    }

    // footer btns
    output += "      <div class='reply-footer'>";
    // richmsg btn
    if (!isNullOrEmpty(richmsgid)) {
        output += "    <button id='btn_showrichmsg_" + id + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='showRichMsg(\"" + richmsgid + "\", \"" + id + "\");'>More</button>";
    }
    // reply to btn
    output += "        <button id='btn_replyto_" + id + "' class='btn btn-link btn-sm' type='button'  onclick='setReplyTo(\"" + mid + "\", \"" + user + "\");'>Reply to</button>";
    output += "      </div>";

    output += "    </div>";

    output += "  </div>";
    output += "</li>";

    return output;
}

function setReplyTo(mid, user) {
    var replybox = $("#replymessage_" + mid);
    if (replybox.length == 0) {
        return;
    }

    replybox.val(replybox.val() + " @" + user + " ");
}


// search function
function SearchTopic(keyword) {
    var listdiv = $("#topiclist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";

    if (isNullOrEmpty(keyword)) {
        showMessage("No keyword.", "loading_message_searchtopic");
        return;
        //apiurl = "/api/topic/getalltopic";
    }
    else {
        apiurl = "/api/topic/searchtopic";
        apidata = "keyword=" + encodeTxt(keyword);
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.", "loading_message_searchtopic");
            }
            else {
                // clear list
                listdiv.empty();

                // create feed 
                $.each(data, function (index, item) {
                    var topicid = item.Id;
                    var topicname = item.Name;
                    var topicdesp = item.Description;
                    var topiccount = item.MsgCount;
                    var isliked = item.IsLiked;
                    var group = item.GroupID;

                    listdiv.append(createTopic(topicid, topicname, topicdesp, topiccount, group));
                    setTopicLikeBtn("btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        },
        null,
        null,
        "loading_message_searchtopic"
    );
}

function SearchUser(keyword) {
    var listdiv = $("#userlist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";

    if (isNullOrEmpty(keyword)) {
        showMessage("No keyword.", "loading_message_searchuser");
        return;
        //apiurl = "/api/account/user";
    }
    else {
        apiurl = "/api/account/searchuser";
        apidata = "keyword=" + encodeTxt(keyword);
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.", "loading_message_searchuser");
            }
            else {
                // clear list
                listdiv.empty();

                // create user list
                $.each(data, function (index, item) {
                    listdiv.append(createUserCard(item));
                    setUserFollowBtn("btn_user_follow_" + encodeId(item.Userid), item.Userid, item.IsFollowing);
                })
            }
        },
        null,
        null,
        "loading_message_searchuser"
    );
}

function SearchMessage(keyword) {
    var searchid_ele = $("#hd_result_id");
    if (searchid_ele.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";

    if (isNullOrEmpty(keyword)) {
        showMessage("No keyword.", "loading_message_searchpost");
        return;
        //apiurl = "/api/topic/getalltopic";
    }
    else if (keyword.length < 4) {
        showMessage("Too short keyword.", "loading_message_searchpost");
        return;
    }
    else {
        apiurl = "/api/message/searchmessage";
        apidata = "keyword=" + encodeTxt(keyword);
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                searchid_ele.val("");
                $("#hd_token").val("");
                $("#searchpost_message").html("");

                showMessage("No content.", "loading_message_searchpost");
            }
            else {
                var resultid = data.ResultId;
                var searchdate = data.SearchDate;
                var searchtime = data.TakenTime;
                var resultcount = data.ResultsCount;

                searchid_ele.val(resultid);
                $("#hd_token").val("");
                $("#searchpost_message").html("- found " + resultcount + " results in " + searchtime + " second(s) at " + DateFormat(searchdate));

                LoadSearchMessageResults(true);
            }
        },
        null,
        null,
        "loading_message_searchpost"
    );
}

function LoadSearchMessageResults(isReload) {
    if (isLoadFeeds) return;    // if it is loading, skip this time.
    isLoadFeeds = true;

    var listdiv = $("#feedlist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";
    var resultid = "";
    var token = "";
    var group = "";
    var filter = "";

    // get paras from page
    if ($("#hd_result_id").length > 0) {
        resultid = $("#hd_result_id").val();
    }
    if ($("#hd_current_group").length > 0) {
        group = $("#hd_current_group").val();
    }
    if ($("#hd_filter").length > 0) {
        filter = $("#hd_filter").val();
    }
    if ($("#hd_token").length > 0) {
        token = $("#hd_token").val();
    }

    // no result id , than do nothing.
    if (isNullOrEmpty(resultid)) {
        isLoadFeeds = false;
        return;
    }

    if (isReload == true) { // reload
        token = "";
        $("#hd_token").val(token);

        listdiv.empty();
        listdiv.append("<li id='loading_message' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");
    }
    else { // load more
        if (token == "nomore") {
            // already no more feeds, don't load any more
            isLoadFeeds = false;
            return;
        }
    }

    // get
    apiurl = "/api/message/searchmessageresults";
    apidata = "searchId=" + encodeTxt(resultid);

    if (!isNullOrEmpty(group)) {   // set group value
        if (isNullOrEmpty(apidata)) {
            apidata = "group=" + group;
        }
        else {
            apidata += "&group=" + group;
        }
    }

    if (!isNullOrEmpty(filter)) {   // set filter value
        if (isNullOrEmpty(apidata)) {
            apidata = "filter=" + filter;
        }
        else {
            apidata += "&filter=" + filter;
        }
    }

    if (!isNullOrEmpty(token)) { // set token value
        if (isNullOrEmpty(apidata)) {
            apidata = "token=" + token;
        }
        else {
            apidata += "&token=" + token;
        }
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            var nexttoken = null;
            if (isNullOrEmpty(data) || data.message.length == 0) {
                showMessage("No content.", "loading_message_searchpost");
            }
            else {
                nexttoken = data.continuationToken;
                data = data.message

                // clear feed list
                if (isNullOrEmpty(token)) {
                    // clear loading message at the first time 
                    listdiv.empty();
                }

                // create feed list
                $.each(data, function (index, item) {
                    listdiv.append(createFeed(item));

                    // render feed
                    initUserPopover("user_pic_" + item.ID, item.User.Userid);
                    enhanceMessage(item.SchemaID, item.ID, item.MessageContent);
                    // if msg is empty, show richmsg instead
                    if (isNullOrEmpty(item.MessageContent) && $("#rich_message_" + item.ID).length > 0) {
                        $("#rich_message_" + item.ID).collapse('show');
                        showRichMsg(item.RichMessageID, item.ID);
                    }
                })
            }

            if (isNullOrEmpty(nexttoken)) {
                $("#lbl_seemore").html("");
                $("#hd_token").val("nomore");
            }
            else {
                $("#lbl_seemore").html("<span class='spinner-loading'></span> Loading more...");
                $("#hd_token").val(nexttoken);
            }

            isLoadFeeds = false;
        },
        function (XMLHttpRequest, textStatus, errorThrown) {
            $("#lbl_seemore").html("");
            $("#hd_token").val("nomore");

            isLoadFeeds = false;
        },
        null,
        "loading_message_searchpost"
    );
}


// notification count function and shortcut function
function getNotificationCount(category, id, group) {
    if (category != "atline"
        && category != "ownerline"
        && category != "replyline"
        // && category != "homeline" // homeline not supported yet.
        && category != "topicline"
        ) {
        return 0;
    }

    var count = 0;

    if (category == "topicline") {
        var apiurl = "/api/topic/getmyfavouritetopicunreadcount";
        var apidata = "topic=" + encodeTxt(id) + "&groupID=" + encodeTxt(group);

        AjaxGetSync(
            apiurl,
            apidata,
            function (data) {
                if (isNullOrEmpty(data)) {
                    count = 0;
                }
                else {
                    count = data;
                }
            },
            null,
            null,
            "no_message_box"
        );
    }
    else {
        var apiurl = "/api/account/getnotificationcount";
        var apidata = "";

        AjaxGetSync(
            apiurl,
            apidata,
            function (data) {
                if (isNullOrEmpty(data)) {
                    count = 0;
                }
                else {
                    var userid = data.Userid;
                    var homelineCount = data.UnreadHomelineMsgCount;
                    var ownerlineCount = data.UnreadOwnerlineMsgCount;
                    var atlineCount = data.UnreadAtlineMsgCount;
                    var replyCount = data.UnreadReplyCount;
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
                            count = 0;
                            break;
                    }
                }
            },
            null,
            null,
            "no_message_box"
        );
    }

    return count;
}

function UpdateNotificationCountTimer() {
    UpdateNotificationCount();
    var timer = $.timer(UpdateNotificationCount, 60000, true);
}

function UpdateNotificationCount() {
    if ($("#shortcut_homeline_count").length == 0
        && $("#notification_navbar_reply_count").length == 0
        && $("#nav_home_count").length == 0) {
        return;
    }

    var apiurl = "/api/account/getnotificationcount";
    var apidata = "";

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                // do nothing
            }
            else {
                var userid = data.Userid;
                var homelineCount = data.UnreadHomelineMsgCount;
                var ownerlineCount = data.UnreadOwnerlineMsgCount;
                var atlineCount = data.UnreadAtlineMsgCount;
                var replyCount = data.UnreadReplyCount;
                var notificationCount = ownerlineCount + replyCount + atlineCount;

                // shortcut panel count
                $("#shortcut_homeline_count").html(homelineCount);
                $("#shortcut_reply_count").html(replyCount);
                $("#shortcut_atline_count").html(atlineCount);
                $("#shortcut_ownerline_count").html(ownerlineCount);
                $("#shortcut_notification_count").html(notificationCount);

                // notification navbar count
                $("#notification_navbar_reply_count").html(replyCount);
                $("#notification_navbar_atline_count").html(atlineCount);
                $("#notification_navbar_ownerline_count").html(ownerlineCount);

                // menu navbar count
                if (homelineCount > 0)
                    $("#nav_home_count").html(homelineCount);
                else
                    $("#nav_home_count").html("");

                if (notificationCount > 0)
                    $("#nav_notification_count").html(notificationCount);
                else
                    $("#nav_notification_count").html("");

                // chrome desktop notification (doesn't work)
                // notify(homelineCount, atlineCount, ownerlineCount, replyCount);
            }
        },
        null,
        null,
        "no_message_box"
    );
}

function LoadHotTopics() {
    var listdiv = $("#topic_collapse");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/topic/hottopics";
    var apidata = "";

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
            }
            else {
                // clear list
                listdiv.empty();

                // create list
                $.each(data, function (index, item) {
                    var output = "";
                    var topicid = item.Id;
                    var topicname = item.Name;
                    var topicdesp = item.Description;
                    var topiccount = item.MsgCount;
                    var isliked = item.IsLiked;
                    var group = item.GroupID;

                    output = "<li class='sub-list-group-item'>"
                            + "  <a class='btn btn-default like-btn' id='shortcut_btn_topic_like_" + topicid + "' style='display:none'>&nbsp;</a>"
                            + "  <span class='badge'>" + topiccount + "</span>"
                            + "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "&group=" + encodeTxt(group) + "'>#" + topicname + "#</a>"
                            + "</li>"
                    listdiv.append(output);
                    setTopicLikeBtn("shortcut_btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        },
        null,
        null,
        "no_message_box"
    );
}

function LoadMyFavoriteTopics() {
    var listdiv = $("#favorite_collapse");
    var navlist = $("#notification_favorite_navbar");
    if (listdiv.length == 0 && navlist.length == 0) {
        return;
    }

    var apiurl = "/api/topic/getmyfavouritetopic";
    var apidata = "";

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
            }
            else {
                // clear list
                listdiv.empty();
                navlist.empty();

                $.each(data, function (index, item) {
                    var output = "";
                    var userid = item.userid;
                    var topicid = item.topicID;
                    var unreadcount = item.UnreadMsgCount;
                    var topicname = item.topicName;
                    var topicdesp = item.topicDescription;
                    var topiccount = item.topicMsgCount;
                    var group = item.GroupID;

                    // shortcut panel
                    if (listdiv.length > 0) {
                        output += "<li class='sub-list-group-item'>";
                        output += "  <span class='badge'>" + unreadcount + "</span>";
                        output += "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "&group=" + encodeTxt(group) + "'>#" + topicname + "#</a>";
                        output += "</li>";
                        listdiv.append(output);
                    }

                    // notification navbar 
                    if (unreadcount > 0 && navlist.length > 0) {
                        output = "<a class='btn btn-link btn-xs' href='/topic/index?topic=" + encodeTxt(topicname) + "'>#" + topicname + "# <span class='badge'>" + unreadcount + "</span></a> ";
                        navlist.append(output);
                    }
                })
            }
        },
        null,
        null,
        "no_message_box"
    );
}

function LoadMyCategories() {
    var listdiv = $("#category_collapse");
    var navlist = $("#notification_category_navbar");
    if (listdiv.length == 0 && navlist.length == 0) {
        return;
    }

    var apiurl = "/api/category/getmycategory";
    var apidata = "";

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
            }
            else {
                // clear list
                listdiv.empty();
                navlist.empty();

                var categories = {};
                var totalcategorycount = 0;

                $.each(data, function (index, item) {
                    var id = item.ID;
                    var name = item.Name;
                    var groupid = item.GroupID;
                    var description = item.Description;
                    var creater = item.Creater;
                    var createtimestamp = item.CreateTimestamp;
                    var eventcount = item.EventCount;

                    if (isNullOrEmpty(categories[name])) {
                        categories[name] = eventcount;
                    }
                    else {
                        categories[name] += eventcount;
                    }
                })

                for (var name in categories) {
                    var output = "";
                    // shortcut panel
                    if (listdiv.length > 0) {
                        output += "<li class='sub-list-group-item'>";
                        output += "  <span class='badge'>" + categories[name] + "</span>";
                        output += "  <a href='/category/index?category=" + encodeTxt(name) + "'>" + name + "</a>";
                        output += "</li>";
                        listdiv.append(output);
                    }

                    // notification navbar 
                    if (navlist.length > 0) {
                        output = "<a class='btn btn-link btn-xs' href='/category/index?category=" + encodeTxt(name) + "'>" + name + " <span class='badge'>" + categories[name] + "</span></a> ";
                        navlist.append(output);
                    }

                    totalcategorycount += categories[name];
                }

                $("#shortcut_category_count").html(totalcategorycount);
            }
        },
        null,
        null,
        "no_message_box"
    );
}

//function notify(homelineCount, atlineCount, ownerlineCount, replyCount) {
//    try {
//        if (isNullOrEmpty(chrome)) {
//            return;
//        }

//        //chrome.notification.getPermissionLevel(function (level) {
//        //    if (level == "granted") {
//        var notifitems = [];

//        if (atlineCount > 0) {
//            notifitems.push({
//                title: "Mentions: ", message: atlineCount
//            });
//        }
//        if (ownerlineCount > 0) {
//            notifitems.push({
//                title: "Owned : ", message: ownerlineCount
//            });
//        }
//        if (replyCount > 0) {
//            notifitems.push({
//                title: "Replies : ", message: replyCount
//            });
//        }

//        var opt = {
//            type: "list",
//            title: "Notifications",
//            message: "You have new unread messages.",
//            iconUrl: "/Content/Images/default_avatar.jpg",
//            items: notifitems
//        };

//        if (notifitems.length > 0) {
//            chrome.notifications.create('chrome_notification', opt, function (id) {
//            });
//        }
//        //    }
//        //    else if (level == "denied") {
//        //        return;
//        //    }
//        //});
//    }
//    catch (e) {
//    }
//}

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


// topic function
function LoadUserFavoriteTopics(user) {
    var listdiv = $("#topiclist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/topic/getuserfavouritetopic";
    var apidata = "userid=" + encodeTxt(user);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.");
            }
            else {
                // clear list
                listdiv.empty();

                // create list
                $.each(data, function (index, item) {
                    var topicid = item.topicID;
                    var topicname = item.topicName;
                    var topicdesp = item.topicDescription;
                    var topiccount = item.topicMsgCount;
                    var isliked = item.IsLiked;
                    var group = item.GroupID;

                    listdiv.append(createTopic(topicid, topicname, topicdesp, topiccount, group));
                    setTopicLikeBtn("btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        }
    );
}

function setTopicLikeBtn(btnid, topicid, isLiked) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    if (isLiked == true) {
        setUnlikeBtn(btnid, topicid, true);
    }
    else if (isLiked == false) {
        setLikeBtn(btnid, topicid, true);
    }
    else {
        // do nothing.
    }
}

function setUnlikeBtn(btnid, topicid, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Liked");
    btn.attr("class", "btn btn-success like-btn");
    if (enabled) {
        btn.attr("onclick", "unlike('" + btnid + "', '" + topicid + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "unlikeBtnMouseOver('" + btnid + "');")
    btn.attr("onmouseout", "unlikeBtnMouseOut('" + btnid + "');")
    btn.show();
}

function unlikeBtnMouseOver(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger like-btn");
    btn.text("- Unlike");
}

function unlikeBtnMouseOut(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success like-btn");
    btn.text("Liked");
}

function setLikeBtn(btnid, topicid, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("+ Like");
    btn.attr("class", "btn btn-primary like-btn");
    if (enabled) {
        btn.attr("onclick", "like('" + btnid + "', '" + topicid + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
    btn.show();
}

function like(btnid, topicid) {
    setUnlikeBtn(btnid, topicid, false);

    var apiurl = "/api/topic/addfavouritetopic";
    var apidata = "topicID=" + encodeTxt(topicid);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to like topic.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    setUnlikeBtn(btnid, topicid, true);
                    // refresh topic info
                    LoadMyFavoriteTopics();
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}

function unlike(btnid, topicid) {
    setLikeBtn(btnid, topicid, false);

    var apiurl = "/api/topic/removefavouritetopic";
    var apidata = "topicID=" + encodeTxt(topicid);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to unlike topic.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    setLikeBtn(btnid, topicid, true);
                    // refresh topic info
                    LoadMyFavoriteTopics();
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}

function createTopic(topicid, topicname, topicdesp, topiccount, group) {
    var output = "";

    output = "<li class='list-group-item'>"
           + "  <a class='btn btn-default like-btn' id='btn_topic_like_" + topicid + "' style='display:none'>&nbsp;</a>"
           + "  <span class='badge'>" + topiccount + "</span>"
           + "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "&group=" + encodeTxt(group) + "'>#" + topicname + "#</a>"
           + "</li>"

    return output;
}


// Welcome
function RefreshWelcomePageTimer() {
    refreshWelcomePage();
    var timer = $.timer(refreshWelcomePage, 60000, true);
}

function RefreshWelcomePage() {
    // load new
    refreshWelcomeNew();
    refreshWelcomeCategories();

    // load my favorites
    refreshWelcomeFavorites();
    // load hot topics
    refreshWelcomeHotTopics();
    // load active users
    refreshWelcomeActiveUsers();

    // load notifications
    refreshWelcomeMentionsFeeds();
    refreshWelcomeMyOwnsFeeds();
    refreshWelcomeRepliesFeeds();

    // load latest feeds
    refreshWelcomeLatestFeeds();
}

function refreshWelcomeNew() {
    var listdiv = $("#welcome_new_list");
    if (listdiv.length == 0) {
        return;
    }

    // notifications
    var apiurl = "/api/account/getnotificationcount";
    var apidata = "";

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                // do nothing
            }
            else {
                var output = "";
                var homelineCount = data.UnreadHomelineMsgCount;
                var ownerlineCount = data.UnreadOwnerlineMsgCount;
                var atlineCount = data.UnreadAtlineMsgCount;
                var replyCount = data.UnreadReplyCount;
                var userid = data.Userid;
                var notificationCount = ownerlineCount + replyCount + atlineCount;

                $("#welcome_new_reply_count").html(replyCount);
                $("#welcome_new_atline_count").html(atlineCount);
                $("#welcome_new_ownerline_count").html(ownerlineCount);

                // menu navbar count
                if (homelineCount > 0)
                    $("#nav_home_count").html(homelineCount);
                else
                    $("#nav_home_count").html("");

                if (notificationCount > 0)
                    $("#nav_notification_count").html(notificationCount);
                else
                    $("#nav_notification_count").html("");
            }
        },
        null,
        null,
        "no_message_box"
    );
}

function refreshWelcomeCategories() {
    var listdiv = $("#welcome_new_categories_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/category/getmycategory";
    var apidata = "";

    listdiv.empty();
    listdiv.append("<li id='loading_message_categories' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            // clear list
            listdiv.empty();

            if (isNullOrEmpty(data) || data.length == 0) {
            }
            else {
                var categories = {};

                $.each(data, function (index, item) {
                    var id = item.ID;
                    var name = item.Name;
                    var groupid = item.GroupID;
                    var description = item.Description;
                    var creater = item.Creater;
                    var createtimestamp = item.CreateTimestamp;
                    var eventcount = item.EventCount;

                    if (isNullOrEmpty(categories[name])) {
                        categories[name]= eventcount;
                    }
                    else {
                        categories[name]+= eventcount;
                        }
                })

                for (var name in categories) {
                    var output = "";

                    output += "<li class='list-group-item'>";
                    output += "  <span class='badge'>" + categories[name] + "</span>";
                    output += "  <a href='/category/index?category=" + encodeTxt(name) + "'>" + name + "</a>";
                    output += "</li>";
                    listdiv.append(output);
                }
            }
        },
        null,
        null,
        "loading_message_categories"
    );
}

function refreshWelcomeFavorites() {
    var listdiv = $("#welcome_new_favorites_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/topic/getmyfavouritetopic";
    var apidata = "";

    listdiv.empty();
    listdiv.append("<li id='loading_message_favorites' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    // my topics
    var maxcount = 99;

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.", "loading_message_favorites");
            }
            else {
                // clear list
                listdiv.empty();

                // create my topic
                $.each(data, function (index, item) {
                    var output = "";
                    var userid = item.userid;
                    var topicid = item.topicID;
                    var unreadcount = item.UnreadMsgCount;
                    var topicname = item.topicName;
                    var topicdesp = item.topicDescription;
                    var topiccount = item.topicMsgCount;
                    var group = item.GroupID;

                    if (unreadcount > 0) {
                        output = "<li class='list-group-item'>"
                               + "  <span class='badge'>" + unreadcount + "</span>"
                               + "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "&group=" + encodeTxt(group) + "'>#" + topicname + "#</a>"
                               + "</li>";
                        listdiv.append(output);
                    }

                    if (maxcount-- <= 0) {
                        return false;
                    }
                })
            }
        },
        null,
        null,
        "loading_message_favorites"
    );
}

function refreshWelcomeHotTopics() {
    var listdiv = $("#welcome_hot_topics_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/topic/hottopics";
    var apidata = "count=10";

    listdiv.empty();
    listdiv.append("<li id='loading_message_hot_topics' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.", "loading_message_hot_topics");
            }
            else {
                // clear list
                listdiv.empty();

                // create list
                $.each(data, function (index, item) {
                    var output = "";
                    var topicid = item.Id;
                    var topicname = item.Name;
                    var topicdesp = item.Description;
                    var topiccount = item.MsgCount;
                    var isliked = item.IsLiked;
                    var group = item.GroupID;

                    listdiv.append(createTopic(topicid, topicname, topicdesp, topiccount, group));
                    setTopicLikeBtn("btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        },
        null,
        null,
        "loading_message_hot_topics"
    );
}

function refreshWelcomeActiveUsers() {
    var listdiv = $("#welcome_active_users_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/account/activeusers";
    var apidata = "count=10";

    listdiv.empty();
    listdiv.append("<li id='loading_message_active_users' class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.", "loading_message_active_users");
            }
            else {
                // clear list
                listdiv.empty();

                // create list
                $.each(data, function (index, item) {
                    var userid = item.Userid;
                    var username = item.DisplayName;
                    var picurl = item.PortraitUrl;
                    var desp = item.Description;
                    var postscount = item.MessageCount;
                    var followingcount = item.FollowingsCount;
                    var followerscount = item.FollowersCount;
                    var isFollowing = item.IsFollowing;

                    if (isNullOrEmpty(picurl)) {
                        picurl = "/Content/Images/default_avatar.jpg";
                    }

                    output = "<li class='list-group-item'>"
                           + "  <a class='btn btn-default follow-btn' id='btn_user_follow_" + encodeId(userid) + "' style='display:none'>&nbsp;</a>"
                           + "  <span class='badge'>" + postscount + "</span>"
                           + "  <img class='img-rounded' src='" + picurl + "' width='25' height='25' />"
                           + "  <a class='fullname' href='/profile/index?user=" + encodeTxt(userid) + "'>" + username + "</a>"
                           + "  <span class='username'>@" + userid + "</span>"
                           + "</li>";

                    listdiv.append(output);
                    setUserFollowBtn("btn_user_follow_" + encodeId(userid), userid, isFollowing);
                })
            }
        },
        null,
        null,
        "loading_message_active_users"
    );
}

function refreshWelcomeLatestFeeds() {
    var listdiv = $("#welcome_latest_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/homeline";
    var apidata = "userid=&count=5&keepUnread=true";

    refreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function refreshWelcomeMentionsFeeds() {
    var listdiv = $("#welcome_mentions_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/atline";
    var apidata = "userid=&count=5&keepUnread=true";

    refreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function refreshWelcomeMyOwnsFeeds() {
    var listdiv = $("#welcome_myowns_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/ownerline";
    var apidata = "userid=&count=5&keepUnread=true";

    refreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function refreshWelcomeRepliesFeeds() {
    var listdiv = $("#welcome_replies_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/reply/getmyreply";
    var apidata = "count=5&keepUnread=true";

    refreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function refreshWelcomeFeeds(listdiv, apiurl, apidata, msgLength) {
    listdiv.empty();
    listdiv.append("<li class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            // clear list
            listdiv.empty();

            if (isNullOrEmpty(data) || data.message.length == 0) {
                listdiv.append("<li class='list-group-item txtaln-c'>No content.</li>");
            }
            else {
                data = data.message;

                // create list
                $.each(data, function (index, item) {
                    var output = "";
                    var type = item.Type;
                    var id = item.ID;
                    var sid = item.SchemaID;
                    var eid = item.EventID;
                    var msg = item.MessageContent;
                    var richmsgid = item.RichMessageID;
                    var posttime = item.PostTime;
                    var user = item.User.Userid;
                    var username = item.User.DisplayName;
                    var picurl = item.User.PortraitUrl;
                    var userdesp = item.User.Description;
                    var owners = item.Owner;
                    var atusers = item.AtUser;
                    var topics = item.TopicName;
                    var attach = item.Attachment;
                    var importence = item.Importance;
                    var group = item.Group;

                    if (type == "reply") {
                        var mid = item.MessageID;
                        var muser = item.MessageUser;
                    } else {
                        var mid = id;
                        var muser = user;
                    }

                    output = "<li class='list-group-item'>"
                           + "  <a class='fr' target='_blank' href='/Message/Index?user=" + encodeTxt(muser) + "&msgID=" + encodeTxt(mid) + "'>Open</a>"
                           + "  <a class='fullname' href='/profile/index?user=" + encodeTxt(user) + "'>" + username + "</a>"
                           + "  <span class='username'> - " + Time2Now(posttime) + "</span>"
                           + "  <p class='break-word'>" + cutHtml(msg, msgLength, atusers, topics, group) + "</p>"
                           + "</li>";

                    listdiv.append(output);
                });
            }
        },
        function (XMLHttpRequest, textStatus, errorThrown) {
            listdiv.empty();
            listdiv.append("<li class='list-group-item txtaln-c'>" + XMLHttpRequest.responseJSON.Message + "</li>");
        }
    );
}


// Group
function LoadMyGroupNavbar() {
    var listdiv = $("#group_navabar");
    if (listdiv.length == 0) {
        return;
    }
    var setdefaultbtn = $("#btn_set_default_group");
    var curgroup = $("#hd_current_group").val();

    var apiurl = "/api/group/getjoinedgroup";
    var apidata = "";

    AjaxGetSync(
        apiurl,
        apidata,
        function (data) {
            // clear list
            listdiv.empty();
            setdefaultbtn.hide();

            if (isNullOrEmpty(data) || data.length == 0) {
                // do nothing
            }
            else {
                // create list
                $.each(data, function (index, item) {
                    var output = "";
                    var groupid = item.GroupID;
                    var displayname = item.DisplayName;
                    var description = item.Description;
                    var isopen = item.IsOpen;
                    var memberid = item.MemberID;
                    var role = item.Role;
                    var curgroup = $("#hd_current_group").val();

                    if (index == 0) {   // first is default
                        $("#hd_default_group").val(groupid);
                        if (isNullOrEmpty(curgroup)) {
                            $("#hd_current_group").val(groupid);
                        }
                    }

                    output = "<li id='group_navbar_" + encodeId(groupid) + "'>"
                           + "  <a href= 'javascript:void(0);' onclick='switchCurrentGroup(\"" + groupid + "\");'>" + displayname + "</a>"
                           + "</li>";

                    listdiv.append(output);
                });

                // active current group tab
                curgroup = $("#hd_current_group").val();
                var curtab = $("#group_navbar_" + encodeId(curgroup));
                if (curtab.length > 0) {
                    curtab.addClass("active");
                }
                else {
                    listdiv.children[0].addClass("active");
                }
            }
        },
        null,
        null,
        "no_message_box"
    );
}

function switchCurrentGroup(gid) {
    if (isLoadFeeds) return;    // if it is loading, skip this time.

    var listdiv = $("#group_navabar");
    if (listdiv.length == 0) {
        return;
    }
    var setdefaultbtn = $("#btn_set_default_group");
    var defaultgroup = $("#hd_default_group").val();

    var items = listdiv.children("li");
    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        if (item.id == ("group_navbar_" + gid)) {
            item.classList.add("active");
            $("#hd_current_group").val(gid);
            LoadFeeds(true);
        }
        else {
            item.classList.remove("active");
        }
    }

    if (gid == defaultgroup) {
        setdefaultbtn.hide();
    }
    else {
        setdefaultbtn.show();
    }
}

function LoadPostGroupList() {
    var listdiv = $("#postmessage_groups");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/group/getjoinedgroup";
    var apidata = "";

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            // disable post btn
            $("#btn_post").attr("disabled", "disabled");
            // clear list
            listdiv.empty();

            if (isNullOrEmpty(data) || data.length == 0) {
                // do nothing
            }
            else {
                // create list
                $.each(data, function (index, item) {
                    var output = "";
                    var groupid = item.GroupID;
                    var displayname = item.DisplayName;
                    var description = item.Description;
                    var isopen = item.IsOpen;
                    var memberid = item.MemberID;
                    var role = item.Role;

                    output = "<option value='" + groupid + "'>" + displayname + "</option>";
                    listdiv.append(output);
                });

                // enable post btn
                $("#btn_post").removeAttr("disabled");
            }
        },
        null,
        null,
        "no_message_box"
    );
}

function SetDefaultGroup() {
    var curgroup = $("#hd_current_group").val();
    if (isNullOrEmpty(curgroup)) {
        return;
    }
    var btn = $("#btn_set_default_group");

    var apiurl = "/api/group/setdefaultgroup";
    var apidata = "group=" + encodeTxt(curgroup);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to set default group.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    $("#hd_default_group").val(curgroup);
                    btn.hide();
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}

function LoadGroups(category, group) {
    var listdiv = $("#grouplist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(category) || category == "allgroups")
        apiurl = "/api/group/getallgroup";
    else if (category == "joinedgroups")
        apiurl = "/api/group/getjoinedgroup";
    else if (category == "ownedgroups")
        apiurl = "/api/group/getownedgroup";
    else {
        showError("Illegal operation.");
        return;
    }
    if (!isNullOrEmpty(group)) {
        apidata = "groupid=" + encodeTxt(group);
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.");
            }
            else {
                // clear list
                listdiv.empty();

                // create list
                $.each(data, function (index, item) {
                    listdiv.append(createGroupCard(item));
                    setGroupJoinBtn("btn_group_join_" + encodeId(item.GroupID), item.GroupID, (item.IsOpen ? (item.IsJoined) : -1));
                })
            }
        }
    );
}

function createGroupCard(data) {
    var output = "";
    var groupid = data.GroupID;
    var groupname = data.DisplayName;
    var picurl = "";
    var desp = data.Description;
    var isopen = data.IsOpen;
    var isjoined = data.IsJoined;

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    output = "  <li class='grouplist-group-item'>"
           + "    <div class='group-card-info'>"
           //+ "      <div class='ma-btm-10'>"
           //+ "        <img class='img-rounded' id='group_pic_" + encodeId(groupid) + "' src='" + picurl + "' width='100' height='100' />"
           //+ "      </div>"
           + "      <div>"
           + "        <a class='fullname' id='group_name_" + encodeId(groupid) + "' href='javascript:void(0);'>" + groupname + "</a>"
           + "      </div>"
           + "      <div class='group-card-desp'>"
           + "        <span class='username' id='group_id_" + encodeId(groupid) + "'>" + desp + "</span>"
           + "      </div>"
           + "    </div>"
           + "    <div class='group-card-postinfo'>"
           + "      <div class='btn-group btn-group-justified'>"
           + "        <a class='btn btn-default join-btn' id='btn_group_join_" + encodeId(groupid) + "'>&nbsp;</a>"
           + "      </div>"
           + "    </div>"
           + "  </li>";

    return output;
}

function setGroupJoinBtn(btnid, group, isJoined) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    if (isJoined == 0) {
        setJoinBtn(btnid, group, true);
    } else if (isJoined == 1) {
        setLeaveBtn(btnid, group, true);
    } else {  // -1: myself
        setCloseBtn(btnid, group);
    }
}

function setCloseBtn(btnid, group) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Not open");
    btn.attr("class", "btn btn-info join-btn");
    btn.attr("href", "javascript:void(0)");
    btn.show();
}

function setLeaveBtn(btnid, group, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Joined");
    btn.attr("class", "btn btn-success join-btn");
    if (enabled) {
        btn.attr("onclick", "leave('" + btnid + "', '" + group + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "leaveBtnMouseOver('" + btnid + "');")
    btn.attr("onmouseout", "leaveBtnMouseOut('" + btnid + "');")
    btn.show();
}

function leaveBtnMouseOver(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger join-btn");
    btn.text("- Leave");
}

function leaveBtnMouseOut(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success join-btn");
    btn.text("Joined");
}

function setJoinBtn(btnid, group, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("+ Join");
    btn.attr("class", "btn btn-primary join-btn");
    if (enabled) {
        btn.attr("onclick", "join('" + btnid + "', '" + group + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
    btn.show();
}

function join(btnid, group) {
    setLeaveBtn(btnid, group, false);

    var apiurl = "/api/group/joingroup";
    var apidata = "groupid=" + encodeTxt(group);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to join group.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    setLeaveBtn(btnid, group, true);
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}

function leave(btnid, group) {
    setJoinBtn(btnid, group, false);

    var apiurl = "/api/group/leavegroup";
    var apidata = "groupid=" + encodeTxt(group);

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data)) {
                showError("Failed to leave group.");
            }
            else {
                var code = data.ActionResultCode;
                var msg = data.Message;
                if (code == "0") {
                    setJoinBtn(btnid, group, true);
                }
                else {
                    showError(msg);
                }
            }
        }
    );
}


// performance chart
var chart_axis_perf_template = {
    tooltip: {
        trigger: 'axis'
    },
    toolbox: {
        show: true,
        y: 'center',
        orient: 'vertical',
        feature: {
            mark: {
                show: true,
                title: {
                    mark: 'New markline',
                    markUndo: 'Erase markline',
                    markClear: 'Clear all marklines'
                },
            },
            magicType: {
                show: true,
                title: {
                    line: 'Line view',
                    bar: 'Bar view',
                    stack: 'Stack view',
                    tiled: 'Tiled view'
                },
                type: ['line', 'bar', 'stack', 'tiled']
            },
            dataZoom: {
                show: true,
                title: {
                    dataZoom: 'Data zoom',
                    dataZoomReset: 'Data zoom reset'
                }
            },
            dataView: {
                show: true,
                title: 'Data view',
                readOnly: false
            },
            restore: {
                show: true,
                title: 'Restore'
            },
            saveAsImage: {
                show: true,
                title: 'Save as image',
                lang: ['Right click and save']
            }
        }
    },
    dataZoom: {
        show: true,
        realtime: true,
        start: 50,
        end: 100
    },
    calculable: true,
    backgroundColor: '#fff',
    title: {
        text: '',
        subtext: '',
        x: 'center'
    },
    legend: {
        x: 'left',
        y: 'top',
        data: []
    },
    xAxis: [
        {
            type: 'category',
            data: []
        }
    ],
    yAxis: [
        {
            type: 'value',
        },
        {
            type: 'value',
        }
    ],
    series: []
};

function LoadPerfChart(chartname, chartdivid) {
    var chartdiv = $("#" + chartdivid);
    if (chartdiv.length == 0 || isNullOrEmpty(chartname)) {
        return;
    }

    var apiurl = "/api/metricchart/getchart";
    var apidata = "chartName=" + encodeTxt(chartname);
    var option = chart_axis_perf_template;

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.DataSet.length == 0) {
                // no chart
                chartdiv.hide();
            }
            else {
                var name = data.Name;
                var title = data.Title;
                var subtitle = data.SubTitle;
                var groupid = data.GroupID;
                var dataset = data.DataSet;

                // set title
                option.title.text = title;
                option.title.subtext = subtitle;

                // get dataset
                $.each(dataset, function (index, item) {
                    var id = item.ID;
                    var legend = item.Legend;
                    var type = item.Type;
                    var mid = item.MetricDataSetID;

                    apiurl = "/api/metricchart/retrivelastestdatarecord";
                    apidata = "id=" + encodeTxt(mid) + "&count=365";
                    var xdata = [];
                    var ydata = [];

                    // get data of dataset
                    AjaxGetSync(
                        apiurl,
                        apidata,
                        function (data) {
                            if (isNullOrEmpty(data) || data.length == 0) {
                                // no data
                                return;
                            }
                            else {
                                $.each(data, function (index, item) {
                                    var key = item.Key;
                                    var value = item.Value;

                                    xdata.push(key);
                                    ydata.push(value);
                                });

                                // set legend
                                option.legend.data.push(legend);

                                // set data
                                option.xAxis[0].data = xdata;
                                option.series.push({
                                    name: legend,
                                    type: type,
                                    data: ydata,
                                    markPoint: {
                                        data: [
                                            { type: 'max', name: 'Max' },
                                            { type: 'min', name: 'Min' }
                                        ]
                                    },
                                    markLine: {
                                        data: [
                                            { type: 'average', name: 'Average' }
                                        ]
                                    }
                                });
                            }
                        },
                        null,
                        null,
                        "no_message_box"
                    );
                });

                // draw chart
                try {
                    if (chartdiv.height() < 400)
                        chartdiv.height(400);
                    chartdiv.show();

                    var chart = echarts.init(chartdiv[0]);
                    chart.setOption(option);
                }
                catch (ex) {
                    chartdiv.hide();
                    showError(ex);
                }
            }
        },
        function () {
            chartdiv.hide();
        },
        null,
        "no_message_box"
    );
}

function addfakedata() {
    for (var i = 0; i < 30; i++) {
        AjaxGetSync(
            "/api/metricchart/insertrecord",
            "id=7&key=" + i + "&value=" + (Math.random() * 100)
        );
        AjaxGetSync(
            "/api/metricchart/insertrecord",
            "id=8&key=" + i + "&value=" + (Math.random() * 100)
        );
        AjaxGetSync(
            "/api/metricchart/insertrecord",
            "id=9&key=" + i + "&value=" + (Math.random() * 100)
        );
    }
}

// category function
function LoadCategoryFeeds() {
    var listdiv = $("#feedlist");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "";
    var apidata = "";
    var category = "";
    var group = "";

    // get paras from page
    if ($("#hd_category_name").length > 0) {
        category = $("#hd_category_name").val();
    }
    if ($("#hd_current_group").length > 0) {
        group = $("#hd_current_group").val();
    }

    // get
    if (isNullOrEmpty(category)) {
        apiurl = "/api/category/RetriveAllCategoryMessage";
        if (!isNullOrEmpty(group)) {   // set group value
            apidata = "group=" + group;
        }
    }
    else {
        if (!isNullOrEmpty(group)) {   // set group value
            apiurl = "/api/category/retrivecategorymessage";
            apidata = "name=" + encodeTxt(category);
            apidata += "&group=" + group;
        }
        else {
            apiurl = "/api/category/retrivecategorymessagebyname";
            apidata = "name=" + encodeTxt(category);
        }
    }

    AjaxGetAsync(
        apiurl,
        apidata,
        function (data) {
            if (isNullOrEmpty(data) || data.length == 0) {
                showMessage("No content.");
            }
            else {
                // clear feed list
                listdiv.empty();

                // create feed list
                $.each(data, function (index, item) {
                    listdiv.append(createCategoryFeed(item));
                    initUserPopover("user_pic_" + item.ID, item.NotifyTo.Userid);
                })
            }
        }
    );
}

function createCategoryFeed(data) {
    var output = "";
    var id = data.ID;
    var fromuser = data.User;
    var user = data.NotifyTo.Userid;
    var username = data.NotifyTo.DisplayName;
    var picurl = data.NotifyTo.PortraitUrl;
    var userdesp = data.NotifyTo.Description;
    var posttime = data.PostTime;
    var group = data.Group.GroupID;
    var groupname = data.Group.DisplayName;
    var cid = data.CategoryID;
    var cname = data.CategoryName;
    var events = data.EventIDs;
    var msg = "";

    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    // create msg
    if (!isNullOrEmpty(events)) {
        msg = "You got " + events.count + " to-do events.";
        for (var e in events) {
            msg += "<a class='btn btn-link' href='/Event/index?eventid=" + encodeTxt(e) + "'>" + e + "</a>";
        }
    }
    else {
        msg = "You have no to-do event now.";
    }

    output += "  <li id='feed_" + id + "' class='list-group-item'>";
    output += "    <div>"

    // user pic
    output += "      <div class='feed-pic'>";
    output += "        <button class='btn btn-img' type='button' id='user_pic_" + id + "' href='javascript:void(0);'>";
    output += "          <img class='img-rounded' src='" + picurl + "' width='100' height='100'/>";
    output += "        </button>";
    output += "      </div>";

    output += "      <div class='feed-content'>";

    // user name and post time
    output += "        <div class='newpost-header'>";
    output += "          <a id='username_" + id + "' class='fullname' href='/profile/index?user=" + encodeTxt(user) + "'>" + username + "</a>&nbsp;";
    output += "          <span class='badge'>@" + user + "&nbsp;-&nbsp;" + Time2Now(posttime) + "</span>&nbsp;-&nbsp;";
    output += "          <a href='/category/index?category=" + encodeTxt(cname) + "&group=" + group + "'>" + cname + " (" + groupname + ")</a>&nbsp;";
    output += "        </div>";

    // message
    if (!isNullOrEmpty(msg)) {
        output += "    <div id='message_" + id + "' class='newpost-input'>" + msg + "</div>";
    }

    output += "      </div>";

    output += "    </div>";
    output += "  </li>";

    return output;
}

