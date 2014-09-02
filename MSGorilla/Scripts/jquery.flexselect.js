(function ($) {
    $.flexselect = function (select, options) { this.init(select, options); };

    $.extend($.flexselect.prototype, {
        settings: {
            allowMismatch: false,
            selectedClass: "active",
            dropdownClass: "flexselect_dropdown",
            inputIdTransform: function (id) { return id + "_flexselect"; },
            inputNameTransform: function (name) { return; },
            dropdownIdTransform: function (id) { return id + "_flexselect_dropdown"; },
            counterIdTransform: function (id) { return id + "_counter"; },
            leastResults: 1,
            leastKeyLength: 3,
            maxDropdownCount: 10,
        },
        //select: null,
        input: null,
        counter: null,
        //hidden: null,
        dropdown: null,
        userCache: {},
        topicCache: {},
        results: [],
        lastAbbreviation: null,
        abbreviationBeforeFocus: null,
        selectedIndex: 0,
        picked: false,
        dropdownMouseover: false, // Workaround for poor IE behaviors
        dropdownShowed: false,

        init: function (select, options) {
            $.extend(this.settings, options);
            this.input = $(select);
            this.counter = $("#"+this.settings.counterIdTransform(this.input.attr("id")));
            this.preloadCache();
            this.renderControls();
            this.wire();
        },

        preloadCache: function () {
            var apiurl = "";
            var self = this;

            //apiurl = "/api/account/user"
            apiurl = "/api/account/followings"
            $.ajax({
                type: "GET",
                url: apiurl,
                dataType: "json",
                success: function (data) {
                    data.map(function (item) {
                        self.userCache[$.trim(item.Userid)] = { key: $.trim(item.Userid), value: "@" + $.trim(item.Userid), name: $.trim(item.DisplayName), picurl: item.PortraitUrl, score: 0.0 };
                    });
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    ShowError(textStatus + ": " + errorThrown);
                }
            });

            //apiurl = "/api/topic/getalltopic"
            //apiurl = "/api/topic/hottopics?count=25"
            apiurl = "/api/topic/getmyfavouritetopic";
            $.ajax({
                type: "GET",
                url: apiurl,
                dataType: "json",
                success: function (data) {
                    data.map(function (item) {
                        self.topicCache[$.trim(item.Name)] = { key: $.trim(item.Name), value: "#" + $.trim(item.Name) + "#", id: $.trim(item.Id), desp: $.trim(item.Description), score: 0.0 };
                    });
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    ShowError(textStatus + ": " + errorThrown);
                }
            });

        },

        renderControls: function () {
            var selectid = this.input.attr("id");
            var selectname = this.input.attr("name");

            //this.hidden = $("<input type='hidden'/>").attr({
            //    id: selectid,
            //    name: selectname
            //});

            //this.input = this.select.attr({
            //    id: this.settings.inputIdTransform(selectid),
            //    name: this.settings.inputNameTransform(selectname),
            //});

            this.dropdown = $("<div><div class='list-group'></div></div>").attr({
                id: this.settings.dropdownIdTransform(selectid)
            }).addClass(this.settings.dropdownClass);

            //this.select.after(this.hidden);
            $("body").append(this.dropdown);
        },

        wire: function () {
            var self = this;

            this.input.click(function () {
                self.lastAbbreviation = null;
                self.focus();
            });

            this.input.mouseup(function (event) {
                // This is so Safari selection actually occurs.
                event.preventDefault();
            });

            this.input.focus(function () {
                self.abbreviationBeforeFocus = self.input.val();
                //self.input.select();
                //if (!self.picked)
                self.filterResults();
            });

            this.input.blur(function () {
                if (!self.dropdownMouseover) {
                    self.hide();
                    //if (!self.picked)
                    self.reset();
                }
            });

            this.dropdown.mouseover(function (event) {
                if (event.target.tagName == "A") {
                    var rows = self.dropdown.find("a");
                    self.markSelected(rows.index($(event.target)));
                }
                self.dropdownMouseover = true;
            });
            this.dropdown.mouseleave(function () {
                self.markSelected(-1);
                self.dropdownMouseover = false;
            });
            this.dropdown.mousedown(function (event) {
                event.preventDefault();
            });
            this.dropdown.mouseup(function (event) {
                self.pickSelected();
                self.focusAndHide();
            });

            this.input.keyup(function (event) {
                if (self.dropdownShowed) {
                    switch (event.keyCode) {
                        case 13: // return
                            event.preventDefault();
                            self.pickSelected();
                            self.focusAndHide();
                            break;
                        case 27: // esc
                            event.preventDefault();
                            self.reset();
                            self.focusAndHide();
                            break;
                        default:
                            self.filterResults();
                            break;
                    }
                } else {
                    self.counter.html(999 - self.input.val().length);
                    self.filterResults();
                }
            });

            this.input.keydown(function (event) {
                if (self.dropdownShowed) {
                    switch (event.keyCode) {
                        case 33: // pgup
                            event.preventDefault();
                            self.markFirst();
                            break;
                        case 34: // pgedown
                            event.preventDefault();
                            self.markLast();
                            break;
                        case 38: // up
                            event.preventDefault();
                            self.moveSelected(-1);
                            break;
                        case 40: // down
                            event.preventDefault();
                            self.moveSelected(1);
                            break;
                        case 9:  // tab
                            event.preventDefault();
                            self.pickSelected();
                            self.hide();
                            break;
                        case 13: // return
                        case 27: // esc
                            event.preventDefault();
                            event.stopPropagation();
                            break;
                    }
                }
            });
        },

        filterResults: function () {
            var abbreviation = this.getUserAtCursor();  // try to find user
            if (isNullOrEmpty(abbreviation)) {
                abbreviation = this.getTopicAtCursor(); // try to find topic
            }

            // hide dropdown
            if (isNullOrEmpty(abbreviation)) {
                this.hide();
                return;
            }
            // if key is too short, hide dropdown
            if (abbreviation.length <= this.settings.leastKeyLength) {
                this.hide();
                return;
            }

            // do nothing 
            if (abbreviation == this.lastAbbreviation) return;

            // show dropdown
            var results = [];
            var type = abbreviation.substr(0, 1);
            var key = abbreviation.substr(1);
            var maxcount = this.settings.maxDropdownCount;

            switch (type) {
                case '@':
                    $.each(this.userCache, function () {
                        if (results.length >= maxcount) return false;
                        this.score = LiquidMetal.score(this.key, key);
                        if (this.score > 0.0) results.push(this);
                    });
                    break;
                case '#':
                    $.each(this.topicCache, function () {
                        if (results.length >= maxcount) return false;
                        this.score = LiquidMetal.score(this.key, key);
                        if (this.score > 0.0) results.push(this);
                    });
                    break;
                default:
                    break;
            }
            this.results = results;

            // too few results, so search again.
            if (results.length < this.settings.leastResults)
                this.searchMore(type, key);

            this.sortResults();
            this.renderDropdown(type, key);
            this.markFirst();
            this.lastAbbreviation = abbreviation;
            this.picked = false;
        },

        sortResults: function () {
            this.results.sort(function (a, b) { return b.score - a.score; });
        },

        renderDropdown: function (type, key) {
            var dropdownBorderWidth = this.dropdown.outerWidth() - this.dropdown.innerWidth();
            var inputOffset = this.input.offset();
            this.dropdown.css({
                width: (this.input.outerWidth() - dropdownBorderWidth) + "px",
                top: (inputOffset.top + this.input.outerHeight()) + "px",
                left: inputOffset.left + "px"
            });
            var self = this;

            var list = this.dropdown.children("div").html("");

            switch (type) {
                case '@':
                    $.each(this.results, function () {
                        var userid = this.key;
                        var username = this.name;
                        var picurl = this.picurl;
                        var output = self.CreateUserItem(userid, username, picurl);
                        list.append($("<a class='list-group-item'/>").html(output));
                    });
                    break;
                case '#':
                    $.each(this.results, function () {
                        var topic = this.key;
                        var id = this.id;
                        var desp = this.desp;
                        var output = self.CreateTopicItem(topic, desp);
                        list.append($("<a class='list-group-item'/>").html(output));
                    });
                    break;
                default:
                    break;
            }

            this.dropdownShowed = true;
            this.dropdown.show();
        },

        searchMore: function (type, keyword) {
            var apiurl = "";
            var list = this.dropdown.children("div").html("");
            var self = this;

            switch (type) {
                case '@':
                    apiurl = "/api/account/searchuser?keyword=" + keyword;

                    $.ajax({
                        type: "GET",
                        url: apiurl,
                        dataType: "json",
                        async: false,
                        success: function (data) {
                            $.each(data, function (index, item) {
                                var userid = item.Userid;
                                var username = item.DisplayName;
                                var picurl = item.PortraitUrl;
                                if (!(userid in self.userCache)) {
                                    self.userCache[userid] =
                                    { key: $.trim(item.Userid), value: "@" + $.trim(item.Userid), name: $.trim(item.DisplayName), picurl: item.PortraitUrl, score: LiquidMetal.score(userid, keyword) };

                                    var output = self.CreateUserItem(userid, username, picurl);
                                    list.append($("<a class='list-group-item'/>").html(output));
                                }
                            })
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            ShowError(textStatus + ": " + errorThrown);
                        }
                    });
                    break;
                case '#':
                    apiurl = "/api/topic/searchtopic?keyword=" + keyword;

                    $.ajax({
                        type: "GET",
                        url: apiurl,
                        dataType: "json",
                        async: false,
                        success: function (data) {
                            $.each(data, function (index, item) {
                                var topicid = item.Id;
                                var topicname = item.Name;
                                var topicdesp = item.Description;
                                if (!(topicname in self.topicCache)) {
                                    self.topicCache[topicname] =
                                        { key: $.trim(item.Name), value: "#" + $.trim(item.Name) + "#", id: $.trim(item.Id), desp: $.trim(item.Description), score: LiquidMetal.score(topicname, keyword) };

                                    var output = self.CreateTopicItem(topicname, topicdesp);
                                    list.append($("<a class='list-group-item'/>").html(output));
                                }
                            })
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            ShowError(textStatus + ": " + errorThrown);
                        }
                    });
                    break;
                default:
                    break;
            }
        },

        markSelected: function (n) {
            if (n >= this.results.length) n = 0;
            if (n < 0) n = this.results.length - 1;

            var rows = this.dropdown.find("a");
            rows.removeClass(this.settings.selectedClass);
            this.selectedIndex = n;

            if (n >= 0) $(rows[n]).addClass(this.settings.selectedClass);
        },

        pickSelected: function () {
            var selected = this.results[this.selectedIndex];
            if (selected) {
                var type = selected.value.substr(0, 1);

                switch(type){
                    case "@":
                        this.replacUserAtCursor(selected.value);
                        break;
                    case "#":
                        this.replaceTopicAtCursor(selected.value);
                        break;
                    default:
                        break;
                }

                //this.hidden.val(selected.name);
                this.picked = true;
            } else if (this.settings.allowMismatch) {
                //this.hidden.val("");
            } else {
                this.reset();
            }
        },

        hide: function () {
            this.dropdownShowed = false;
            this.dropdown.hide();
            this.lastAbbreviation = null;
        },

        getUserAtCursor: function () {
            var str = this.input.val();
            var pos = this.input.getSelectionStart();
            var ret = "";

            var i = pos - 1;
            var j = pos;
            while (i >= 0) {
                if (isValidForUserid(str[i])) {
                    i--;
                    continue;
                }
                break;
            }

            if (i < pos - 1 && (str[i] == '@')) { // found sth.
                while (j < str.length) {
                    if (isValidForUserid(str[j])) {
                        j++;
                        continue;
                    }
                    break;
                }
                ret = str.substring(i, j);
            }

            return ret;
        },

        getTopicAtCursor: function () {
            var str = this.input.val();
            var pos = this.input.getSelectionStart();
            var ret = "";

            var i = pos - 1;
            var j = pos;
            while (i >= 0) {
                if (isValidForTopic(str[i])) {
                    i--;
                    continue;
                }
                break;
            }

            if (i < pos - 1 && (str[i] == '#')) { // found sth.
                while (j < str.length) {
                    if (isValidForTopic(str[j])) {
                        j++;
                        continue;
                    }
                    break;
                }
                ret = str.substring(i, j);
            }

            return ret;
        },

        replacUserAtCursor: function (newWord) {
            var str = this.input.val();
            var pos = this.input.getSelectionStart();

            var i = pos - 1;
            var j = pos;
            while (i >= 0) {
                if (isValidForUserid(str[i])) {
                    i--;
                    continue;
                }
                break;
            }

            if (i < pos - 1 && (str[i] == '@')) { // found sth.
                while (j < str.length) {
                    if (isValidForUserid(str[j])) {
                        j++;
                        continue;
                    }
                    break;
                }
                var len = newWord.length + 1;
                if (str[j] != ' ') newWord += " ";
                if (i > 0 && (str[i - 1] != ' ' && str[i - 1] != '\n')) {
                    newWord = " " + newWord;
                    len++;
                }
                this.input.val(str.substring(0, i) + newWord + str.substring(j));
                this.input.moveCursor(i + len);
            }

            return;
        },

        replaceTopicAtCursor: function (newWord) {
            var str = this.input.val();
            var pos = this.input.getSelectionStart();

            var i = pos - 1;
            var j = pos;
            while (i >= 0) {
                if (isValidForTopic(str[i])) {
                    i--;
                    continue;
                }
                break;
            }

            if (i < pos - 1 && (str[i] == '#')) { // found sth.
                while (j < str.length) {
                    if (isValidForTopic(str[j])) {
                        j++;
                        continue;
                    }
                    break;
                }
                var len = newWord.length + 1;
                if (str[j] != ' ') newWord += " ";
                if (i > 0 && (str[i - 1] != ' ' && str[i - 1] != '\n')) {
                    newWord = " " + newWord;
                    len++;
                }
                this.input.val(str.substring(0, i) + newWord + str.substring(j));
                this.input.moveCursor(i + len);
            }

            return;
        },

        CreateUserItem: function (userid, username, picurl) {
            var output = "";

            if (isNullOrEmpty(picurl)) {
                picurl = "/Content/Images/default_avatar.jpg";
            }

            output += "<img class='img-rounded' src='" + picurl + "' width='25' height='25' />&nbsp;&nbsp;"
                    + "<span class='fullname'>" + username + "</span>&nbsp;"
                    + "<span class='username' >@" + userid + "</span>&nbsp;";

            return output;
        },

        CreateTopicItem: function (topic, desp) {
            var output = "";

            output += "<span class='fullname'>#" + topic + "#</span>&nbsp;"
                    + "<span class='username' >" + desp + "</span>&nbsp;";

            return output;
        },

        moveSelected: function (n) { this.markSelected(this.selectedIndex + n); },
        markFirst: function () { this.markSelected(0); },
        markLast: function () { this.markSelected(this.results.length - 1); },
        reset: function () { /* this.input.val(this.abbreviationBeforeFocus);*/ },
        focus: function () { this.input.focus(); },
        focusAndHide: function () { this.focus(); this.hide(); }
    });

    $.fn.flexselect = function (options) {
        this.each(function () {
            new $.flexselect(this, options);
        });
        return this;
    };
})(jQuery);

