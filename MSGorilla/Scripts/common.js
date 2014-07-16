/* common function */
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

function cutHtml(code, length, atusers, topics) {
    if (isNullOrEmpty(code)) {
        return "";
    }

    if (code.length <= length) {
        return encodeHtml(code, atusers, topics);
    }
    else {
        return txt2Html(code.substr(0, length - 3) + "...");
    }
}

function encodeUserid(code) {
    code = code.replace(/@/mg, '_');
    code = code.replace(/\./mg, '_');
    return code;
}

function encodeHtml(code, atusers, topics) {
    code = txt2Html(code);

    // autolink http[s]
    //var strRegex = "http[s]?://(([0-9a-z]+(\\-)?)*[0-9a-z]+(\\.))*[0-9a-z]+(:[0-9]{1,5})?(/([\\w\\-/\\+\\?%#=\\.:{}]|(&#38;))*)?";
    var strRegex = "(https?://)?([\\da-z\\.-]+)\\.([a-z\\.]{2,6})([/\\w \\.-]*)*/?";
    var linkre = new RegExp(strRegex, "gi");

    code = code.replace(linkre, function (s) {
        return ('<a href="' + (html2Txt(s)) + '">' + s + '</a>');
    });

    // autolink @user
    if (!isNullOrEmpty(atusers)) {
        for (var key in atusers) {
            var user = atusers[key];
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
        for (var key in topics) {
            var topic = topics[key];
            var htmltopic = txt2Html(topic);
            code = code.replace(new RegExp("#" + htmltopic + "#", "gm"), ('<a href="/topic/index?topic=' + encodeTxt(topic) + '">#' + htmltopic + '#</a>'));
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
        y: 'top',
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
                msgdiv.height(500);
                var chart = echarts.init(msgdiv[0]);
                var json = eval("(" + msg + ")");

                chart.setOption(json);
                break;

            case "chart-axis-singleaxis":
                msgdiv.height(500);
                var chart = echarts.init(msgdiv[0]);
                var json = eval("(" + msg + ")");

                var option = chart_axis_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.xAxis[0].data = json.xAxis;
                option.series = json.yAxis;

                chart.setOption(option);
                break;

            case "chart-axis-doubleaxes":
                msgdiv.height(500);
                var chart = echarts.init(msgdiv[0]);
                var json = eval("(" + msg + ")");

                var option = chart_axis_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.xAxis[0].data = json.xAxis;
                option.series = json.yAxis;
                for (var key in json.yAxis2) {
                    var y2 = json.yAxis2[key];
                    y2.yAxisIndex = 1;
                    option.series.push(y2);
                }

                chart.setOption(option);
                break;

            case "chart-scatter":
                msgdiv.height(500);
                var chart = echarts.init(msgdiv[0]);
                var json = eval("(" + msg + ")");

                var option = chart_scatter_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.series = json.data;

                chart.setOption(option);
                break;

            case "chart-pie":
                msgdiv.height(500);
                var chart = echarts.init(msgdiv[0]);
                var json = eval("(" + msg + ")");

                var option = chart_pie_template;
                option.title.text = json.title;
                option.title.subtext = json.subtitle;
                option.legend.data = json.legend;
                option.series = json.data;

                chart.setOption(option);
                break;

            default:
                break;
        }
    }
    catch (ex) { }
}


/* show message function */
function showError(msg, msgboxid) {
    var msghtml = "<div class='alert alert-dismissable alert-warning'><button class='close' type='button' data-dismiss='alert'>×</button><p>" + msg + "</p></div>";
    $("#notifications").append(msghtml);

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
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
        apidata = "userid=" + encodeTxt(user);
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

            LoadUserFollowBtn("btn_user_follow_" + encodeUserid(user), user, isFollowimg);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadUserFollowBtn(btnid, user, isFollowing) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    if (isFollowing == 0) {
        SetFollowBtn(btnid, user, true);
    } else if (isFollowing == 1) {
        SetUnfollowBtn(btnid, user, true);
    } else {  // -1: myself
        SetEditProfileBtn(btnid, user);
    }
}

function SetEditProfileBtn(btnid, user) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Edit profile");
    btn.attr("class", "btn btn-info follow-btn");
    btn.attr("href", "/account/manage");
    btn.show();
}

