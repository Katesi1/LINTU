(function (d, e) {
  "function" === typeof define && define.amd
    ? define("pnotify.buttons", ["jquery", "pnotify"], e)
    : "object" === typeof exports && "undefined" !== typeof module
    ? (module.exports = e(require("jquery"), require("./pnotify")))
    : e(d.jQuery, d.PNotify);
})(this, function (d, e) {
  e.prototype.options.buttons = {
    closer: !0,
    closer_hover: !0,
    sticker: !0,
    sticker_hover: !0,
    show_on_nonblock: !1,
    labels: { close: "Close", stick: "Stick", unstick: "Unstick" },
    classes: { closer: null, pin_up: null, pin_down: null },
  };
  e.prototype.modules.buttons = {
    closer: null,
    sticker: null,
    init: function (a, b) {
      var c = this;
      a.elem.on({
        mouseenter: function (b) {
          !c.options.sticker ||
            (a.options.nonblock &&
              a.options.nonblock.nonblock &&
              !c.options.show_on_nonblock) ||
            c.sticker
              .trigger("pnotify:buttons:toggleStick")
              .css("visibility", "visible");
          !c.options.closer ||
            (a.options.nonblock &&
              a.options.nonblock.nonblock &&
              !c.options.show_on_nonblock) ||
            c.closer.css("visibility", "visible");
        },
        mouseleave: function (a) {
          c.options.sticker_hover && c.sticker.css("visibility", "hidden");
          c.options.closer_hover && c.closer.css("visibility", "hidden");
        },
      });
      this.sticker = d("<div />", {
        class: "ui-pnotify-sticker",
        "aria-role": "button",
        "aria-pressed": a.options.hide ? "false" : "true",
        tabindex: "0",
        title: a.options.hide ? b.labels.stick : b.labels.unstick,
        css: {
          cursor: "pointer",
          visibility: b.sticker_hover ? "hidden" : "visible",
        },
        click: function () {
          a.options.hide = !a.options.hide;
          a.options.hide ? a.queueRemove() : a.cancelRemove();
          d(this).trigger("pnotify:buttons:toggleStick");
        },
      })
        .bind("pnotify:buttons:toggleStick", function () {
          var b =
              null === c.options.classes.pin_up
                ? a.styles.pin_up
                : c.options.classes.pin_up,
            e =
              null === c.options.classes.pin_down
                ? a.styles.pin_down
                : c.options.classes.pin_down;
          d(this)
            .attr(
              "title",
              a.options.hide ? c.options.labels.stick : c.options.labels.unstick
            )
            .children()
            .attr("class", "")
            .addClass(a.options.hide ? b : e)
            .attr("aria-pressed", a.options.hide ? "false" : "true");
        })
        .append("<span />")
        .trigger("pnotify:buttons:toggleStick")
        .prependTo(a.container);
      (!b.sticker ||
        (a.options.nonblock &&
          a.options.nonblock.nonblock &&
          !b.show_on_nonblock)) &&
        this.sticker.css("display", "none");
      this.closer = d("<div />", {
        class: "ui-pnotify-closer",
        "aria-role": "button",
        tabindex: "0",
        title: b.labels.close,
        css: {
          cursor: "pointer",
          visibility: b.closer_hover ? "hidden" : "visible",
        },
        click: function () {
          a.remove(!1);
          c.sticker.css("visibility", "hidden");
          c.closer.css("visibility", "hidden");
        },
      })
        .append(
          d("<span />", {
            class:
              null === b.classes.closer ? a.styles.closer : b.classes.closer,
          })
        )
        .prependTo(a.container);
      (!b.closer ||
        (a.options.nonblock &&
          a.options.nonblock.nonblock &&
          !b.show_on_nonblock)) &&
        this.closer.css("display", "none");
    },
    update: function (a, b) {
      !b.closer ||
      (a.options.nonblock && a.options.nonblock.nonblock && !b.show_on_nonblock)
        ? this.closer.css("display", "none")
        : b.closer && this.closer.css("display", "block");
      !b.sticker ||
      (a.options.nonblock && a.options.nonblock.nonblock && !b.show_on_nonblock)
        ? this.sticker.css("display", "none")
        : b.sticker && this.sticker.css("display", "block");
      this.sticker.trigger("pnotify:buttons:toggleStick");
      this.closer
        .find("span")
        .attr("class", "")
        .addClass(
          null === b.classes.closer ? a.styles.closer : b.classes.closer
        );
      b.sticker_hover
        ? this.sticker.css("visibility", "hidden")
        : (a.options.nonblock &&
            a.options.nonblock.nonblock &&
            !b.show_on_nonblock) ||
          this.sticker.css("visibility", "visible");
      b.closer_hover
        ? this.closer.css("visibility", "hidden")
        : (a.options.nonblock &&
            a.options.nonblock.nonblock &&
            !b.show_on_nonblock) ||
          this.closer.css("visibility", "visible");
    },
  };
  d.extend(e.styling.brighttheme, {
    closer: "brighttheme-icon-closer",
    pin_up: "brighttheme-icon-sticker",
    pin_down: "brighttheme-icon-sticker brighttheme-icon-stuck",
  });
  d.extend(e.styling.jqueryui, {
    closer: "ui-icon ui-icon-close",
    pin_up: "ui-icon ui-icon-pin-w",
    pin_down: "ui-icon ui-icon-pin-s",
  });
  d.extend(e.styling.bootstrap2, {
    closer: "icon-remove",
    pin_up: "icon-pause",
    pin_down: "icon-play",
  });
  d.extend(e.styling.bootstrap3, {
    closer: "glyphicon glyphicon-remove",
    pin_up: "glyphicon glyphicon-pause",
    pin_down: "glyphicon glyphicon-play",
  });
  d.extend(e.styling.fontawesome, {
    closer: "fa fa-times",
    pin_up: "fa fa-pause",
    pin_down: "fa fa-play",
  });
});
