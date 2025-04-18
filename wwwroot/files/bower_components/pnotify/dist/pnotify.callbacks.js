(function (b, a) {
  "function" === typeof define && define.amd
    ? define("pnotify.callbacks", ["jquery", "pnotify"], a)
    : "object" === typeof exports && "undefined" !== typeof module
    ? (module.exports = a(require("jquery"), require("./pnotify")))
    : a(b.jQuery, b.PNotify);
})(this, function (b, a) {
  var c = a.prototype.init,
    d = a.prototype.open,
    e = a.prototype.remove;
  a.prototype.init = function () {
    this.options.before_init && this.options.before_init(this.options);
    c.apply(this, arguments);
    this.options.after_init && this.options.after_init(this);
  };
  a.prototype.open = function () {
    var a;
    this.options.before_open && (a = this.options.before_open(this));
    !1 !== a &&
      (d.apply(this, arguments),
      this.options.after_open && this.options.after_open(this));
  };
  a.prototype.remove = function (a) {
    var b;
    this.options.before_close && (b = this.options.before_close(this, a));
    !1 !== b &&
      (e.apply(this, arguments),
      this.options.after_close && this.options.after_close(this, a));
  };
});