function SetUnfollowBtn(btnid, user, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Following");
    btn.attr("class", "btn btn-success follow-btn");
    if (enabled) {
        btn.attr("onclick", "Unfollow('" + user + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "UnfollowBtnMouseOver('" + btnid + "');")
    btn.attr("onmouseout", "UnfollowBtnMouseOut('" + btnid + "');")
    btn.show();
}

function UnfollowBtnMouseOver(btnid) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-danger follow-btn");
    btn.text("Unfollow");
}

function UnfollowBtnMouseOut(btnid) {
    var btn = $("#" +btnid);
    if (btn.length == 0) {
        return;
    }

    btn.attr("class", "btn btn-success follow-btn");
    btn.text("Following");
}

function SetFollowBtn(btnid, user, enabled) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    btn.text("Follow");
    btn.attr("class", "btn btn-primary follow-btn");
    if (enabled) {
        btn.attr("onclick", "Follow('" + user + "');");
    }
    else {
        btn.attr("onclick", "");
    }
    btn.attr("onmouseover", "")
    btn.attr("onmouseout", "")
    btn.show();
}

function Follow(btnid, user) {
    SetUnfollowBtn(btnid, user, false);
    $.ajax({
        type: "GET",
        url: "/api/account/follow",
        data: "userid=" + encodeTxt(user),
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadUserInfo(user);
                LoadMyInfo();
            }
            else {
                showError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function Unfollow(btnid, user) {
    SetFollowBtn(btnid, user, false);
    $.ajax({
        type: "GET",
        url: "/api/account/unfollow",
        data: "userid=" + encodeTxt(user),
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadUserInfo(user);
                LoadMyInfo();
            }
            else {
                showError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
        showError("Illegal operation.");
        return;
    }

    if (!isNullOrEmpty(user)) {
        apidata = "userid=" + encodeTxt(user);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                showError("No content.");
            }
            else {
                // create user list
                $("#userlist").empty();
                $.each(data, function (index, item) {
                    $("#userlist").append(CreateUserCard(item));
                    LoadUserFollowBtn("btn_user_follow_" + encodeUserid(item.Userid), item.Userid, item.IsFollowing);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
           + "        <img class='img-rounded' id='user_pic_" + encodeUserid(userid) + "' src='" + picurl + "' width='100' height='100' />"
           + "      </div>"
           + "      <div>"
           + "        <a class='' id='user_name_" + encodeUserid(userid) + "' href='/profile/index?user=" + encodeTxt(userid) + "'>" + username + "</a>"
           + "      </div>"
           + "      <div>"
           + "        <span id='user_id_" + encodeUserid(userid) + "'>@" + userid + "</span>"
           + "      </div>"
           + "    </div>"
           + "    <div class='user-card-postinfo'>"
           + "      <div class='btn-group btn-group-justified'>"
           + "        <a class='btn btn-default follow-btn' id='btn_user_follow_" + encodeUserid(userid) + "'>&nbsp;</a>"
           + "      </div>"
           + "    </div>"
           + "  </li>";

    return output;
}


/* post function */
function PostMessage() {
    var msgbox = $("#postmessage");
    var message = encodeTxt(msgbox.val().trim());
    if (message.length === 0) {
        return;
    }

    $("#btn_post").button('loading');
    $.ajax({
        type: "POST",
        url: "/api/message/postmessage",
        data: "message=" + message + "&importance=2", // + "&schemaID=" + "chart-pie",
        dataType: "json",
        success: function (data) {
            msgbox.val("");
            // insert the new posted message
            $("#feedlist").prepend(CreateFeed(data));
            // enhanceMessage(data.SchemaID, data.ID, data.MessageContent);  // user can not post schema data from webpage.
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    }).always(function () {
        $("#btn_post").button('reset');
    });
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
    for (var key in tousers) {
        if (!isNullOrEmpty(tousers[key]))
            apidata += "&to=" + encodeTxt(tousers[key]);
    }

    $("#btn_reply_" + mid).button('loading');
    $.ajax({
        type: "POST",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            replybox.val("");
            $("#replylist_" + mid).prepend(CreateReply(data));
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
        showError("Illegal operation.");
        return;
    }

    apiurl = "/api/message/getdisplaymessage";
    apidata = "msgUser=" + encodeTxt(user) + "&msgID=" + encodeTxt(mid);

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        data: apidata,
        success: function (data) {
            if (isNullOrEmpty(data)) {
                showError("No content.");
            }
            else {
                $("#feedlist").empty();
                // create feed 
                $("#feedlist").append(CreateFeed(data, true));
                enhanceMessage(data.SchemaID, data.ID, data.MessageContent);
                if ($("#rich_message_" + mid).length > 0) {
                    $("#rich_message_" + mid).collapse('show');
                    ShowRichMsg(data.RichMessageID, mid);
                }
                $("#reply_" + mid).collapse('show');
                ShowReplies(mid);
            }

        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
                showError("No content.");
            }
            else {
                nexttoken = data.continuationToken
                data = data.message
                //showError(data);
                if (data.length == 0) {
                    showError("No content.");
                }
                else {
                    // create feed list
                    if (isNullOrEmpty(token)) {
                        // clear feeds at the first time 
                        $("#feedlist").empty();
                    }
                    $.each(data, function (index, item) {
                        $("#feedlist").append(CreateFeed(item));

                        if (count-- > 0) {
                            $("#feed_" + item.ID).addClass('new-notification');
                        }

                        enhanceMessage(item.SchemaID, item.ID, item.MessageContent);
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
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
            $("#lbl_seemore").html("");
            $("#hd_token").val("nomore");
            isLoadFeeds = false;
        }
    });
}

var ImportenceTags = [
    "<span class='label label-danger feed-importence'>Important</span>", // 0
    "<span class='label label-info feed-importence'>Info</span>",
    "<span class='label label-primary feed-importence'>Primary</span>",
    "<span class='label label-success feed-importence'>Success</span>",
    "<span class='label label-warning feed-importence'>Warning</span>",
    "<span class='label label-danger feed-importence'>Danger</span>",
    "<span class='label label-default feed-importence'>Default</span>",
];

function CreateFeed(data, hideOpenBtn) {
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
    output += "        <img class='img-rounded' id='user_pic_" + id + "' src='" + picurl + "' width='100' height='100' />";
    output += "      </div>";

    // importance
    if (importence == 0) {
        output += ImportenceTags[importence] + "&nbsp;";
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
        for (var key in owners) {
            output += "  <a href='/profile/index?user=" + encodeTxt(owners[key]) + "'>@" + owners[key] + "</a>&nbsp;";
        }
        output += "    </div>";
    }

    // message
    if (!isNullOrEmpty(msg)) {
        output += "    <div id='message_" + id + "' class='newpost-input'>" + encodeHtml(msg, atusers, topics) + "</div>";
    }

    // richmsg
    if (!isNullOrEmpty(richmsgid)) {
        output += "    <div id='rich_message_" + id + "' class='newpost-richinput panel-collapse collapse out clickable' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='ShowRichMsg(\"" + richmsgid + "\", \"" + id + "\");'></div>";
    }

    // attachment
    if (!isNullOrEmpty(attach)) {
        output += "    <div class='newpost-input'><span class=''>Attachments: </span>";
        for (var key in attach) {
            output += "  <a class='btn btn-link btn-xs' onclick='ShowAttach(\"" + attach[key].AttachmentID + "\", \"" + attach[key].Filetype + "\", \"" + id + "\");' >" + attach[key].Filename + " (" + attach[key].Filesize + ")</a>&nbsp;";
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
        output += "      <button id='btn_showrichmsg_" + id + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='ShowRichMsg(\"" + richmsgid + "\", \"" + id + "\");'>More</button>";
    }
    // replies btn
    if (type == "message") {
        output += "      <button id='btn_showreply_" + id + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + id + "' href='#reply_" + id + "' onclick='ShowReplies(\"" + id + "\");'>Replies</button>";
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
            for (var key in owners) {
                replyto += owners[key] + ";";
                replytostring += " @" + owners[key];
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
        output += "        </div>";

        output += "      </div>";
        output += "    </div>";
    }

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
                data: "richMsgID=" + encodeTxt(rmid),
                success: function (data) {
                    richmsgdiv.empty();
                    // create rich message
                    richmsgdiv.append(data);

                    // show rich
                    showbtn.text("Less");
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    richmsgdiv.empty();
                    showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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

    scrollTo("feed_" + mid);
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
        data: "msgID=" + encodeTxt(mid),
        success: function (data) {
            // create reply list
            replydiv.empty();
            $.each(data, function (index, item) {
                replydiv.append(CreateReply(item));
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            replydiv.empty();
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function CreateReply(data) {
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
    var mid = data.MessageID;
    var muser = data.MessageUser;
    if (isNullOrEmpty(picurl)) {
        picurl = "/Content/Images/default_avatar.jpg";
    }

    output += "<li class='list-group-item clickable' onclick='SetReplyTo(event, \"" + mid + "\", \"" + user + "\");'>";
    output += "  <div>"

    // user pic
    output += "    <div class='reply-pic'>";
    output += "      <img class='img-rounded' id='reply_user_pic_" + id + "' src='" + picurl + "' width='50' height='50' />";
    output += "    </div>";

    // importance
    if (importence == 0) {
        output += ImportenceTags[importence] + "&nbsp;";
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
        for (var key in owners) {
            output += "<a href='/profile/index?user=" + encodeTxt(owners[key]) + "'>@" + owners[key] + "</a>&nbsp;";
        }
        output += "  </div>";
    }

    // message
    if (!isNullOrEmpty(msg)) {
        output += "  <div id='message_" + id + "' class='reply-input'>" + encodeHtml(msg, atusers, topics) + "</div>";
    }

    // richmsg
    if (!isNullOrEmpty(richmsgid)) {
        output += "  <div id='rich_message_" + id + "' class='reply-richinput panel-collapse collapse out clickable' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='ShowRichMsg(\"" + richmsgid + "\", \"" + id + "\");'></div>";
    }

    // attachment
    if (!isNullOrEmpty(attach)) {
        output += "  <div class='reply-input'><span class=''>Attachments: </span>";
        for (var key in attach) {
            output += "<a class='btn btn-link btn-xs' onclick='ShowAttach(\"" + attach[key].AttachmentID + "\", \"" + attach[key].Filetype + "\", \"" + id + "\");' >" + attach[key].Filename + " (" + attach[key].Filesize + ")</a>&nbsp;";
        }
        output += "  </div>";
        output += "  <div id='attachment_" + id + "' class='reply-input' style='display:none;'></div>";
    }

    // footer btns
    output += "      <div class='reply-footer'>";
    // richmsg btn
    if (!isNullOrEmpty(richmsgid)) {
        output += "    <button id='btn_showrichmsg_" + id + "' class='btn btn-link btn-sm' type='button' data-toggle='collapse' data-parent='#feed_" + id + "' href='#rich_message_" + id + "' onclick='ShowRichMsg(\"" + richmsgid + "\", \"" + id + "\");'>More</button>";
    }
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

    replybox.val(replybox.val() + " @" + user + " ");
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
        data: "msgUser=" + encodeTxt(user) + "&msgID=" + encodeTxt(mid),
        success: function (data) {
            message_div.empty();
            // create message
            message_div.append(CreateFeed(data));
            enhanceMessage(data.SchemaID, data.ID, data.MessageContent);
            $("#reply_" + mid).collapse('show');
            ShowReplies(mid);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}


// search function
function SearchTopic(keyword) {
    var apiurl = "";
    var apidata = "";
    if (isNullOrEmpty(keyword))
        apiurl = "/api/topic/getalltopic";
    else {
        apiurl = "/api/topic/searchtopic";
        apidata = "keyword=" + encodeTxt(keyword);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                showError("No content.");
            }
            else {
                $("#topiclist").empty();
                $.each(data, function (index, item) {
                    var topicid = item.Id;
                    var topicname = item.Name;
                    var topicdesp = item.Description;
                    var topiccount = item.MsgCount;
                    var isliked = item.IsLiked;

                    $("#topiclist").append(CreateTopic(topicid, topicname, topicdesp, topiccount));
                    LoadTopicLikeBtn("btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
        apidata = "keyword=" + encodeTxt(keyword);
    }

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                showError("No content.", "loading_message_searchuser");
            }
            else {
                // create user list
                $("#userlist").empty();
                $.each(data, function (index, item) {
                    $("#userlist").append(CreateUserCard(item));
                    LoadUserFollowBtn("btn_user_follow_" + encodeUserid(item.Userid), item.Userid, item.IsFollowing);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message, "loading_message_searchuser");
        }
    });
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
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
                var isliked = item.IsLiked;

                output = "<li class='sub-list-group-item'>"
                       + "  <a class='btn btn-default like-btn' id='shortcut_btn_topic_like_" + topicid + "' style='display:none'>&nbsp;</a>"
                       + "  <span class='badge'>" + topiccount + "</span>"
                       + "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "'>#" + topicname + "#</a>"
                       + "</li>"
                $("#topic_collapse").append(output);
                LoadTopicLikeBtn("shortcut_btn_topic_like_" + topicid, topicid, isliked);
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadUserFavoriteTopics(user) {
    var apiurl = "/api/topic/getuserfavouritetopic";
    var apidata = "userid=" + encodeTxt(user);

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            if (data.length == 0) {
                showError("No content.");
            }
            else {
                $("#topiclist").empty();
                $.each(data, function (index, item) {
                    var topicid = item.topicID;
                    var topicname = item.topicName;
                    var topicdesp = item.topicDescription;
                    var topiccount = item.topicMsgCount;
                    var isliked = item.IsLiked;

                    $("#topiclist").append(CreateTopic(topicid, topicname, topicdesp, topiccount));
                    LoadTopicLikeBtn("btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
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
                output += "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "'>#" + topicname + "#</a>";
                output += "</li>";
                $("#favorite_collapse").append(output);

                // homepage 
                if (unreadcount > 0 && navlist.length > 0) {
                    output = "";
                    //output += "<li>";
                    output += "  <a class='btn btn-link btn-xs' href='/topic/index?topic=" + encodeTxt(topicname) + "'>#" + topicname + "# <span class='badge'>" + unreadcount + "</span></a> ";
                    //output += "</li>";

                    navlist.append(output);
                }
            })
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            //showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function LoadTopicLikeBtn(btnid, topicid, isLiked) {
    var btn = $("#" + btnid);
    if (btn.length == 0) {
        return;
    }

    if (isLiked == true) {
        SetUnlikeBtn(btnid, topicid, true);
    }
    else if (isLiked == false) {
        SetLikeBtn(btnid, topicid, true);
    }
    else {
        // do nothing.
    }
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
        data: "topicID=" + encodeTxt(topicid),
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadMyFavoriteTopics();
                SetUnlikeBtn(btnid, topicid, true);
            }
            else {
                showError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function Unlike(btnid, topicid) {
    SetLikeBtn(btnid, topicid, false);
    $.ajax({
        type: "GET",
        url: "/api/topic/removefavouritetopic",
        data: "topicID=" + encodeTxt(topicid),
        success: function (data) {
            var code = data.ActionResultCode;
            var msg = data.Message;
            if (code == "0") {
                LoadMyFavoriteTopics();
                SetLikeBtn(btnid, topicid, true);
            }
            else {
                showError(msg);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function CreateTopic(topicid, topicname, topicdesp, topiccount) {
    var output = "";

    output = "<li class='list-group-item'>"
           + "  <a class='btn btn-default like-btn' id='btn_topic_like_" + topicid + "' style='display:none'>&nbsp;</a>"
           + "  <span class='badge'>" + topiccount + "</span>"
           + "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "'>#" + topicname + "#</a>"
           + "</li>"

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

// Welcome
function RefreshWelcomePageTimer() {
    RefreshWelcomePage();
    var timer = $.timer(RefreshWelcomePage, 60000, true);
}

function RefreshWelcomePage() {
    // load new
    RefreshWelcomeNew();

    // load notifications
    RefreshWelcomeMentionsFeeds();
    RefreshWelcomeMyOwnsFeeds();
    RefreshWelcomeRepliesFeeds();

    // load hot topics
    RefreshWelcomeHotTopics();

    // load active users
    RefreshWelcomeActiveUsers();

    // load latest feeds
    RefreshWelcomeLatestFeeds();

}

function RefreshWelcomeNew() {
    var listdiv = $("#welcome_new_list");
    if (listdiv.length == 0) {
        return;
    }

    listdiv.empty();
    listdiv.append("<li class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    // notifications
    var apiurl = "/api/account/getnotificationcount";

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        async: false,
        success: function (data) {
            // create list
            listdiv.empty();

            if (isNullOrEmpty(data)) {
                listdiv.append("<li class='list-group-item txtaln-c'>No content</li>");
            }
            else {
                var output = "";
                var homelineCount = data.UnreadHomelineMsgCount;
                var ownerlineCount = data.UnreadOwnerlineMsgCount;
                var atlineCount = data.UnreadAtlineMsgCount;
                var replyCount = data.UnreadReplyCount;
                var userid = data.Userid;
                var notificationCount = ownerlineCount + replyCount + atlineCount;

                output = "<li class='list-group-item'>"
                       + "  <span class='badge'>" + replyCount + "</span>"
                       + "  <a href='/Notification/index?category=replyline'>Replies</a>"
                       + "</li>"
                listdiv.prepend(output);

                output = "<li class='list-group-item'>"
                       + "  <span class='badge'>" + ownerlineCount + "</span>"
                       + "  <a href='/Notification/index?category=ownerline'>My Owns</a>"
                       + "</li>"
                listdiv.prepend(output);

                output = "<li class='list-group-item'>"
                       + "  <span class='badge'>" + atlineCount + "</span>"
                       + "  <a href='/Notification/index?category=atline'>Mentions</a>"
                       + "</li>"
                listdiv.prepend(output);

                // nav bar count
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
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
            listdiv.empty();
            listdiv.append("<li class='list-group-item txtaln-c'>" + XMLHttpRequest.responseJSON.Message + "</li>");
        }
    });

    // my topics
    apiurl = "/api/topic/getmyfavouritetopic";
    var maxcount = 15;

    $.ajax({
        type: "GET",
        url: apiurl,
        dataType: "json",
        success: function (data) {
            // create my topic
            if (isNullOrEmpty(data)) {
            }
            else {
                $.each(data, function (index, item) {
                    var output = "";
                    var userid = item.userid;
                    var topicid = item.topicID;
                    var unreadcount = item.UnreadMsgCount;
                    var topicname = item.topicName;
                    var topicdesp = item.topicDescription;
                    var topiccount = item.topicMsgCount;

                    if (unreadcount > 0) {
                        output = "<li class='list-group-item'>"
                               + "  <span class='badge'>" + unreadcount + "</span>"
                               + "  <a href='/topic/index?topic=" + encodeTxt(topicname) + "'>#" + topicname + "#</a>"
                               + "</li>";
                        listdiv.append(output);
                    }

                    if (maxcount-- <= 0) {
                        return false;
                    }
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            //showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
        }
    });
}

function RefreshWelcomeHotTopics() {
    var listdiv = $("#welcome_hot_topics_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/topic/hottopics";
    var apidata = "count=10";

    listdiv.empty();
    listdiv.append("<li class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            // create list
            listdiv.empty();

            if (data.length == 0) {
                listdiv.append("<li class='list-group-item txtaln-c'>No content</li>");
            }
            else {
                $.each(data, function (index, item) {
                    var output = "";
                    var topicid = item.Id;
                    var topicname = item.Name;
                    var topicdesp = item.Description;
                    var topiccount = item.MsgCount;
                    var isliked = item.IsLiked;

                    listdiv.append(CreateTopic(topicid, topicname, topicdesp, topiccount));
                    LoadTopicLikeBtn("btn_topic_like_" + topicid, topicid, isliked);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
            listdiv.empty();
            listdiv.append("<li class='list-group-item txtaln-c'>" + XMLHttpRequest.responseJSON.Message + "</li>");
        }
    });
}

function RefreshWelcomeActiveUsers() {
    var listdiv = $("#welcome_active_users_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/account/activeusers";
    var apidata = "count=10";

    listdiv.empty();
    listdiv.append("<li class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            // create list
            listdiv.empty();

            if (data.length == 0) {
                listdiv.append("<li class='list-group-item txtaln-c'>No content</li>");
            }
            else {
                $.each(data, function (index, item) {
                    var userid = item.Userid;
                    var username = item.DisplayName;
                    var picurl = item.PortraitUrl;
                    var desp = item.Description;
                    var postscount = item.MessageCount;
                    var followingcount = item.FollowingsCount;
                    var followerscount = item.FollowersCount;
                    var isFollowing = item.IsFollowing;

                    //if (isNullOrEmpty(picurl)) {
                    //    picurl = "/Content/Images/default_avatar.jpg";
                    //}

                    output = "<li class='list-group-item'>"
                           + "  <a class='btn btn-default follow-btn' id='btn_user_follow_" + encodeUserid(userid) + "' style='display:none'>&nbsp;</a>"
                           + "  <span class='badge'>" + postscount + "</span>"
                           //+ "  <img class='img-rounded' src='" + picurl + "' width='20' height='20' />"
                           + "  <a class='fullname' href='/profile/index?user=" + encodeTxt(userid) + "'>" + username + "</a>"
                           + "  <span class='username'>@" + userid + "</span>"
                           + "</li>";

                    listdiv.append(output);
                    LoadUserFollowBtn("btn_user_follow_" + encodeUserid(userid), userid, isFollowing);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
            listdiv.empty();
            listdiv.append("<li class='list-group-item txtaln-c'>" + XMLHttpRequest.responseJSON.Message + "</li>");
        }
    });
}

function RefreshWelcomeLatestFeeds() {
    var listdiv = $("#welcome_latest_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/publicsquareline";
    var apidata = "count=5";

    RefreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function RefreshWelcomeMentionsFeeds() {
    var listdiv = $("#welcome_mentions_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/atline";
    var apidata = "userid=&count=5&keepUnread=true";

    RefreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function RefreshWelcomeMyOwnsFeeds() {
    var listdiv = $("#welcome_myowns_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/message/ownerline";
    var apidata = "userid=&count=5&keepUnread=true";

    RefreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function RefreshWelcomeRepliesFeeds() {
    var listdiv = $("#welcome_replies_feeds_list");
    if (listdiv.length == 0) {
        return;
    }

    var apiurl = "/api/reply/getmyreply";
    var apidata = "count=5&keepUnread=true";

    RefreshWelcomeFeeds(listdiv, apiurl, apidata, 140);
}

function RefreshWelcomeFeeds(listdiv, apiurl, apidata, msgLength) {
    listdiv.empty();
    listdiv.append("<li class='list-group-item txtaln-c'><span class='spinner-loading'></span> Loading...</li>");

    $.ajax({
        type: "GET",
        url: apiurl,
        data: apidata,
        dataType: "json",
        success: function (data) {
            // create list
            listdiv.empty();

            if (isNullOrEmpty(data) || data.message.length == 0) {
                listdiv.append("<li class='list-group-item txtaln-c'>No content</li>");
            }
            else {
                data = data.message;

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
                           + "  <p class='break-word'>" + cutHtml(msg, msgLength, atusers, topics) + "</p>"
                           + "</li>";

                    listdiv.append(output);
                })
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // showAjaxError(textStatus, errorThrown, XMLHttpRequest.responseJSON.ActionResultCode, XMLHttpRequest.responseJSON.Message);
            listdiv.empty();
            listdiv.append("<li class='list-group-item txtaln-c'>" + XMLHttpRequest.responseJSON.Message + "</li>");
        }
    });
}

