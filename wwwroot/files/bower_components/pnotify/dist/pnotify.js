/*
PNotify 3.0.0 sciactive.com/pnotify/
(C) 2015 Hunter Perrin; Google, Inc.
license Apache-2.0
*/
(function (b, k) {
  "function" === typeof define && define.amd
    ? define("pnotify", ["jquery"], function (q) {
        return k(q, b);
      })
    : "object" === typeof exports && "undefined" !== typeof module
    ? (module.exports = k(require("jquery"), global || b))
    : (b.PNotify = k(b.jQuery, b));
})(this, function (b, k) {
  var q = function (l) {
    var k = {
        dir1: "down",
        dir2: "left",
        push: "bottom",
        spacing1: 36,
        spacing2: 36,
        context: b("body"),
        modal: !1,
      },
      g,
      h,
      n = b(l),
      r = function () {
        h = b("body");
        d.prototype.options.stack.context = h;
        n = b(l);
        n.bind("resize", function () {
          g && clearTimeout(g);
          g = setTimeout(function () {
            d.positionAll(!0);
          }, 10);
        });
      },
      s = function (c) {
        var a = b("<div />", { class: "ui-pnotify-modal-overlay" });
        a.prependTo(c.context);
        c.overlay_close &&
          a.click(function () {
            d.removeStack(c);
          });
        return a;
      },
      d = function (c) {
        this.parseOptions(c);
        this.init();
      };
    b.extend(d.prototype, {
      version: "3.0.0",
      options: {
        title: !1,
        title_escape: !1,
        text: !1,
        text_escape: !1,
        styling: "brighttheme",
        addclass: "",
        cornerclass: "",
        auto_display: !0,
        width: "300px",
        min_height: "16px",
        type: "notice",
        icon: !0,
        animation: "fade",
        animate_speed: "normal",
        shadow: !0,
        hide: !0,
        delay: 8e3,
        mouse_reset: !0,
        remove: !0,
        insert_brs: !0,
        destroy: !0,
        stack: k,
      },
      modules: {},
      runModules: function (c, a) {
        var p, b;
        for (b in this.modules)
          (p = "object" === typeof a && b in a ? a[b] : a),
            "function" === typeof this.modules[b][c] &&
              ((this.modules[b].notice = this),
              (this.modules[b].options =
                "object" === typeof this.options[b] ? this.options[b] : {}),
              this.modules[b][c](
                this,
                "object" === typeof this.options[b] ? this.options[b] : {},
                p
              ));
      },
      state: "initializing",
      timer: null,
      animTimer: null,
      styles: null,
      elem: null,
      container: null,
      title_container: null,
      text_container: null,
      animating: !1,
      timerHide: !1,
      init: function () {
        var c = this;
        this.modules = {};
        b.extend(!0, this.modules, d.prototype.modules);
        this.styles =
          "object" === typeof this.options.styling
            ? this.options.styling
            : d.styling[this.options.styling];
        this.elem = b("<div />", {
          class: "ui-pnotify " + this.options.addclass,
          css: { display: "none" },
          "aria-live": "assertive",
          "aria-role": "alertdialog",
          mouseenter: function (a) {
            if (c.options.mouse_reset && "out" === c.animating) {
              if (!c.timerHide) return;
              c.cancelRemove();
            }
            c.options.hide && c.options.mouse_reset && c.cancelRemove();
          },
          mouseleave: function (a) {
            c.options.hide &&
              c.options.mouse_reset &&
              "out" !== c.animating &&
              c.queueRemove();
            d.positionAll();
          },
        });
        "fade" === this.options.animation &&
          this.elem.addClass("ui-pnotify-fade-" + this.options.animate_speed);
        this.container = b("<div />", {
          class:
            this.styles.container +
            " ui-pnotify-container " +
            ("error" === this.options.type
              ? this.styles.error
              : "info" === this.options.type
              ? this.styles.info
              : "success" === this.options.type
              ? this.styles.success
              : this.styles.notice),
          role: "alert",
        }).appendTo(this.elem);
        "" !== this.options.cornerclass &&
          this.container
            .removeClass("ui-corner-all")
            .addClass(this.options.cornerclass);
        this.options.shadow && this.container.addClass("ui-pnotify-shadow");
        !1 !== this.options.icon &&
          b("<div />", { class: "ui-pnotify-icon" })
            .append(
              b("<span />", {
                class:
                  !0 === this.options.icon
                    ? "error" === this.options.type
                      ? this.styles.error_icon
                      : "info" === this.options.type
                      ? this.styles.info_icon
                      : "success" === this.options.type
                      ? this.styles.success_icon
                      : this.styles.notice_icon
                    : this.options.icon,
              })
            )
            .prependTo(this.container);
        this.title_container = b("<h4 />", {
          class: "ui-pnotify-title",
        }).appendTo(this.container);
        !1 === this.options.title
          ? this.title_container.hide()
          : this.options.title_escape
          ? this.title_container.text(this.options.title)
          : this.title_container.html(this.options.title);
        this.text_container = b("<div />", {
          class: "ui-pnotify-text",
          "aria-role": "alert",
        }).appendTo(this.container);
        !1 === this.options.text
          ? this.text_container.hide()
          : this.options.text_escape
          ? this.text_container.text(this.options.text)
          : this.text_container.html(
              this.options.insert_brs
                ? String(this.options.text).replace(/\n/g, "<br />")
                : this.options.text
            );
        "string" === typeof this.options.width &&
          this.elem.css("width", this.options.width);
        "string" === typeof this.options.min_height &&
          this.container.css("min-height", this.options.min_height);
        d.notices =
          "top" === this.options.stack.push
            ? b.merge([this], d.notices)
            : b.merge(d.notices, [this]);
        "top" === this.options.stack.push && this.queuePosition(!1, 1);
        this.options.stack.animation = !1;
        this.runModules("init");
        this.options.auto_display && this.open();
        return this;
      },
      update: function (c) {
        var a = this.options;
        this.parseOptions(a, c);
        this.elem.removeClass(
          "ui-pnotify-fade-slow ui-pnotify-fade-normal ui-pnotify-fade-fast"
        );
        "fade" === this.options.animation &&
          this.elem.addClass("ui-pnotify-fade-" + this.options.animate_speed);
        this.options.cornerclass !== a.cornerclass &&
          this.container
            .removeClass("ui-corner-all " + a.cornerclass)
            .addClass(this.options.cornerclass);
        this.options.shadow !== a.shadow &&
          (this.options.shadow
            ? this.container.addClass("ui-pnotify-shadow")
            : this.container.removeClass("ui-pnotify-shadow"));
        !1 === this.options.addclass
          ? this.elem.removeClass(a.addclass)
          : this.options.addclass !== a.addclass &&
            this.elem.removeClass(a.addclass).addClass(this.options.addclass);
        !1 === this.options.title
          ? this.title_container.slideUp("fast")
          : this.options.title !== a.title &&
            (this.options.title_escape
              ? this.title_container.text(this.options.title)
              : this.title_container.html(this.options.title),
            !1 === a.title && this.title_container.slideDown(200));
        !1 === this.options.text
          ? this.text_container.slideUp("fast")
          : this.options.text !== a.text &&
            (this.options.text_escape
              ? this.text_container.text(this.options.text)
              : this.text_container.html(
                  this.options.insert_brs
                    ? String(this.options.text).replace(/\n/g, "<br />")
                    : this.options.text
                ),
            !1 === a.text && this.text_container.slideDown(200));
        this.options.type !== a.type &&
          this.container
            .removeClass(
              this.styles.error +
                " " +
                this.styles.notice +
                " " +
                this.styles.success +
                " " +
                this.styles.info
            )
            .addClass(
              "error" === this.options.type
                ? this.styles.error
                : "info" === this.options.type
                ? this.styles.info
                : "success" === this.options.type
                ? this.styles.success
                : this.styles.notice
            );
        if (
          this.options.icon !== a.icon ||
          (!0 === this.options.icon && this.options.type !== a.type)
        )
          this.container.find("div.ui-pnotify-icon").remove(),
            !1 !== this.options.icon &&
              b("<div />", { class: "ui-pnotify-icon" })
                .append(
                  b("<span />", {
                    class:
                      !0 === this.options.icon
                        ? "error" === this.options.type
                          ? this.styles.error_icon
                          : "info" === this.options.type
                          ? this.styles.info_icon
                          : "success" === this.options.type
                          ? this.styles.success_icon
                          : this.styles.notice_icon
                        : this.options.icon,
                  })
                )
                .prependTo(this.container);
        this.options.width !== a.width &&
          this.elem.animate({ width: this.options.width });
        this.options.min_height !== a.min_height &&
          this.container.animate({ minHeight: this.options.min_height });
        this.options.hide ? a.hide || this.queueRemove() : this.cancelRemove();
        this.queuePosition(!0);
        this.runModules("update", a);
        return this;
      },
      open: function () {
        this.state = "opening";
        this.runModules("beforeOpen");
        var c = this;
        this.elem.parent().length ||
          this.elem.appendTo(
            this.options.stack.context ? this.options.stack.context : h
          );
        "top" !== this.options.stack.push && this.position(!0);
        this.animateIn(function () {
          c.queuePosition(!0);
          c.options.hide && c.queueRemove();
          c.state = "open";
          c.runModules("afterOpen");
        });
        return this;
      },
      remove: function (c) {
        this.state = "closing";
        this.timerHide = !!c;
        this.runModules("beforeClose");
        var a = this;
        this.timer && (l.clearTimeout(this.timer), (this.timer = null));
        this.animateOut(function () {
          a.state = "closed";
          a.runModules("afterClose");
          a.queuePosition(!0);
          a.options.remove && a.elem.detach();
          a.runModules("beforeDestroy");
          if (a.options.destroy && null !== d.notices) {
            var c = b.inArray(a, d.notices);
            -1 !== c && d.notices.splice(c, 1);
          }
          a.runModules("afterDestroy");
        });
        return this;
      },
      get: function () {
        return this.elem;
      },
      parseOptions: function (c, a) {
        this.options = b.extend(!0, {}, d.prototype.options);
        this.options.stack = d.prototype.options.stack;
        for (var p = [c, a], m, f = 0; f < p.length; f++) {
          m = p[f];
          if ("undefined" === typeof m) break;
          if ("object" !== typeof m) this.options.text = m;
          else
            for (var e in m)
              this.modules[e]
                ? b.extend(!0, this.options[e], m[e])
                : (this.options[e] = m[e]);
        }
      },
      animateIn: function (c) {
        this.animating = "in";
        var a = this;
        c = function () {
          a.animTimer && clearTimeout(a.animTimer);
          "in" === a.animating &&
            (a.elem.is(":visible")
              ? (this && this.call(), (a.animating = !1))
              : (a.animTimer = setTimeout(c, 40)));
        }.bind(c);
        "fade" === this.options.animation
          ? (this.elem
              .one(
                "webkitTransitionEnd mozTransitionEnd MSTransitionEnd oTransitionEnd transitionend",
                c
              )
              .addClass("ui-pnotify-in"),
            this.elem.css("opacity"),
            this.elem.addClass("ui-pnotify-fade-in"),
            (this.animTimer = setTimeout(c, 650)))
          : (this.elem.addClass("ui-pnotify-in"), c());
      },
      animateOut: function (c) {
        this.animating = "out";
        var a = this;
        c = function () {
          a.animTimer && clearTimeout(a.animTimer);
          "out" === a.animating &&
            ("0" != a.elem.css("opacity") && a.elem.is(":visible")
              ? (a.animTimer = setTimeout(c, 40))
              : (a.elem.removeClass("ui-pnotify-in"),
                this && this.call(),
                (a.animating = !1)));
        }.bind(c);
        "fade" === this.options.animation
          ? (this.elem
              .one(
                "webkitTransitionEnd mozTransitionEnd MSTransitionEnd oTransitionEnd transitionend",
                c
              )
              .removeClass("ui-pnotify-fade-in"),
            (this.animTimer = setTimeout(c, 650)))
          : (this.elem.removeClass("ui-pnotify-in"), c());
      },
      position: function (c) {
        var a = this.options.stack,
          b = this.elem;
        "undefined" === typeof a.context && (a.context = h);
        if (a) {
          "number" !== typeof a.nextpos1 && (a.nextpos1 = a.firstpos1);
          "number" !== typeof a.nextpos2 && (a.nextpos2 = a.firstpos2);
          "number" !== typeof a.addpos2 && (a.addpos2 = 0);
          var d = !b.hasClass("ui-pnotify-in");
          if (!d || c) {
            a.modal && (a.overlay ? a.overlay.show() : (a.overlay = s(a)));
            b.addClass("ui-pnotify-move");
            var f;
            switch (a.dir1) {
              case "down":
                f = "top";
                break;
              case "up":
                f = "bottom";
                break;
              case "left":
                f = "right";
                break;
              case "right":
                f = "left";
            }
            c = parseInt(b.css(f).replace(/(?:\..*|[^0-9.])/g, ""));
            isNaN(c) && (c = 0);
            "undefined" !== typeof a.firstpos1 ||
              d ||
              ((a.firstpos1 = c), (a.nextpos1 = a.firstpos1));
            var e;
            switch (a.dir2) {
              case "down":
                e = "top";
                break;
              case "up":
                e = "bottom";
                break;
              case "left":
                e = "right";
                break;
              case "right":
                e = "left";
            }
            c = parseInt(b.css(e).replace(/(?:\..*|[^0-9.])/g, ""));
            isNaN(c) && (c = 0);
            "undefined" !== typeof a.firstpos2 ||
              d ||
              ((a.firstpos2 = c), (a.nextpos2 = a.firstpos2));
            if (
              ("down" === a.dir1 &&
                a.nextpos1 + b.height() >
                  (a.context.is(h)
                    ? n.height()
                    : a.context.prop("scrollHeight"))) ||
              ("up" === a.dir1 &&
                a.nextpos1 + b.height() >
                  (a.context.is(h)
                    ? n.height()
                    : a.context.prop("scrollHeight"))) ||
              ("left" === a.dir1 &&
                a.nextpos1 + b.width() >
                  (a.context.is(h)
                    ? n.width()
                    : a.context.prop("scrollWidth"))) ||
              ("right" === a.dir1 &&
                a.nextpos1 + b.width() >
                  (a.context.is(h) ? n.width() : a.context.prop("scrollWidth")))
            )
              (a.nextpos1 = a.firstpos1),
                (a.nextpos2 +=
                  a.addpos2 +
                  ("undefined" === typeof a.spacing2 ? 25 : a.spacing2)),
                (a.addpos2 = 0);
            "number" === typeof a.nextpos2 &&
              (a.animation
                ? b.css(e, a.nextpos2 + "px")
                : (b.removeClass("ui-pnotify-move"),
                  b.css(e, a.nextpos2 + "px"),
                  b.css(e),
                  b.addClass("ui-pnotify-move")));
            switch (a.dir2) {
              case "down":
              case "up":
                b.outerHeight(!0) > a.addpos2 && (a.addpos2 = b.height());
                break;
              case "left":
              case "right":
                b.outerWidth(!0) > a.addpos2 && (a.addpos2 = b.width());
            }
            "number" === typeof a.nextpos1 &&
              (a.animation
                ? b.css(f, a.nextpos1 + "px")
                : (b.removeClass("ui-pnotify-move"),
                  b.css(f, a.nextpos1 + "px"),
                  b.css(f),
                  b.addClass("ui-pnotify-move")));
            switch (a.dir1) {
              case "down":
              case "up":
                a.nextpos1 +=
                  b.height() +
                  ("undefined" === typeof a.spacing1 ? 25 : a.spacing1);
                break;
              case "left":
              case "right":
                a.nextpos1 +=
                  b.width() +
                  ("undefined" === typeof a.spacing1 ? 25 : a.spacing1);
            }
          }
          return this;
        }
      },
      queuePosition: function (b, a) {
        g && clearTimeout(g);
        a || (a = 10);
        g = setTimeout(function () {
          d.positionAll(b);
        }, a);
        return this;
      },
      cancelRemove: function () {
        this.timer && l.clearTimeout(this.timer);
        this.animTimer && l.clearTimeout(this.animTimer);
        "closing" === this.state &&
          ((this.state = "open"),
          (this.animating = !1),
          this.elem.addClass("ui-pnotify-in"),
          "fade" === this.options.animation &&
            this.elem.addClass("ui-pnotify-fade-in"));
        return this;
      },
      queueRemove: function () {
        var b = this;
        this.cancelRemove();
        this.timer = l.setTimeout(
          function () {
            b.remove(!0);
          },
          isNaN(this.options.delay) ? 0 : this.options.delay
        );
        return this;
      },
    });
    b.extend(d, {
      notices: [],
      reload: q,
      removeAll: function () {
        b.each(d.notices, function () {
          this.remove && this.remove(!1);
        });
      },
      removeStack: function (c) {
        b.each(d.notices, function () {
          this.remove && this.options.stack === c && this.remove(!1);
        });
      },
      positionAll: function (c) {
        g && clearTimeout(g);
        g = null;
        if (d.notices && d.notices.length)
          b.each(d.notices, function () {
            var a = this.options.stack;
            a &&
              (a.overlay && a.overlay.hide(),
              (a.nextpos1 = a.firstpos1),
              (a.nextpos2 = a.firstpos2),
              (a.addpos2 = 0),
              (a.animation = c));
          }),
            b.each(d.notices, function () {
              this.position();
            });
        else {
          var a = d.prototype.options.stack;
          a && (delete a.nextpos1, delete a.nextpos2);
        }
      },
      styling: {
        brighttheme: {
          container: "brighttheme",
          notice: "brighttheme-notice",
          notice_icon: "brighttheme-icon-notice",
          info: "brighttheme-info",
          info_icon: "brighttheme-icon-info",
          success: "brighttheme-success",
          success_icon: "brighttheme-icon-success",
          error: "brighttheme-error",
          error_icon: "brighttheme-icon-error",
        },
        jqueryui: {
          container: "ui-widget ui-widget-content ui-corner-all",
          notice: "ui-state-highlight",
          notice_icon: "ui-icon ui-icon-info",
          info: "",
          info_icon: "ui-icon ui-icon-info",
          success: "ui-state-default",
          success_icon: "ui-icon ui-icon-circle-check",
          error: "ui-state-error",
          error_icon: "ui-icon ui-icon-alert",
        },
        bootstrap3: {
          container: "alert",
          notice: "alert-warning",
          notice_icon: "glyphicon glyphicon-exclamation-sign",
          info: "alert-info",
          info_icon: "glyphicon glyphicon-info-sign",
          success: "alert-success",
          success_icon: "glyphicon glyphicon-ok-sign",
          error: "alert-danger",
          error_icon: "glyphicon glyphicon-warning-sign",
        },
      },
    });
    d.styling.fontawesome = b.extend({}, d.styling.bootstrap3);
    b.extend(d.styling.fontawesome, {
      notice_icon: "fa fa-exclamation-circle",
      info_icon: "fa fa-info",
      success_icon: "fa fa-check",
      error_icon: "fa fa-warning",
    });
    l.document.body ? r() : b(r);
    return d;
  };
  return q(k);
});