$.fn.extend({
    getSelectionStart: function () {
        var e = this[0];
        if (e.selectionStart) {
            return e.selectionStart;
        } else if (document.selection) {
            e.focus();
            var r = document.selection.createRange();
            var sr = r.duplicate();
            sr.moveToElementText(e);
            sr.setEndPoint('EndToEnd', r);
            return sr.text.length - r.text.length;
        }

        return 0;
    },

    getSelectionEnd: function () {
        var e = this[0];
        if (e.selectionEnd) {
            return e.selectionEnd;
        } else if (document.selection) {
            e.focus();
            var r = document.selection.createRange();
            var sr = r.duplicate();
            sr.moveToElementText(e);
            sr.setEndPoint('EndToEnd', r);
            return sr.text.length;
        }

        return 0;
    },

    insertAtCursor: function (myValue) {
        var $t = $(this)[0];
        if (document.selection) {
            this.focus();
            sel = document.selection.createRange();
            sel.text = myValue;
            this.focus();
        } else if ($t.selectionStart || $t.selectionStart == '0') {
            var startPos = $t.selectionStart;
            var endPos = $t.selectionEnd;
            var scrollTop = $t.scrollTop;
            $t.value = $t.value.substring(0, startPos) + myValue + $t.value.substring(endPos, $t.value.length);
            this.focus();
            $t.selectionStart = startPos + myValue.length;
            $t.selectionEnd = startPos + myValue.length;
            $t.scrollTop = scrollTop;
        } else {
            this.value += myValue;
            this.focus();
        }
    },

    moveCursor: function (pos) {
        var e = this[0];

        e.focus();
        if (document.selection) {
            var sel = e.createTextRange();
            sel.moveStart('character', pos);
            sel.collapse();
            sel.select();
        } else if (typeof e.selectionStart == 'number' && typeof e.selectionEnd == 'number') {
            e.selectionStart = e.selectionEnd = pos;
        }
    },

});

